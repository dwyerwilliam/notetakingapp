namespace NotePad.Controls;

public class StatusBar : UserControl
{
    private readonly StatusStrip _strip;
    private readonly ToolStripStatusLabel _lineLabel;
    private readonly ToolStripStatusLabel _charLabel;
    private readonly ToolStripStatusLabel _statusLabel;

    public StatusBar()
    {
        Dock = DockStyle.Bottom;

        _strip = new StatusStrip
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(2, 0, 2, 0),
            BackColor = SystemColors.Control,
            ForeColor = SystemColors.ControlText
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

        _strip.Items.AddRange(new ToolStripItem[] { _lineLabel, _charLabel, _statusLabel });
        Controls.Add(_strip);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _strip.Dispose();
        }
        base.Dispose(disposing);
    }
}
