using NotePad.Models;

namespace NotePad.Services;

public sealed class DocumentStore
{
    private static readonly Lazy<DocumentStore> _instance =
        new(() => new DocumentStore());

    public static DocumentStore Instance => _instance.Value;

    public DocumentState State { get; private set; } = new();

    public event Action<DocumentState>? StateChanged;

    private DocumentStore() { }

    public void Open(string filename, string content)
    {
        State = new DocumentState(filename, content, false);
        NotifyStateChanged();
    }

    public void Save(string content, string? filename = null)
    {
        State = new DocumentState(
            filename ?? State.Filename, content, false);
        NotifyStateChanged();
    }

    public void SetDirty(bool dirty)
    {
        State = new DocumentState(State.Filename, State.Content, dirty);
        NotifyStateChanged();
    }

    public void SaveAs(string content, string filename)
    {
        State = new DocumentState(filename, content, false);
        NotifyStateChanged();
    }

    public void Reset()
    {
        State = new DocumentState();
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(State);
    }
}
