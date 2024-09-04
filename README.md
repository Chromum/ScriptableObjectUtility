# ScriptableObjectUtility


## How to Use

1. **Add the Attribute to a Scriptable Object:**
   - Create a new Scriptable Object or open an existing one.
   - Add the `[ScriptableObjectAttribute(path)]` attribute to the Scriptable Object class. The `path` parameter specifies where the Scriptable Object will appear in the Unity Editor window.
     ```csharp
     [ScriptableObjectAttribute("Your/Path/Here")]
     public class YourScriptableObject : ScriptableObject
     {
         // Your code here
     }
     ```

2. **Open the Scriptable Object Utility Window:**
   - In the Unity Editor, go to `Window > Scriptable Object Utility`.

3. **Create the Scriptable Object:**
   - In the Scriptable Object Utility window, you will see the Scriptable Object listed under the specified path.
   - Click on the Scriptable Object to create the asset in your current project view folder.
   - You will be prompted to rename the newly created asset.
  
   
 
## Installing from Releases Tab
1. **Download the Package:**
   - Navigate to the Releases tab of the repository.
   - Download the latest `.unitypackage` file.

2. **Import the Package:**
   - Open your Unity project.
   - Go to `Assets > Import Package > Custom Package...`.
   - Select the downloaded `.unitypackage` file.
   - Click `Import` to add the package to your project.

## Git Clone

1. **Clone the Repository:**
   - Open your terminal or command prompt.
   - Run the following command to clone the repository:
     ```bash
     git clone https://github.com/YourRepository.git
     ```
2. **Drag into Unity Project:**
   - Open your Unity project.
   - In your file explorer, navigate to the cloned repository.
   - Drag the entire folder or specific assets you need into the `Assets` folder of your Unity project.
