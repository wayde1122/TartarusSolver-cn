using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Tartarus.App.Presentation;

/// <summary>Projects an existing status string into a display label; never changes the source.</summary>
public sealed class DisplayTextConverter : IValueConverter
{
    public string Pattern { get; set; } = "";
    public string Fallback { get; set; } = "";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text) return Fallback;
        var match = Regex.Match(text, Pattern);
        return match.Success ? match.Groups[1].Value : Fallback;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
