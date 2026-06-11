using System.Text;

namespace NotePad.Services;

public static class FileService
{
    public static string? ShowOpenDialog(IWin32Window owner)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Text Files|*.txt|All Files|*.*",
            Title = "Open File"
        };

        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
    }

    public static string? ShowSaveDialog(IWin32Window owner, string defaultName = "Untitled.txt")
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Text Files|*.txt|All Files|*.*",
            Title = "Save File",
            FileName = defaultName
        };

        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
    }

    public static string ReadFile(string path)
    {
        return File.ReadAllText(path, Encoding.UTF8);
    }

    public static void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content, Encoding.UTF8);
    }
}
