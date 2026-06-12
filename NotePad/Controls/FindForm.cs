namespace NotePad.Controls;

public class FindForm : Form
{
    private readonly RichTextBox _richTextBox;
    private readonly TextBox _searchTextBox;
    private readonly Label _statusLabel;
    private readonly Button _findNextButton;
    private readonly Button _closeButton;

    private string _lastSearchText = string.Empty;
    private int _currentMatchIndex;
    private int _matchCount;

    public FindForm(RichTextBox richTextBox)
    {
        _richTextBox = richTextBox ?? throw new ArgumentNullException(nameof(richTextBox));

        Text = "Find";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(300, 90);
        BackColor = Color.White;
        ForeColor = Color.Black;

        _searchTextBox = new TextBox
        {
            Location = new Point(10, 10),
            Size = new Size(280, 23),
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        _statusLabel = new Label
        {
            Location = new Point(10, 38),
            Size = new Size(130, 23),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        _findNextButton = new Button
        {
            Location = new Point(145, 55),
            Size = new Size(75, 25),
            Text = "Find Next",
            BackColor = Color.White,
            ForeColor = Color.Black,
            UseVisualStyleBackColor = false
        };
        _findNextButton.Click += FindNextButton_Click;

        _closeButton = new Button
        {
            Location = new Point(225, 55),
            Size = new Size(65, 25),
            Text = "Close",
            DialogResult = DialogResult.Cancel,
            BackColor = Color.White,
            ForeColor = Color.Black,
            UseVisualStyleBackColor = false
        };
        _closeButton.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { _searchTextBox, _statusLabel, _findNextButton, _closeButton });

        AcceptButton = _findNextButton;
        CancelButton = _closeButton;

        _searchTextBox.Focus();
        _searchTextBox.SelectAll();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _searchTextBox.Focus();
        _searchTextBox.SelectAll();
    }

    private void FindNextButton_Click(object? sender, EventArgs e)
    {
        FindNext();
    }

    private void FindNext()
    {
        string searchText = _searchTextBox.Text;
        if (string.IsNullOrEmpty(searchText))
        {
            _statusLabel.Text = "Enter search text";
            return;
        }

        if (!string.Equals(searchText, _lastSearchText, StringComparison.Ordinal))
        {
            _lastSearchText = searchText;
            _matchCount = CountMatches(searchText);
            _currentMatchIndex = 0;
        }

        if (_matchCount == 0)
        {
            _statusLabel.Text = "Not found";
            return;
        }

        int startPosition = GetStartPosition(searchText);
        int foundPosition = _richTextBox.Find(searchText, startPosition, RichTextBoxFinds.None);
        bool wrapped = false;

        if (foundPosition == -1 && startPosition > 0)
        {
            wrapped = true;
            foundPosition = _richTextBox.Find(searchText, 0, RichTextBoxFinds.None);
        }

        if (foundPosition == -1)
        {
            _statusLabel.Text = "Not found";
            return;
        }

        _richTextBox.Focus();
        _richTextBox.Select(foundPosition, searchText.Length);
        _currentMatchIndex = GetMatchIndex(searchText, foundPosition);
        _statusLabel.Text = wrapped
            ? $"Reached end of document - Found {_currentMatchIndex} of {_matchCount}"
            : $"Found {_currentMatchIndex} of {_matchCount}";
    }

    private int GetStartPosition(string searchText)
    {
        if (_richTextBox.SelectionLength == searchText.Length
            && string.Equals(_richTextBox.SelectedText, searchText, StringComparison.OrdinalIgnoreCase))
        {
            return _richTextBox.SelectionStart + _richTextBox.SelectionLength;
        }

        return _richTextBox.SelectionStart;
    }

    private int CountMatches(string searchText)
    {
        int count = 0;
        int startPosition = 0;

        while (startPosition < _richTextBox.TextLength)
        {
            int foundPosition = _richTextBox.Find(searchText, startPosition, RichTextBoxFinds.None);
            if (foundPosition == -1)
            {
                break;
            }

            count++;
            startPosition = foundPosition + searchText.Length;
        }

        return count;
    }

    private int GetMatchIndex(string searchText, int targetPosition)
    {
        int index = 0;
        int startPosition = 0;

        while (startPosition <= targetPosition && startPosition < _richTextBox.TextLength)
        {
            int foundPosition = _richTextBox.Find(searchText, startPosition, RichTextBoxFinds.None);
            if (foundPosition == -1 || foundPosition > targetPosition)
            {
                break;
            }

            index++;
            startPosition = foundPosition + searchText.Length;
        }

        return index;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _searchTextBox.Dispose();
            _statusLabel.Dispose();
            _findNextButton.Dispose();
            _closeButton.Dispose();
        }

        base.Dispose(disposing);
    }
}
