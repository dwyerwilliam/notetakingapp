using NotePad.Controls;
using NotePad.Models;
using NotePad.Services;

namespace NotePad;

public class MainForm : Form
{
    private readonly DocumentStore _store = DocumentStore.Instance;
    private MenuStrip _menuStrip = null!;
    private StatusBar _statusBar = null!;
    private LineNumbers _lineNumbers = null!;
    private RichTextBox _editor = null!;

    public MainForm(string? initialFile = null)
    {
        Text = "NotePad - Untitled";
        Size = new Size(900, 650);
        MinimumSize = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = LoadIcon();

        InitializeMenu();
        InitializeStatusBar();
        InitializeEditor();
        SubscribeToStore();
        SubscribeToTheme();

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
        _menuStrip = new MenuStrip();

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
        _menuStrip.Items.Add(fileMenu);

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
            CreateMenuItem("Select &All", Keys.Control | Keys.A, OnSelectAll),
            new ToolStripSeparator(),
            CreateMenuItem("&Find...", Keys.Control | Keys.F, OnFind)
        });
        _menuStrip.Items.Add(editMenu);

        var helpMenu = new ToolStripMenuItem("&Help");
        helpMenu.DropDownItems.Add(CreateMenuItem("&About", Keys.None, OnAbout));
        helpMenu.Alignment = ToolStripItemAlignment.Right;
        _menuStrip.Items.Add(helpMenu);

        MainMenuStrip = _menuStrip;
        Controls.Add(_menuStrip);
    }

    private void InitializeEditor()
    {
        _editor = new RichTextBox
        {
            Dock = DockStyle.None,
            Font = new Font("Consolas", 10f),
            ScrollBars = RichTextBoxScrollBars.ForcedBoth,
            ForeColor = SystemColors.WindowText,
            BackColor = SystemColors.Window,
            WordWrap = false
        };
        _editor.TextChanged += OnEditorTextChanged;
        _editor.VScroll += (_, _) => _lineNumbers.RefreshLineNumbers();
        _editor.MouseUp += (_, _) => _lineNumbers.RefreshLineNumbers();

        _lineNumbers = new LineNumbers(_editor);
        Controls.AddRange(new Control[] { _lineNumbers, _editor });
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutEditorBelowMenu();
    }

    private void LayoutEditorBelowMenu()
    {
        if (_menuStrip is null || _statusBar is null || _editor is null || _lineNumbers is null)
            return;

        var gutterWidth = _lineNumbers.Width;
        var top = _menuStrip.Bottom;
        var bottom = _statusBar.Top > top ? _statusBar.Top : ClientSize.Height;
        var editorBounds = new Rectangle(gutterWidth, top, ClientSize.Width - gutterWidth, Math.Max(0, bottom - top));

        if (_editor.Bounds != editorBounds)
            _editor.Bounds = editorBounds;
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

    private void SubscribeToTheme()
    {
        ThemeHelper.ApplyTheme(this);
        ThemeHelper.ThemeChanged += OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Invoke(new Action(() =>
        {
            ThemeHelper.ApplyTheme(this);
            _editor.BackColor = SystemColors.Window;
            _editor.ForeColor = SystemColors.WindowText;
            _lineNumbers.RefreshLineNumbers();
        }));
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
            _store.SaveAs(_editor.Text, path);
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

    private void OnFind(object? sender, EventArgs e)
    {
        using var findForm = new FindForm(_editor);
        findForm.ShowDialog(this);
    }

    private void OnAbout(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "NotePad v1.0\n\n" +
            "A simple text editor built with .NET 7 WinForms.\n\n" +
            "Developed with Bill Dwyer.\n\n" +
            $"Built: {DateTime.Now:MMMM dd, yyyy}\n\n" +
            "© 2026 All rights reserved.",
            "About NotePad",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

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

    private static Icon LoadIcon()
    {
        try
        {
            using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("NotePad.Resources.notepad.png");
            if (stream is not null)
            {
                using var bitmap = new Bitmap(stream);
                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
        catch
        {
            return SystemIcons.Application;
        }

        return SystemIcons.Application;
    }
}
