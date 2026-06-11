using NotePad.Models;

namespace NotePad.Services;

public sealed class DocumentStore
{
    private static readonly Lazy<DocumentStore> _instance =
        new(() => new DocumentStore());

    public static DocumentStore Instance => _instance.Value;

    public DocumentState State { get; } = new();

    public event Action<DocumentState>? StateChanged;

    private DocumentStore() { }

    public void Open(string filename, string content)
    {
        State.Filename = filename;
        State.Content = content;
        State.Dirty = false;
        NotifyStateChanged();
    }

    public void Save(string content, string? filename = null)
    {
        if (filename is not null)
            State.Filename = filename;

        State.Content = content;
        State.Dirty = false;
        NotifyStateChanged();
    }

    public void SetDirty(bool dirty)
    {
        State.Dirty = dirty;
        NotifyStateChanged();
    }

    public void SaveAs(string content)
    {
        State.Content = content;
        State.Dirty = false;
        NotifyStateChanged();
    }

    public void Reset()
    {
        State.Filename = null;
        State.Content = string.Empty;
        State.Dirty = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(State);
    }
}
