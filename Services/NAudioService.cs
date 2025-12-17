using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.IO;

namespace SUP.Services;
// Källa: Eriks föreläsning 
public sealed class NAudioService : IAudioService, IDisposable
{
    // ===== SFX (effekter) =====
    private readonly IWavePlayer _sfxOut;
    private readonly MixingSampleProvider _sfxMixer;
    private readonly Dictionary<string, CachedSound> _effectSoundCache = new();
    private readonly object _sfxLock = new();

    // Mixer-target format (float 44.1kHz stereo)
    private static readonly WaveFormat MixerFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    // ===== Musik =====
    private IWavePlayer? _musicOut;
    private AudioFileReader? _musicReader;             // äger filströmmen
    private VolumeSampleProvider? _musicVolumeNode;    // volymnod ovanpå loop
    private readonly object _musicLock = new();
    private float _audioVolume = 0.25f;
    private bool _audioMuted = false;

    public NAudioService()
    {
        _sfxMixer = new MixingSampleProvider(MixerFormat) { ReadFully = true };
        _sfxOut = new WaveOutEvent();
        _sfxOut.Init(_sfxMixer);
        _sfxOut.Play();
    }

    // ============== MUSIK ==============

    public void SetMusicLoop(string path, bool autoPlay = false)
    {
        lock (_musicLock)
        {
            // Städa tidigare
            _musicOut?.Stop();
            _musicReader?.Dispose();
            _musicOut?.Dispose();
            _musicReader = null; _musicOut = null; _musicVolumeNode = null;

            var fullPath = ResolvePath(path);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Hittade inte filen: {fullPath}");

            // Ladda fil -> loop -> volymnod
            _musicReader = new AudioFileReader(fullPath);
            ISampleProvider loop = new LoopStream(_musicReader);


            loop = NormalizeToMixer(loop);

            _musicVolumeNode = new VolumeSampleProvider(loop);
            _musicOut = new WaveOutEvent();
            _musicOut.Init(_musicVolumeNode);
            ApplyMusicGain_NoLock();

            if (autoPlay) _musicOut.Play();
            else _musicOut.Pause(); // buffrad & redo men tyst tills ResumeMusic()
        }
    }
    // Detta startar musiken både från början och efter paus
    public void ResumeMusic()
    {
        lock (_musicLock)
        {
            if (_musicOut != null)
            {
                ApplyMusicGain_NoLock();
                _musicOut.Play();
            }
        }
    }

    public void PauseMusic()
    {
        lock (_musicLock) { _musicOut?.Pause(); }
    }

    public void StopMusic()
    {
        lock (_musicLock)
        {
            try { _musicOut?.Stop(); } catch { }
            _musicReader?.Dispose();
            _musicOut?.Dispose();
            _musicReader = null; _musicOut = null; _musicVolumeNode = null;
        }
    }

    public async Task StopMusicAsync(TimeSpan fadeOut)
    {
        VolumeSampleProvider? node;
        IWavePlayer? player;
        lock (_musicLock) { node = _musicVolumeNode; player = _musicOut; }

        if (player == null || node == null) { StopMusic(); return; }

        var steps = Math.Max(4, (int)(fadeOut.TotalMilliseconds / 20));
        float startVol; lock (_musicLock) { startVol = _audioMuted ? 0f : _audioVolume; }
        for (int i = steps; i >= 0; i--)
        {
            var v = startVol * (i / (float)steps);
            lock (_musicLock) { if (_musicVolumeNode != null) _musicVolumeNode.Volume = v; }
            await Task.Delay(20);
        }
        StopMusic();
    }

    public void SetMusicVolume(float v)
    {
        lock (_musicLock)
        {
            _audioVolume = Math.Clamp(v, 0f, 1f);
            ApplyMusicGain_NoLock();
        }
    }

    public void SetMusicMuted(bool muted)
    {
        lock (_musicLock)
        {
            _audioMuted = muted;
            ApplyMusicGain_NoLock();
        }
    }

    private void ApplyMusicGain_NoLock()
    {
        if (_musicVolumeNode != null)
            _musicVolumeNode.Volume = _audioMuted ? 0f : _audioVolume;
    }

