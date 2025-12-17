using PropertyChanged;
using SUP.Commands;
using SUP.Services;
using SUP.ViewModels.Game;
namespace SUP.ViewModels.Shell;

[AddINotifyPropertyChangedInterface]
public class MainShellViewModel
{
    #region Audio
    private void OnAudioVolumeChanged() { _audio.SetMusicVolume((float)AudioVolume); _audio.SetSfxVolume((float)AudioVolume); }
    private void OnAudioMutedChanged() { _audio.SetMusicMuted(AudioMuted); _audio.SetSfxMuted(AudioMuted); }
    public double AudioVolume { get; set; } = 0.25;
    public bool AudioMuted { get; set; } = false;
    private IAudioService _audio;
    #endregion


    #region ViewModels
    public GameBoardViewModel GameBoardVM { get; set; }
    public StartGameViewModel StartGameVM { get; set; }
    public EndGameViewModel EndGameVM { get; set; }

    #endregion

    public object Screen { get; set; }
    private IGameSessionService _sessionSvc;
    private string _gameName = "Farm Escape";
    private IGameHubDbService _db;

    public MainShellViewModel(IGameHubDbService db, IAudioService audio, IGameSessionService sessionSvc)
    {
        _audio = audio;
        _db = db;
        _sessionSvc = sessionSvc;
        GameBoardVM = new GameBoardViewModel(_audio);
        StartGameVM = new StartGameViewModel(new RelayCommand(async _ => await StartAsync()), _gameName);
        EndGameVM = new EndGameViewModel(this, _db, new RelayCommand(async _ => await StartAsync()), _sessionSvc, new RelayCommand(async async => await BackToStartAsync()));

        ApplyAudioSettings();
        _audio.SetMusicLoop("Assets/Sounds/banjo.mp3", autoPlay: true);

        // https://www.youtube.com/watch?v=e4G8VgqdaD4&t=1s
        GameBoardVM.GameOver += OnGameOver;
        Screen = StartGameVM;
    }

    private void ApplyAudioSettings()
    {
        _audio.SetMusicVolume((float)AudioVolume);
        _audio.SetSfxVolume((float)AudioVolume);
        _audio.SetMusicMuted(AudioMuted);
        _audio.SetSfxMuted(AudioMuted);
    }
    /// <summary>
    /// Skickar med poängen så de syns på gameover sidan, och byter skärm till gameoverview.
    /// </summary>
    public void OnGameOver()
    {
        EndGameVM.Score = GameBoardVM.Score;          
        Screen = EndGameVM;
    }
    public async Task StartAsync()
    {
        Screen = GameBoardVM;
        await _sessionSvc.SetSessionIdAsync(_gameName);
        _audio.StopMusic();
        await GameBoardVM.StartNewGameAsync();
    }
    public async Task BackToStartAsync()
    {
        Screen = StartGameVM;
    }

}
