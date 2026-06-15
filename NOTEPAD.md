# NotePad Specification

## Overview

A simple, lightweight Windows desktop note-taking application built with a modern framework. Supports basic text file operations: open, edit, save, create new, and exit. Designed with clear module boundaries for incremental agent-led development.

---

## Requirements

### Functional Requirements

| ID    | Requirement                          | Priority |
|-------|--------------------------------------|----------|
| FR-01 | Application launches with a window   | Must    |
| FR-02 | Create a new blank document          | Must    |
| FR-03 | Open an existing .txt file           | Must    |
| FR-04 | Edit the document content            | Must    |
| FR-05 | Save the current document (overwrite)| Must    |
| FR-06 | Exit the application                 | Must    |
| FR-07 | Show unsaved changes indicator       | Should  |
| FR-08 | Prompt to save when closing unsaved  | Should  |
| FR-09 | Show current filename in title bar   | Should  |
| FR-10 | Status indicator for file state      | Could   |

### Non-Functional Requirements

- Single-window desktop application
- Fast startup, minimal memory footprint
- Windows 10+ compatible
- Clean separation of concerns for modular development
- **Dark mode support**: auto-detect system theme, apply matching colors across all UI elements

---

## Architecture

```
NotePad/
├── NotePad.sln
├── NotePad/
│   ├── NotePad.csproj
│   ├── Program.cs                  # Entry point, Application.Run
│   ├── MainForm.cs                 # Main window, menu bar, status bar wiring
│   ├── Models/
│   │   └── DocumentState.cs        # Document state: filename, content, dirty, charCount
│   ├── Services/
│   │   ├── DocumentStore.cs        # Central document state management
│   │   ├── FileService.cs          # File I/O: open, save, save-as operations
│   │   └── ThemeHelper.cs          # Dark/light theme detection and color management
│   └── Controls/
│       ├── StatusBar.cs            # Custom status bar control (lines, chars, status)
│       ├── FindForm.cs             # Modal find dialog (Ctrl+F search)
│       └── LineNumbers.cs          # Line number gutter (left of editor)
└── README.md
```

### Module Responsibilities

| Module              | Responsibility                                             |
|---------------------|------------------------------------------------------------|
| `Program.cs`        | Entry point, configure and run the WinForms application    |
| `MainForm.cs`       | Main window: menu bar, editor, status bar, close handling  |
| `DocumentState.cs`  | POCO holding filename, content, dirty flag, char count     |
| `DocumentStore.cs`  | Singleton managing DocumentState, raises Changed events    |
| `FileService.cs`    | File I/O: OpenFileDialog/SaveFileDialog, read/write disk   |
| `ThemeService.cs`   | Detect system theme, provide color palette, notify on change |
| `StatusBar.cs`      | Custom UserControl showing lines, characters, status msg   |

---

## Data Model

### DocumentState

```csharp
public class DocumentState
{
    /// <summary>
    /// Absolute path of the current file. Null for a new, unsaved document.
    /// </summary>
    public string? Filename { get; init; }

    /// <summary>
    /// Current text content of the document.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Whether the content has unsaved changes.
    /// </summary>
    public bool Dirty { get; init; }

    /// <summary>
    /// Character count for the status bar.
    /// </summary>
    public int CharCount { get; init; }

    /// <summary>
    /// Line count for the status bar.
    /// </summary>
    public int LineCount { get; init; }
}
```

### DocumentStore (Singleton)

```csharp
public sealed class DocumentStore
{
    public static DocumentStore Instance { get; } = new();

    public DocumentState State { get; private set; } = new();

    public event Action<DocumentState>? StateChanged;

    public void Open(string filename, string content);
    public void Save(string content, string? filename = null);
    public void SaveAs(string content);
    public void SetDirty(bool dirty);
    public void Reset();

    private void NotifyStateChanged();
}
```

### FileService (Static Helpers)

```csharp
public static class FileService
{
    public static string? ShowOpenDialog(IWin32Window owner);
    public static string? ShowSaveDialog(IWin32Window owner, string defaultName = "Untitled.txt");
    public static string ReadFile(string path);
    public static void WriteFile(string path, string content);
}
```

---

## State Machine

