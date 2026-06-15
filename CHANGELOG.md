# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Tabbed multi-document editing
- Find & Replace dialog
- Auto-save toggle
- Syntax highlighting

---

## [1.0.0] - 2026-06-14

### Added
- New / Open / Save / Save As file operations
- Unsaved changes indicator (`*` in title bar)
- Close prompt for dirty documents
- Dark mode (auto-detects system theme)
- Line number gutter
- Find dialog (Ctrl+F)
- Status bar (line count, character count, document state)
- Full Edit menu (Undo, Redo, Cut, Copy, Paste, Select All)
- Error handling with user-friendly MessageBox dialogs
- Modular architecture: DocumentStore, FileService, ThemeHelper
- MIT License

### Technical
- .NET 9 + Windows Forms
- C# 13 with nullable reference types
- Single-file self-contained publishing support (`win-x64`)
