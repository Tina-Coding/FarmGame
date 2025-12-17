namespace SUP.Services;

public interface IAudioService : IDisposable
{
    // Musik
    void SetMusicLoop(string path, bool autoPlay = false);
    void ResumeMusic();
    void PauseMusic();
    void StopMusic();
    Task StopMusicAsync(TimeSpan fadeOut);

    // Volym/mute
    void SetMusicVolume(float v);
    void SetSfxVolume(float v);
    void SetMusicMuted(bool muted);
    void SetSfxMuted(bool muted);

    // SFX
    void LoadSfx(IDictionary<string, string> files, bool clearExisting = true);
    void ClearSfx();
    void PlaySfx(string key);

}
