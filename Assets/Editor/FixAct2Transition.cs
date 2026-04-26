#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FixAct2Transition
{
    [MenuItem("Tools/Fix Act2 Song Wait")]
    public static void Run()
    {
        const string path = "Assets/Scripts/Act2Controller.cs";
        string s = File.ReadAllText(path);

        s = s.Replace(
            "        CloseDlg();\n\n        SceneManager.LoadScene(4);",
            "        CloseDlg();\n\n        // Wait for cinematic song to finish before transitioning\n        yield return new WaitUntil(delegate { return _src == null || !_src.isPlaying; });\n\n        SceneManager.LoadScene(4);"
        );

        File.WriteAllText(path, s);
        AssetDatabase.Refresh();
        Debug.Log("[FixAct2Transition] Done.");
    }
}
#endif