```
[New Document]
       │
       ▼
   dirty? ────(close)──▶ [Prompt Save?] ─(yes/No file)──▶ [Save Dialog] ─(saved)──▶ [New Document]
       │                      │ (no)                            │ (cancel)
       │                      ▼                                 ▼
     (edit)                [New Doc]                    [New Document]
       │
       ▼
   [Loaded File] ──dirty?──▶ same flow as above ──(save/overwrite)──▶ [Loaded File]
```

- **States:** `NewDocument`, `LoadedFile`
- **Transitions trigger:** `DocumentStore.Open()`, `DocumentStore.Save()`, `DocumentStore.SetDirty()`, `MainForm.FormClosing`

---

## UI Layout

```
┌─────────────────────────────────────────────┐
│  NotePad                               _ □ X │  ← Title bar (filename + dirty marker)
├─────────────────────────────────────────────┤
│ [File] [Edit]                              │  ↓ Menu bar
├─────────────────────────────────────────────┤
│                                             │
│                                             │
│         Text Editor Area                    │  ↑ Content editable
│                                             │
│                                             │
│                                             │
├─────────────────────────────────────────────┤
│ [Lines: #]  Chars: #       Status message  │  ↓ Status bar
└─────────────────────────────────────────────┘
```

### Window Properties

| Property         | Value           |
|------------------|-----------------|
| Initial width    | 900 px          |
| Initial height   | 650 px          |
| Minimum width    | 400 px          |
| Minimum height   | 300 px          |

### Menu Items

#### File
| Item      | Shortcut  | Action              |
|-----------|-----------|---------------------|
| New       | Ctrl+N    | Create new document |
| Open      | Ctrl+O    | Open file dialog    |
| Save      | Ctrl+S    | Overwrite current   |
| Exit      | —         | Close window        |

#### Edit
| Item     | Shortcut | Action              |
|----------|----------|---------------------|
| Undo     | Ctrl+Z   | Edit undo           |
| Redo     | Ctrl+Y   | Edit redo           |
| Cut      | Ctrl+X   | Edit cut            |
| Copy     | Ctrl+C   | Edit copy            |
| Paste    | Ctrl+V   | Edit paste          |
| Select All | Ctrl+A  | Select all content  |

---

## File I/O Contracts

### FileService.ShowOpenDialog() — `string?`

1. Create `OpenFileDialog` filtering `*.txt` and `*.*` files
2. Show dialog owned by `MainForm`
3. Return selected path, or `null` on cancellation

### FileService.ReadFile() — `string`

