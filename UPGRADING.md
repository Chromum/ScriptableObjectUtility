# Upgrading from v1.0.0

1. **Delete the Old Files:**
   - v1.0.0 was at `Assets/ScriptableObjectWindow/`. Delete that whole folder from your project.

2. **Update `using`s if You Used the Attribute:**
   - The old `[ScriptableObjectAttribute]` lived in the global namespace. The new one is in `Chromum.ScriptableObjectUtility`.
   - Once the old files are deleted, any script still writing `[ScriptableObjectAttribute("Foo/Bar")]` without a `using` will fail to compile. Pick one:
     - Add `using Chromum.ScriptableObjectUtility;` to keep the custom grouping/rename behavior, or
     - Delete the attribute because auto-discovery covers it now, but it will fall back to namespace-based grouping instead of the custom path.

3. **Add an Assembly Reference if You Use a Custom `.asmdef`:**
   - If your `ScriptableObject` scripts live inside their own custom assembly definition (not the default/no-asmdef assembly), add `Chromum.ScriptableObjectUtility.Runtime` to that asmdef's `"references"` array to see the attribute.

4. **Close and Reopen the Tool Window:**
   - If it was docked or saved in your layout, the old window class goes away with the deleted files.
   - Reopen it fresh via `Window > Scriptable Object Utility` afterward.