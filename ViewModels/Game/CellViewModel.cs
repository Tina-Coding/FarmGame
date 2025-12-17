using PropertyChanged;
using SUP.Views.Game;


namespace SUP.ViewModels.Game;

[AddINotifyPropertyChangedInterface]
public class CellViewModel
{
    public BoardCellEnum CellType { get; set; }
}
