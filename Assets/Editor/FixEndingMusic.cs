#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FixEndingMusic
{
    [MenuItem("Tools/Fix Ending Music")]
    public static void Run()
    {
        const string path = "Assets/Scripts/Space/PlanetEncounterManager.cs";
        string s = File.ReadAllText(path);

        // Remove the entire carrier block — Act2Controller handles cinematic music itself.
        // We keep the _mus.Stop() that already appears before this block.
        int start = s.IndexOf("\n        if (cinematicSong != null)\n        {");
        int end   = s.IndexOf("\n        yield return StartCoroutine(ShowDlg(\"A\u0160TAR: ZEM\u011a.", start);
        if (start < 0 || end < 0)
        {
            Debug.LogError("[FixEndingMusic] Could not locate carrier block. Indices: start=" + start + " end=" + end);
            return;
        }
        s = s.Substring(0, start) + s.Substring(end);
        File.WriteAllText(path, s);
        AssetDatabase.Refresh();
        Debug.Log("[FixEndingMusic] Carrier block removed. Act2Controller handles cinematic music.");
    }
}
#endif
