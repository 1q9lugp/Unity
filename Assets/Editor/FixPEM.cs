#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FixPEM
{
    [MenuItem("Tools/Fix PEM Corruption")]
    public static void Run()
    {
        const string path = "Assets/Scripts/Space/PlanetEncounterManager.cs";
        string s = File.ReadAllText(path);

        // Fix 1: corrupted field declarations on line 30
        s = s.Replace(
            "    public AudioClip combatMusic;\n        public AudioClip cinematicSong;maticSong;ialogueMusic;",
            "    public AudioClip combatMusic;\n    public AudioClip dialogueMusic;\n    public AudioClip cinematicSong;"
        );

        // Fix 2: corrupted src.clip line in EndingSequence
        s = s.Replace(
            "        15f; src.volume = 0.8f; src.loop = false;",
            "        src.clip = cinematicSong; src.time = 15f; src.volume = 0.8f; src.loop = false;"
        );

        // Fix 3: remove non-existent Act2Controller reference
        s = s.Replace(
            "        Act2Controller.CinematicMusic = src;\n",
            ""
        );

        File.WriteAllText(path, s);
        AssetDatabase.Refresh();
        Debug.Log("[FixPEM] Done.");
    }
}
#endif
