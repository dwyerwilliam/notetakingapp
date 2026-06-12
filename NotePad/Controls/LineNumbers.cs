using System.Runtime.InteropServices;

namespace NotePad.Controls;

public class LineNumbers : UserControl
{
    private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
    private const int RightPadding = 8;

    private readonly RichTextBox _editor;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    public LineNumbers(RichTextBox editor)
    {
        _editor = editor;

        Width = 45;
        Dock = DockStyle.Left;
        BackColor = SystemColors.Control;
        ForeColor = SystemColors.ControlText;
        Font = new Font("Consolas", 9f);
        DoubleBuffered = true;
    }

    public void RefreshLineNumbers()
    {
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_editor is null || !_editor.IsHandleCreated)
            return;

        var firstVisibleLine = SendMessage(_editor.Handle, EM_GETFIRSTVISIBLELINE, 0, 0);
        var lineHeight = Math.Max(1, (int)_editor.Font.GetHeight());
        var visibleLineCount = (_editor.ClientSize.Height / lineHeight) + 1;
        var currentLine = _editor.GetLineFromCharIndex(_editor.SelectionStart);
        var textBounds = new Rectangle(0, 0, Width - RightPadding, lineHeight);

        using var normalBrush = new SolidBrush(SystemColors.ControlDark);
        using var currentLineBrush = new SolidBrush(SystemColors.ControlText);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Near
        };

        for (var i = 0; i < visibleLineCount; i++)
        {
            var lineIndex = firstVisibleLine + i;
            if (lineIndex >= _editor.Lines.Length)
                break;

            textBounds.Y = i * lineHeight;
            var brush = lineIndex == currentLine ? currentLineBrush : normalBrush;
            e.Graphics.DrawString((lineIndex + 1).ToString(), Font, brush, textBounds, format);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Font.Dispose();
        }

        base.Dispose(disposing);
    }
}
