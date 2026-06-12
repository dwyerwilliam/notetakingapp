using Microsoft.Win32;
using System.Windows.Forms;

namespace NotePad;

public static class ThemeHelper
{
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string RegistryKey = "AppsUseLightTheme";

    public static event EventHandler? ThemeChanged;
    private static bool? _cachedDarkMode;

    public static bool IsDarkMode => _cachedDarkMode ??= DetectDarkMode();

    public static void Initialize()
    {
#pragma warning disable WFO5001
        Application.SetColorMode(SystemColorMode.System);
#pragma warning restore WFO5001

        _cachedDarkMode = DetectDarkMode();

        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    private static bool DetectDarkMode()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(RegistryKey);
            if (value is int intValue)
                return intValue == 0;
        }
        catch
        {
        }

        return false;
    }

    private static void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category != UserPreferenceCategory.General)
            return;

        bool newDarkMode = DetectDarkMode();
        if (newDarkMode != _cachedDarkMode)
        {
            _cachedDarkMode = newDarkMode;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static void ApplyTheme(Form form)
    {
        form.BackColor = SystemColors.Window;
        form.ForeColor = SystemColors.WindowText;

        foreach (Control control in form.Controls)
        {
            ApplyThemeToControl(control);
        }
    }

    private static void ApplyThemeToControl(Control control)
    {
        control.BackColor = SystemColors.Window;
        control.ForeColor = SystemColors.WindowText;

        if (control is RichTextBox richTextBox)
        {
            richTextBox.BackColor = SystemColors.Window;
            richTextBox.ForeColor = SystemColors.WindowText;
        }

        foreach (Control child in control.Controls)
        {
            ApplyThemeToControl(child);
        }
    }
}
