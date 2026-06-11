namespace NotePad;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        string? filePath = null;
        if (args.Length > 0 && File.Exists(args[0]))
            filePath = args[0];

        Application.Run(new MainForm(filePath));
    }
}
