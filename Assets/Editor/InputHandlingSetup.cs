#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class InputHandlingSetup
{
    [MenuItem("Tools/Set Input Handling Both")]
    public static void SetBoth()
    {
        // activeInputHandler: 0 = Input Manager (Old), 1 = Input System Package (New), 2 = Both
        var settingsPath = "ProjectSettings/ProjectSettings.asset";
        var allObjs = AssetDatabase.LoadAllAssetsAtPath(settingsPath);
        if (allObjs == null || allObjs.Length == 0)
        {
            Debug.LogError("[InputHandlingSetup] Could not load ProjectSettings.asset");
            return;
        }
        var so = new SerializedObject(allObjs[0]);
        var prop = so.FindProperty("activeInputHandler");
        if (prop == null)
        {
            Debug.LogError("[InputHandlingSetup] Property 'activeInputHandler' not found.");
            return;
        }
        prop.intValue = 2;
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        Debug.Log("[InputHandlingSetup] Active Input Handling set to BOTH (2). Domain reload may follow.");
    }
}
#endif
