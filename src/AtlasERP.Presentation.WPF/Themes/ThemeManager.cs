using System.Windows;

namespace AtlasERP.Presentation.WPF.Themes;

/// <summary>
/// Swaps the theme ResourceDictionary at runtime. Relies on App.xaml merging
/// Generic.xaml first and a theme file (Light.xaml by default) second — this only ever
/// replaces that second slot, so control styles in Generic.xaml stay untouched.
/// </summary>
public static class ThemeManager
{
    private const int ThemeDictionaryIndex = 1;

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static event EventHandler<AppTheme>? ThemeChanged;

    public static void ApplyTheme(AppTheme theme)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var themeDictionary = new ResourceDictionary
        {
            Source = new Uri($"Themes/{theme}.xaml", UriKind.Relative)
        };

        if (dictionaries.Count > ThemeDictionaryIndex)
        {
            dictionaries[ThemeDictionaryIndex] = themeDictionary;
        }
        else
        {
            dictionaries.Add(themeDictionary);
        }

        CurrentTheme = theme;
        ThemeChanged?.Invoke(null, theme);
    }

    public static void ToggleTheme()
    {
        ApplyTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }
}
