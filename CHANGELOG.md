# Changelog

## v2.0.1

1. **Repo Cleanup:**
   - Removed stray `.meta` files that had been generated for `README.md`, `CHANGELOG.md`, and `UPGRADING.md` and accidentally committed.
   - Added a `.gitignore` entry (`*.md.meta`) so they don't get re-added if Unity regenerates them locally.

## v2.0.0

1. **No More Required Attribute:**
   - Every non abstract `ScriptableObject` type in your project is now discovered automatically.
   - The `[ScriptableObjectAttribute]` still exists, but is now optional, used only to override the grouping path and/or display name.

2. **Converted to a Package:**
   - The repo root is now a proper Unity Package Manager (UPM) package (`package.json` at the top level, `Runtime`/`Editor` folders each with their own assembly definition).
   - Install via `Window > Package Manager > Install package from git URL...`, or from disk instead of cloning and dragging files into `Assets/`, or importing a `.unitypackage`.
   - Assembly definitions are `Chromum.ScriptableObjectUtility.Runtime` and `Chromum.ScriptableObjectUtility.Editor`.

3. **Better Discovery & Filtering:**
   - Fixed a bug where a type had to derive directly from `ScriptableObject` subclasses more than one level deep were silently rejected.
   - Excludes `Editor`/`EditorWindow` subclasses, which also technically derive from `ScriptableObject`.
   - By default only scans assemblies compiled from `Assets/`, keeping the list free of the hundreds of `ScriptableObject` types Unity's own bundled packages define. Any other assembly, or individual classes within it, can be re enabled from Preferences.
   - Each row's `⋮` options menu lets you hide a type from the list entirely.

4. **New Features:**
   - Search field filters the list live.
   - Undo support on asset creation.
   - Foldout expand/collapse state remembered per group across sessions.
   - "Edit Script" button per row opens the type's `.cs` file in your IDE.
   - "Show All Assets" browser: a per-type view listing every existing asset of that exact type, with a "Select In Project" button per asset and a back button to return to the tree.
   - Embedded, resizable inspector preview at the bottom of the window — click an asset in the browser to preview it without touching your actual Project selection or hijacking your real Inspector window.
   - Asset count per type (e.g. `WeaponData (3)`), togglable.
   - Configurable asset creation location: active Project folder, the folder with the most existing assets of that type, a fixed configured folder, or a folder-picker prompt every time.
   - New Preferences page (`Edit > Preferences > Scriptable Object Utility`): asset-creation mode, window behavior toggles, included assemblies/classes, hidden types, and a "Reset All Settings to Defaults" button.
   - Auto refreshes the list after Unity recompiles scripts.
   - Remembers your last search text and whether you were viewing the tree or a specific type's asset browser.

5. **Fixes & Cleanup:**
   - Full visual pass on the tool window: consistent styling, spacing, hover states, and a resizable split view.