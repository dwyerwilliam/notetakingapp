# .NET 9 Upgrade Plan

## Motivation

RichTextBox is required for full Edit menu functionality (Undo, Redo, Cut, Copy, Paste, Select All).
A plain TextBox lacks these capabilities. RichTextBox has known RTF deserialization vulnerabilities
(CVE-2023-36449) that are patched in .NET 9.

## Scope

Upgrade the NotePad project from .NET 7 to .NET 9 while preserving all existing functionality.

## Steps

### Step 1 — Verify .NET 9 SDK availability
- Check `dotnet --list-sdks` for 9.x presence
- If absent, install latest .NET 9 SDK via official installer

### Step 2 — Update project file
- Change `TargetFramework` from `net7.0-windows` to `net9.0-windows`
- Keep all other properties unchanged (nullable, implicit usings, WinForms)

### Step 3 — Update NOTEPAD.md spec
- Change Technology Stack runtime row to reflect .NET 9
- Add RichTextBox security mitigation note

### Step 4 — Build and verify
- Run `dotnet restore` and `dotnet build`
- Confirm 0 warnings, 0 errors

### Step 5 — Apply RichTextBox security mitigation
- Ensure files are opened via `.Text` property only (not `.Rtf`)
- Add input sanitization: strip RTF control words before loading into editor
- Document mitigation in code comments

### Step 6 — Commit changes
- Stage all modified files
- Commit with descriptive message

## Risks

- .NET 9 SDK may not be installed locally — install required
- Minor API surface changes between .NET 7 and .NET 9 WinForms — unlikely for our usage
- Build artifacts (bin/obj) must be cleaned before first .NET 9 build

## Files Affected

- `NotePad/NotePad.csproj` — target framework change
- `NOTEPAD.md` — technology stack update
- `notepad_issues.md` — mark SEC-01 as mitigated via .NET 9 upgrade
