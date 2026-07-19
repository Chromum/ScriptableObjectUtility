# ScriptableObjectUtility

## How to Use

1. **Open the Tool:**
   - In the Unity Editor, go to `Window > Scriptable Object Utility`.

2. **Find Your Scriptable Objects:**
   - Every non abstract `ScriptableObject` type defined in your project's `Assets/` folder shows up automatically. No attribute or manual registration needed.
   - Types are grouped by namespace (a type with no namespace appears under `Global`).
   - Types from Unity's own packages (URP, VFX Graph, TextMesh Pro, etc.) and other installed packages/plugins are excluded by default. See **Preferences** below if you want to include one.

3. **Create the Scriptable Object:**
   - Click an entry to create a new asset instance.
   - You will be prompted to rename the newly created asset.
   - Creation can be undone with `Ctrl+Z` like any other asset creation.
   - Where the asset gets placed depends on the Asset Creation mode set in Preferences (see below) by default, your currently selected Project window folder.

4. **Search:**
   - Use the search field at the top of the window to filter the list by name.

5. **Hide a Type:**
   - Click a row's `⋮` button and choose `Hide This Type` to remove it from the list.
   - Manage/restore hidden types from the Preferences page.

6. **Edit the Script:**
   - Click a row's pencil icon to open that type's `.cs` file in your IDE.

7. **Browse Existing Assets:**
   - Click a row's magnifying glass icon to see every existing asset of that exact type.
   - Click an asset in that list to preview it in the embedded inspector at the bottom of the window, or click its folder icon to select it in the Project window.

8. **Refresh:**
   - The list refreshes automatically after Unity recompiles scripts.
   - Use the window's `⋮` menu and choose `Refresh ScriptableObject List` if you want to force it sooner.

### Customizing Grouping and Display Name (Optional)

1. **Add the Attribute to a Scriptable Object:**
   - Add the `[ScriptableObjectAttribute(path)]` attribute to the Scriptable Object class. The `path` parameter specifies where the Scriptable Object will appear in the Editor Window, and the last segment becomes its display name.
     ```csharp
     using Chromum.ScriptableObjectUtility;

     [ScriptableObjectAttribute("Items/Weapons/Sword")]
     public class WeaponData : ScriptableObject
     {

     }
     ```
   - `WeaponData` would appear under `Items > Weapons`, labeled "Sword".

## Installing via Package Manager

This is a Unity Package Manager (UPM) package.

1. **Install from Git URL:**
   - In Unity, open `Window > Package Manager`.
   - Click the `+` button and choose `Install package from git URL...`.
   - Enter:
     ```
     https://github.com/Chromum/ScriptableObjectUtility.git
     ```
   - Click `Install`.

2. **Or Install from Disk:**
   - Clone the repository.
   - In Package Manager, click the `+` button and choose `Install package from disk...`.
   - Select this repository's `package.json`.

## Preferences

1. **Open Preferences:**
   - Go to `Edit > Preferences > Scriptable Object Utility`, or use the tool window's `⋮` menu and choose `Manage Preferences...`.
   - Everything here is saved per-machine, like any other Unity preference.

2. **Asset Creation:**
   - Choose where new assets get placed: the active Project folder, whichever folder already has the most assets of that type, a fixed folder you configure, or a folder picker prompt every time.

3. **Window Behavior:**
   - Toggle whether newly discovered groups start expanded, whether the list auto refreshes after a script compile, whether the asset count badge shows, and whether the "Edit Script" / "Show All Assets" row buttons are shown.

4. **Included Assemblies / Included Individual Classes:**
   - Enable a whole assembly (a package, a plugin, a local/embedded package) to include every eligible `ScriptableObject` type it defines.
   - Or pick specific classes to include even when their assembly isn't enabled, for when you only want one or two types out of an otherwise excluded package.
   - Both sections have their own search field, and the tool window updates automatically whenever you change either list.

5. **Hidden Types:**
   - Untick a type here to bring it back into the tool window.

6. **Reset All Settings to Defaults:**
   - Resets everything on this page back to defaults. Does not clear remembered per group expand/collapse state.
