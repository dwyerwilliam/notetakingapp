namespace NotePad.Controls;

public class StatusBar : UserControl
{
    private readonly ToolStripStatusLabel _lineLabel;
    private readonly ToolStripStatusLabel _charLabel;
    private readonly ToolStripStatusLabel _statusLabel;

    public StatusBar()
    {
        var strip = new StatusStrip
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(2, 0, 2, 0)
        };

        _lineLabel = new ToolStripStatusLabel("Lines: 1")
        {
            Spring = false
        };
        _charLabel = new ToolStripStatusLabel("Chars: 0")
        {
            Spring = false
        };
        _statusLabel = new ToolStripStatusLabel("Ready")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleRight
        };

        strip.Items.AddRange(new ToolStripItem[] { _lineLabel, _charLabel, _statusLabel });
        Controls.Add(strip);
    }

    public void UpdateCounts(int lines, int chars)
    {
        _lineLabel.Text = $"Lines: {lines}";
        _charLabel.Text = $"Chars: {chars}";
    }

    public void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
    }
}
