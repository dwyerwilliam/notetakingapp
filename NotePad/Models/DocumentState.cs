namespace NotePad.Models;

public class DocumentState
{
    public string? Filename { get; }
    public string Content { get; }
    public bool Dirty { get; }
    public int CharCount { get; }
    public int LineCount { get; }

    public DocumentState()
        : this(null, string.Empty, false)
    {
    }

    public DocumentState(string? filename, string content, bool dirty)
    {
        Filename = filename;
        Content = content;
        Dirty = dirty;
        CharCount = content.Length;
        LineCount = content.Split('\n').Length;
    }
}
