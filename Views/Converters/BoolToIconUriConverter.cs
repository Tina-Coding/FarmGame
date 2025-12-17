using PropertyChanged;
using System.Globalization;
using System.Windows.Data;

namespace SUP.Views.Converters;

[AddINotifyPropertyChangedInterface]
public class BoolToIconUriConverter : IValueConverter
{
    public string OnUri { get; set; }
    public string OffUri { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isMuted) return Binding.DoNothing;

        else
        {
            if (isMuted)
            {
                return new Uri(OffUri, UriKind.Relative);
            }
            else
            {
                return new Uri(OnUri, UriKind.Relative);
            }
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
