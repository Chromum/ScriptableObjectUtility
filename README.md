# Scriptable Object Utility

A Unity Editor window that automatically finds every `ScriptableObject` type in your
project and lets you create new asset instances with one click — no attribute or
manual registration required.

## Installing

This is a Unity Package Manager (UPM) package, installed via git URL:

1. In Unity, open `Window > Package Manager`.
2. Click the `+` button and choose `Install package from git URL...`.
3. Enter:
   ```
   https://github.com/YourOrg/ScriptableObjectUtility.git
   ```
4. Click `Install`.

You can also install from a local clone via `Install package from disk...`, pointing at
this repository's `package.json`.

## How to Use

1. Open the tool via `Window > Scriptable Object Utility`.
2. Every non-abstract `ScriptableObject` type in your project is listed automatically,
   grouped by namespace (a type with no namespace is grouped under `Global`).
3. Click an entry to create a new asset instance in the currently selected Project
   window folder. You'll be prompted to rename it, and the creation can be undone
   with `Ctrl+Z` like any other asset creation.
4. Use the search field at the top of the window to filter the list by type name.
5. Foldout expand/collapse state is remembered between sessions.
6. Use the window's `⋮` menu and choose `Refresh ScriptableObject List` if you add a
   new type while the window is already open.

### Customizing grouping and display name (optional)

By default, types are grouped by namespace and labeled with their class name. To
override this, add the `[ScriptableObjectAttribute(path)]` attribute to a class:

```csharp
using Bonejam.ScriptableObjectUtility;

[ScriptableObjectAttribute("Items/Weapons/Sword")]
public class WeaponData : ScriptableObject
{
    // Your code here
}
```

The path is split on `/` (or `\`): every segment except the last becomes a nested
folder in the tool window, and the last segment becomes the entry's display name —
here, `WeaponData` would appear under `Items > Weapons`, labeled "Sword".
