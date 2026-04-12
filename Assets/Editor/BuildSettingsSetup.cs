#if UNITY_EDITOR
using UnityEditor;

public static class BuildSettingsSetup
{
    [MenuItem("Tools/Setup Build Settings")]
    public static void Setup()
    {
        var scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Act0_Briefing.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Act1_Space.unity",    true),
            new EditorBuildSettingsScene("Assets/Scenes/Act2_Transition.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Act3_Arena.unity",    true),
        };
        EditorBuildSettings.scenes = scenes;
        UnityEngine.Debug.Log("[BuildSettingsSetup] 4 scenes registered in Build Settings.");
    }
}
#endif