1. Call `File.ReadAllText(path, Encoding.UTF8)`
2. Throw on I/O failure (handled by caller's `try/catch`)

### FileService.WriteFile() — `void`

1. Call `File.WriteAllText(path, content, Encoding.UTF8)`
2. Throw on I/O failure (handled by caller's `try/catch`)

### FileService.ShowSaveDialog() — `string?`

1. Create `SaveFileDialog` defaulting filename `"Untitled.txt"`
2. Show dialog owned by `MainForm`
3. Return selected path, or `null` on cancellation

### DocumentStore.Open() — `void`

1. Call `FileService.ShowOpenDialog()` → get path
2. Call `FileService.ReadFile(path)` → get content
3. Update internal `DocumentState`: `{ Filename, Content, Dirty: false }`
4. Compute `CharCount` and `LineCount`
5. Raise `StateChanged` event

### DocumentStore.Save() — `void`

1. If `State.Filename` is null, delegate to `SaveAs()`
2. Call `FileService.WriteFile(State.Filename, State.Content)`
3. On success: set `Dirty = false`, raise `StateChanged`
4. On failure: rethrow (caller shows `MessageBox`)

### DocumentStore.SaveAs() — `void`

1. Call `FileService.ShowSaveDialog()` → get path
2. Update `State.Filename` with chosen path
3. Delegate to `Save()`

---

## Error Handling

| Scenario            | .NET Exception              | Behavior                                              |
|----------------------|-----------------------------|-------------------------------------------------------|
| File not found       | `FileNotFoundException`     | `MessageBox.Show(..., MessageBoxButtons.OK, MessageBoxIcon.Error)` |
| Permission denied    | `UnauthorizedAccessException` | Same as above                                       |
| Disk full            | `IOException`               | Same as above                                         |
| General I/O failure  | `Exception` (catch-all)     | Same as above with generic message                    |

All file operations in `MainForm` are wrapped in `try/catch` blocks that route to `ShowError(string message)`.

---

## Technology Stack

| Layer        | Technology                    | Rationale                              |
|-------------|-------------------------------|----------------------------------------|
| Runtime     | .NET 9                        | RTF security patches (CVE-2023-36449), dark mode APIs |
| Framework   | Windows Forms                 | Native desktop UI, minimal overhead    |
| Editor      | `RichTextBox`                 | Full Undo/Redo/Cut/Copy/Paste/SelectAll support required |
| Language    | C# 13                         | Nullable refs, file-scoped namespaces, primary constructors |
| File I/O    | `System.IO` + CommonDialog    | Built-in, no external dependencies     |
| Packaging   | `dotnet publish -r win-x64`   | Single-file self-contained executable  |

---

## Development Plan — Agent-Ready Tasks

Each task should be completed sequentially and independently verifiable.

### Phase 1 — Foundation

| #  | Task                              | Expected Output                                       |
|----|-----------------------------------|-------------------------------------------------------|
| 1  | Scaffold .NET 9 WinForms project  | `NotePad.sln`, `NotePad.csproj`, `dotnet build` OK    |
| 2  | Create `Program.cs` entry point   | App launches, shows `MainForm`                        |
| 3  | Build `MainForm` shell            | Window with menu strip, RichTextBox editor, status label  |
| 4  | Implement `DocumentState` model   | POCO with `Filename`, `Content`, `Dirty`, counts      |
| 5  | Implement `DocumentStore`         | Singleton with `StateChanged` event, `Open/Save/Reset`|

### Phase 2 — Core Features

| #  | Task                              | Expected Output                                       |
|----|-----------------------------------|-------------------------------------------------------|
| 6  | Implement `FileService`           | Static helpers: `ShowOpenDialog`, `ShowSaveDialog`    |
| 7  | Wire File > New/Open/Save/Exit    | Menu items invoke store methods                       |
| 8  | Wire Edit > Undo/Cut/Copy/Paste   | Menu items call `TextBox` methods                     |
| 9  | Sync editor ↔ store on load       | Opening file populates `TextBox.Text`                 |
| 10 | Track dirty state on edit         | Typing sets `Dirty = true`, raises `StateChanged`     |

### Phase 3 — Polish

| #  | Task                              | Expected Output                                       |
|----|-----------------------------------|-------------------------------------------------------|
| 11 | Title bar dirty indicator         | `*` appended to title when `Dirty` is true            |
| 12 | Close prompt for unsaved changes  | `MessageBox` on `FormClosing` when dirty              |
| 13 | Status bar with line/char count   | `StatusLabel` updates on every `StateChanged`         |
| 14 | Error handling with MessageBox    | I/O failures show user-friendly error dialogs         |

---

## Build & Run Commands

```bash
dotnet restore
dotnet build
dotnet run                  # Launch debug build
dotnet run --no-build       # Skip rebuild when iterating

# Production packaging
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Project File Template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net7.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationManifest>app.manifest</ApplicationManifest>
  </PropertyGroup>
</Project>
```

---

## Acceptance Criteria (E2E)

1. Launch → blank editor window appears, title "NotePad — Untitled"
2. File > Open → file dialog → selected .txt loads into editor
3. Type text → red star (*) appears in title bar
4. File > Save → saves to current path, star removed
5. File > New → clears editor, marks dirty
6. Close window with unsaved changes → save prompt shown
7. File > Exit / close → application terminates cleanly
8. All menu shortcuts (Ctrl+S, Ctrl+O, Ctrl+N) work

---

## Assumptions & Future Scope

- Only plain .txt files supported (no rich text formatting)
- Single-file editing only; no tabs or multi-document support
- No auto-save feature (not in v1)
- No dark/light theme toggle (can be layered on top of editor later)
- No search and replace (add to Edit menu in v2, see below)

### ThemeService API

```csharp
public static class ThemeService
{
    public static bool IsDarkMode { get; }
    public static event Action<bool>? ThemeChanged;
    public static Colors GetColors();
    public static void ApplyToForm(Form form);
}

public record Colors(
    Color EditorBackground,
    Color EditorForeground,
    Color MenuBackground,
    Color MenuForeground,
    Color StatusBarBackground,
    Color StatusBarForeground
);
```

### Planned Future Features

| #    | Feature              | Module to Add               |
|------|----------------------|-----------------------------|
| FF-1 | Tabbed editing       | `TabManager.cs`             |
| FF-2 | Find/Replace         | `FindReplaceForm.cs`        |
| FF-3 | Auto-save toggle     | `SettingsStore.cs`          |
