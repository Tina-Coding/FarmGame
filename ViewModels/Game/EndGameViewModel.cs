using PropertyChanged;
using SUP.Commands;
using SUP.Models;
using SUP.Services;
using SUP.ViewModels.Shell;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SUP.ViewModels.Game;

[AddINotifyPropertyChangedInterface]
public class EndGameViewModel
{

    private readonly MainShellViewModel MainShellVM;
    public ICommand TryAgainCommand { get; set; }
    public ICommand SaveScoreCommand { get; set; }
    public ICommand BackToStartCommand { get; set; }
    public string NickName { get; set; }
    IGameSessionService _sessionService;
    public int Score { get; set; }
    public string ErrorMessage { get; set; }
    private string ScoreCode = "HIGHSCORE"; // Använder oss av Highscore som code i databasen
    public string ResultText { get; set; } = "Game Over";
    public ObservableCollection<PlayerWithScore> PlayerHighScores { get; set; } = new ObservableCollection<PlayerWithScore>(); // istället för new list
    public readonly IGameHubDbService _db;
    public ImageSource EndPicture { get; set; }

    // Alternativ lösning till BackToStartCommand
    //public Action BackToStart; 

    public EndGameViewModel(MainShellViewModel mainShellVM, IGameHubDbService db, ICommand tryAgainCommand, IGameSessionService sessionSvc, ICommand backtostart)
    {

        MainShellVM = mainShellVM;
        _sessionService = sessionSvc;
        SaveScoreCommand = new RelayCommand(async _ => await SavePlayerAsync());
        BackToStartCommand = backtostart;
        _db = db;
        GetHighScoresAsync();
        TryAgainCommand = tryAgainCommand;

    }
    /// <summary>
    /// Hämtar highscores från databasen och lägger till en lista som visas i vyn
    /// </summary>
    /// <returns></returns>
    public async Task GetHighScoresAsync()
    {
        PlayerHighScores.Clear();
        List<PlayerWithScore> highScores = await _db.GetHighScores();

        foreach (PlayerWithScore player in highScores)
        {
            PlayerHighScores.Add(player);
        }
    }
    /// <summary>
    /// Funktion som använder sessionservice för att spara resultatet till databasen, uppdaterar highscorelistan genom att hämta den nya listan
    /// </summary>
    /// <returns></returns>
    private async Task SavePlayerAsync()
    {
        try
        {
            PlayerWithScore player = await _sessionService.EnsurePlayerAsync(NickName);
            await _sessionService.SaveResultAsync(ScoreCode, Score);
            MessageBox.Show($"Spelare sparad: {NickName}. Poäng {Score}");
            GetHighScoresAsync();
            NickName = String.Empty;
        }
        catch (Exception e)
        {
            if (NickName == null || NickName == string.Empty)
            {
                MessageBox.Show("Skriv in ditt användarnamn");
            }
            else
            {
                MessageBox.Show($"Fel {e}");
            }
        }
    }
}


