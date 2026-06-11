using NotePad.Controls;
using NotePad.Models;
using NotePad.Services;

namespace NotePad;

public class MainForm : Form
{
    private readonly DocumentStore _store = DocumentStore.Instance;
    private StatusBar _statusBar = null!;
    private RichTextBox _editor = null!;

    public MainForm(string? initialFile = null)
    {
        Text = "NotePad - Untitled";
        Size = new Size(900, 650);
        MinimumSize = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;

        InitializeEditor();
        InitializeMenu();
        InitializeStatusBar();
        SubscribeToStore();

        if (initialFile is not null)
            OpenFile(initialFile);
    }

    private void OpenFile(string path)
    {
        try
        {
            var content = FileService.ReadFile(path);
            _store.Open(path, content);
            _editor.Text = content;
            _statusBar.UpdateStatus($"Opened: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            ShowError($"Failed to open file:\n{ex.Message}");
        }
    }

    private void InitializeMenu()
    {
        var menuStrip = new MenuStrip { Dock = DockStyle.Top };

        // File menu
        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            CreateMenuItem("&New", Keys.Control | Keys.N, OnNew),
            CreateMenuItem("&Open...", Keys.Control | Keys.O, OnOpen),
            CreateMenuItem("&Save", Keys.Control | Keys.S, OnSave),
            CreateMenuItem("Save &As...", Keys.Shift | Keys.Control | Keys.S, OnSaveAs),
            new ToolStripSeparator(),
            CreateMenuItem("E&xit", Keys.None, OnExit)
        });
        menuStrip.Items.Add(fileMenu);

        // Edit menu
        var editMenu = new ToolStripMenuItem("&Edit");
        editMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            CreateMenuItem("&Undo", Keys.Control | Keys.Z, OnUndo),
            new ToolStripSeparator(),
            CreateMenuItem("Cu&t", Keys.Control | Keys.X, OnCut),
            CreateMenuItem("&Copy", Keys.Control | Keys.C, OnCopy),
            CreateMenuItem("&Paste", Keys.Control | Keys.V, OnPaste),
            new ToolStripSeparator(),
            CreateMenuItem("Select &All", Keys.Control | Keys.A, OnSelectAll)
        });
        menuStrip.Items.Add(editMenu);

        MainMenuStrip = menuStrip;
        Controls.Add(menuStrip);
    }

    private void InitializeEditor()
    {
        _editor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10f),
            ScrollBars = RichTextBoxScrollBars.ForcedBoth,
            ForeColor = Color.Black,
            BackColor = Color.White,
            WordWrap = false
        };
        _editor.TextChanged += OnEditorTextChanged;

        Controls.Add(_editor);
    }

    private void InitializeStatusBar()
    {
        _statusBar = new StatusBar
        {
            Dock = DockStyle.Bottom,
            Height = 30
        };
        Controls.Add(_statusBar);
    }

    private void SubscribeToStore()
    {
        _store.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(DocumentState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<DocumentState>(OnStateChanged), state);
            return;
        }

        UpdateTitle();
        UpdateStatusBar();
    }

    private void UpdateTitle()
    {
        var state = _store.State;
        var dirtyMarker = state.Dirty ? " *" : string.Empty;
        var name = state.Filename is not null ? Path.GetFileName(state.Filename) : "Untitled";
        Text = $"NotePad - {name}{dirtyMarker}";
    }

    private void UpdateStatusBar()
    {
        var state = _store.State;
        _statusBar.UpdateCounts(state.LineCount, state.CharCount);
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        _store.SetDirty(true);
        _statusBar.UpdateCounts(_editor.Lines.Length, _editor.Text.Length);
    }

    // --- File menu handlers ---

    private void OnNew(object? sender, EventArgs e)
    {
        try
        {
            _store.Reset();
            _editor.Text = string.Empty;
            _statusBar.UpdateStatus("New document created");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void OnOpen(object? sender, EventArgs e)
    {
        try
        {
            var path = FileService.ShowOpenDialog(this);
            if (path is null)
                return;

            var content = FileService.ReadFile(path);
            _store.Open(path, content);
            _editor.Text = content;
            _statusBar.UpdateStatus($"Opened: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void OnSave(object? sender, EventArgs e)
    {
        try
        {
            var state = _store.State;
            var content = _editor.Text;

            if (state.Filename is null)
            {
                var path = FileService.ShowSaveDialog(this);
                if (path is null)
                    return;

                FileService.WriteFile(path, content);
                _store.Save(content, path);
                _statusBar.UpdateStatus($"Saved: {Path.GetFileName(path)}");
            }
            else
            {
                FileService.WriteFile(state.Filename, content);
                _store.Save(content);
                _statusBar.UpdateStatus($"Saved: {Path.GetFileName(state.Filename)}");
            }
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void OnSaveAs(object? sender, EventArgs e)
    {
        try
        {
            var path = FileService.ShowSaveDialog(this);
            if (path is null)
                return;

            FileService.WriteFile(path, _editor.Text);
            _store.SaveAs(_editor.Text);
            _store.State.Filename = path;
            _statusBar.UpdateStatus($"Saved as: {Path.GetFileName(path)}");
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        Close();
    }

    // --- Edit menu handlers ---

    private void OnUndo(object? sender, EventArgs e) => _editor.Undo();
    private void OnCut(object? sender, EventArgs e) => _editor.Cut();
    private void OnCopy(object? sender, EventArgs e) => _editor.Copy();
    private void OnPaste(object? sender, EventArgs e) => _editor.Paste();
    private void OnSelectAll(object? sender, EventArgs e) => _editor.SelectAll();

    // --- Helpers ---

    private static ToolStripMenuItem CreateMenuItem(string text, Keys shortcut, EventHandler onClick)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeys = shortcut };
        item.Click += onClick;
        return item;
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // --- Close handling ---

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_store.State.Dirty && e.CloseReason != CloseReason.TaskManagerClosing)
        {
            var result = MessageBox.Show(
                "Do you want to save changes before closing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == DialogResult.Yes)
            {
                try
                {
                    var state = _store.State;
                    var content = _editor.Text;

                    if (state.Filename is null)
                    {
                        var path = FileService.ShowSaveDialog(this);
                        if (path is null)
                        {
                            e.Cancel = true;
                            return;
                        }
                        FileService.WriteFile(path, content);
                    }
                    else
                    {
                        FileService.WriteFile(state.Filename, content);
                    }
                    _store.SetDirty(false);
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                    e.Cancel = true;
                }
            }
        }

        base.OnFormClosing(e);
    }
}
