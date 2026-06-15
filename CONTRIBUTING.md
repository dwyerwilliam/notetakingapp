# Contributing to NotePad

Thank you for your interest in contributing! Here's how to get started.

## Development Setup

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 10 or later
- Git

### Local Development

```bash
# Clone the repository
git clone https://github.com/dwyerwilliam/notetakingapp.git
cd notetakingapp

# Restore and build
dotnet restore
dotnet build

# Run the application
dotnet run
```

## Project Structure

```
NotePad/
├── Program.cs           # Entry point
├── MainForm.cs          # Main window, menus, close handling
├── Models/
│   └── DocumentState.cs # Document state POCO
├── Services/
│   ├── DocumentStore.cs # Singleton state manager
│   ├── FileService.cs   # File I/O helpers
│   └── ThemeHelper.cs   # Theme detection + colors
└── Controls/
    ├── StatusBar.cs     # Custom status bar
    ├── FindForm.cs      # Find dialog
    └── LineNumbers.cs   # Line number gutter
```

## Guidelines

### Code Style
- Follow existing C# conventions in the codebase
- Use nullable reference types (`#nullable enable`)
- Use file-scoped namespaces (C# 10+)
- Keep modules focused — one responsibility per class

### Commit Messages
Use conventional commit format:
- `feat:` — new features
- `fix:` — bug fixes
- `docs:` — documentation changes
- `chore:` — maintenance tasks
- `refactor:` — code restructuring

Example:
```
feat: add find and replace dialog
fix: resolve dark mode flicker on theme change
docs: update README with build instructions
```

### Pull Requests
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Make your changes
4. Test locally (`dotnet build` and `dotnet run`)
5. Commit with a descriptive message
6. Push and open a PR against `primary` branch

## Reporting Issues

Before opening an issue, check if it already exists. When reporting:
- Include your OS version and .NET SDK version
- Provide steps to reproduce
- Include any relevant error messages or screenshots

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