    // ============== SFX ==============

    public void LoadSfx(IDictionary<string, string> files, bool clearExisting = true)
    {
        lock (_sfxLock)
        {
            if (clearExisting) _effectSoundCache.Clear();

            foreach (var (key, p) in files)
            {
                if (!File.Exists(p)) continue;
                _effectSoundCache[key] = new CachedSound(p);
            }
        }
    }

    public void ClearSfx()
    {
        lock (_sfxLock) _effectSoundCache.Clear();
    }

    public void SetSfxVolume(float v)
    {
        lock (_sfxLock) _audioVolume = Math.Clamp(v, 0f, 1f);
    }

    public void SetSfxMuted(bool muted)
    {
        lock (_sfxLock) _audioMuted = muted;
    }

    public void PlaySfx(string key)
    {
        CachedSound? snd;
        lock (_sfxLock) _effectSoundCache.TryGetValue(key, out snd);
        if (snd == null) return;

        // Skapa kedja: cached → (mono→stereo) → (resample) → volym → mixer
        ISampleProvider src = new CachedSoundSampleProvider(snd);

        if (src.WaveFormat.Channels == 1)
            src = new MonoToStereoSampleProvider(src);

        if (src.WaveFormat.SampleRate != MixerFormat.SampleRate)
            src = new WdlResamplingSampleProvider(src, MixerFormat.SampleRate);

        // volym per spelning (tar hänsyn till global SFX-vol/mute)
        float vol; bool muted;
        lock (_sfxLock) { vol = _audioVolume; muted = _audioMuted; }
        src = new VolumeSampleProvider(src) { Volume = muted ? 0f : vol };

        // Lägg in i mixern (tråd-säkert i praktiken)
        _sfxMixer.AddMixerInput(src);
    }

    // ============== Hjälpmetoder ==============

    private static string ResolvePath(string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

    private static ISampleProvider NormalizeToMixer(ISampleProvider src)
    {
        ISampleProvider current = src;

        // till stereo
        if (current.WaveFormat.Channels == 1)
            current = new MonoToStereoSampleProvider(current);

        // till 44.1k
        if (current.WaveFormat.SampleRate != MixerFormat.SampleRate)
            current = new WdlResamplingSampleProvider(current, MixerFormat.SampleRate);

        // MixerFormat är float stereo → vi är rätt nu
        return current;
    }

    public void Dispose()
    {
        StopMusic();
        _sfxOut?.Stop();
        _sfxOut?.Dispose();
    }

    // ============== In-memory cacheklasser ==============

    private sealed class CachedSound
    {
        public float[] AudioData { get; }
        public WaveFormat WaveFormat { get; }

        public CachedSound(string file)
        {
            using var afr = new AudioFileReader(file); // float 32
            WaveFormat = afr.WaveFormat;
            var list = new List<float>();
            var buf = new float[afr.WaveFormat.SampleRate * afr.WaveFormat.Channels];
            int read;
            while ((read = afr.Read(buf, 0, buf.Length)) > 0)
                list.AddRange(buf.AsSpan(0, read).ToArray());
            AudioData = list.ToArray();
        }
    }

    private sealed class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound _source;
        private long _pos;
        public CachedSoundSampleProvider(CachedSound src) { _source = src; }
        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            var remaining = _source.AudioData.Length - _pos;
            var toCopy = (int)Math.Min(remaining, count);
            if (toCopy > 0)
            {
                Array.Copy(_source.AudioData, _pos, buffer, offset, toCopy);
                _pos += toCopy;
            }
            return toCopy;
        }
    }

    // Loopar källan (AudioFileReader) från början när den tar slut
    private sealed class LoopStream : ISampleProvider
    {
        private readonly ISampleProvider _src;
        public LoopStream(ISampleProvider src) { _src = src; }
        public WaveFormat WaveFormat => _src.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = _src.Read(buffer, offset + total, count - total);
                if (read == 0)
                {
                    if (_src is AudioFileReader afr) { afr.Position = 0; continue; }
                    break;
                }
                total += read;
            }
            return total;
        }
    }
}
