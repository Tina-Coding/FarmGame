using PropertyChanged;
using System.Windows.Input;
namespace SUP.ViewModels.Game;

[AddINotifyPropertyChangedInterface]

public class StartGameViewModel
{
    public string GameTitle { get; set; }
    public ICommand StartCommand { get; set; }
    public int HighScore { get; set; }

    public StartGameViewModel(ICommand startnewgame, string title)
    {
        StartCommand = startnewgame;
        GameTitle = title;
       
    }
}
