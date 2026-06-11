namespace NotePad.Models;

public class DocumentState
{
    public string? Filename { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool Dirty { get; set; }
    public int CharCount => Content.Length;
    public int LineCount => Content.Split('\n').Length;
}
