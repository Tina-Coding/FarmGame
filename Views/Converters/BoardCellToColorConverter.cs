using SUP.Views.Game;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SUP.Views.Converters;

public class BoardCellToColorConverter : IValueConverter
{
    public readonly static ImageSource Ground = LoadImage("ground.png");
    public readonly static ImageSource Bush = LoadImage("backgroundBush.png");
    public readonly static ImageSource Farmer = LoadImage("farmer.png");
    public readonly static ImageSource Chicken = LoadImage("chicken.png");
    public readonly static ImageSource PigDown = LoadImage("pigDown.png");
    public readonly static ImageSource PigRight = LoadImage("pigRight.png");
    public readonly static ImageSource PigUp = LoadImage("pigUp.png");
    public readonly static ImageSource PigLeft = LoadImage("pigLeft.png");

    private static ImageSource LoadImage(string filename)
    {
        //https://stackoverflow.com/questions/5982625/loading-images-in-wpf-from-code

        return new BitmapImage(new Uri($"pack://application:,,,/Assets/Images/{filename}", UriKind.Absolute));
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            BoardCellEnum.Wall => Bush,
            BoardCellEnum.Corridor => Ground,
            BoardCellEnum.Farmer => Farmer,
            BoardCellEnum.Chicken => Chicken,
            BoardCellEnum.PigUp => PigUp,
            BoardCellEnum.PigDown => PigDown,
            BoardCellEnum.PigLeft => PigLeft,
            BoardCellEnum.PigRight => PigRight,
            _ => throw new NotImplementedException(),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
