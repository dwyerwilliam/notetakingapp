# NotePad Issues — Fix Plan

## Critical: Security

### SEC-01: RichTextBox RTF Deserialization Vulnerability (CVE-2023-36449)
- **File:** `MainForm.cs`
- **Issue:** `RichTextBox` is vulnerable to arbitrary code execution via crafted RTF content. Even though we open `.txt` files, a malicious file could contain RTF payloads.
- **Fix:** Replace `RichTextBox` with `TextBox` (Multiline=true). We're a plain text editor — RichTextBox is unnecessary and dangerous.

## High: Best Practices

### BP-01: Empty catch block suppresses errors silently
- **File:** `MainForm.cs` line 334 (`LoadIcon`)
- **Issue:** Empty `catch {}` block swallows all exceptions. Violates coding standards.
- **Fix:** Remove the catch or return fallback inline. The `try/return` followed by a fallback already handles this.

### BP-02: DocumentState properties are mutable (should be encapsulated)
- **File:** `Models/DocumentState.cs`
- **Issue:** Public `set` on `Filename`, `Content`, `Dirty` allows external mutation of store state. Spec called for `init` but implementation uses mutable setters. `MainForm.OnSaveAs` directly mutates `_store.State.Filename`.
- **Fix:** Make `DocumentState` properties internal/set-only. Route all mutations through `DocumentStore` methods.

### BP-03: DocumentStore.State exposed publicly, bypassing encapsulation
- **File:** `Services/DocumentStore.cs`, `MainForm.cs`
- **Issue:** `State` property is public with mutable backing object. `MainForm.OnSaveAs` does `_store.State.Filename = path` instead of using store API.
- **Fix:** Change `State` to `get; private set;` with a read-only accessor. Add `SaveAs` to `DocumentStore` that properly updates filename.

### BP-04: DocumentStore.Save / SaveAs don't actually write files
- **File:** `Services/DocumentStore.cs`
- **Issue:** `Save()` and `SaveAs()` only update state — they don't call `FileService.WriteFile`. File I/O is duplicated in `MainForm`. Store should own the full save contract per spec.
- **Fix:** Have `DocumentStore.Save` and `DocumentStore.SaveAs` handle file I/O internally, or document that MainForm is the orchestrator. Per spec, the store should delegate.

### BP-05: LineCount recomputes on every access (performance)
- **File:** `Models/DocumentState.cs`
- **Issue:** `LineCount` uses `Content.Split('\n')` as an expression property — allocates a new array on every read.
- **Fix:** Compute line count at mutation time in `DocumentStore`, store as a cached field.

### BP-06: StatusBar doesn't dispose child controls
- **File:** `Controls/StatusBar.cs`
- **Issue:** The `StatusStrip` added to `Controls` is not disposed. `ToolStripStatusLabel` fields are never cleaned up.
- **Fix:** Override `Dispose(disposing)` to dispose the StatusStrip.

## Medium: Code Quality

### CQ-01: Help > About menu not in spec
- **File:** `MainForm.cs`
- **Issue:** `Help` menu and `OnAbout` handler exist but were not in the original spec.
- **Fix:** Keep as-is. Not a bug, just out of spec scope.

### CQ-02: Save As menu item not in spec
- **File:** `MainForm.cs`
- **Issue:** "Save As..." is a bonus feature, not in the original plan.
- **Fix:** Keep as-is. Useful feature.

### CQ-03: Command-line file argument not in spec
- **File:** `Program.cs`
- **Issue:** Opening file from CLI args is a bonus feature.
- **Fix:** Keep as-is. Useful feature.

## Fix Order

1. [x] SEC-01 — Replace RichTextBox with TextBox (security)
2. [x] BP-01 — Fix empty catch block
3. [x] BP-06 — Fix StatusBar disposal
4. [x] BP-05 — Cache LineCount
5. [x] BP-02 + BP-03 — Encapsulate DocumentState / DocumentStore
6. [x] BP-04 — Fix Save/SaveAs file I/O delegation

## Status: All issues resolved. Build: 0 warnings, 0 errors.
