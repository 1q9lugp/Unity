#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Run via menu: Tools > Assign Planet Backgrounds
/// Loads sprites from Assets/Sprites/Backrounds/Space and .../Planets
/// and wires them into PlanetEncounterManager's planets array (index-matched).
/// </summary>
public static class BackgroundAutoAssigner
{
    // Space backgrounds — one per planet (fighting phase), sorted chronologically
    static readonly string[] SpaceGuids =
    {
        "a4b391909c0e4b943bb360bea4a03a90", // SpaceBackround.png        -> Kepler-186f
        "b5b019c3bd8fb2d11861b7f11a16eddc", // 14-34_22-03-2026.png     -> Proxima b
        "193a817a0b48c9b44840cc4124617c99", // 19-41_13-04-2026.png     -> Tau Ceti e
        "0ca21abf00887b456b84d2027bf39f18", // 19-43_13-04-2026.png     -> Gliese 667Cc
        "262fcf3759383eb97a203c1dfeb9f899", // 19-44_13-04-2026.png     -> Wolf 1061c
        "e55047d0e40dc0a829f21134cb494a6a", // 19-46_13-04-2026.png     -> Trappist-1e
        "a48ba151c60ae2f2cbf4a84ccb827ae7", // 19-49_22-03-2026.png     -> Luyten b
        "3cb275b07440537ec88c09d26b8b31fa", // 19-51_13-04-2026.png     -> Ross 128b
    };

    // Planet-surface backgrounds — one per planet (landing phase)
    static readonly string[] PlanetGuids =
    {
        "6e195a2a2fb85d167882c88e543af48a", // 15-53_22-03-2026.png     -> Kepler-186f
        "b0d3be62f38acabed9f6fe26e0dbf67b", // 15-56_22-03-2026.png     -> Proxima b
        "ccda9dbbc96646684933b30627572e5b", // 16-38_22-03-2026.png     -> Tau Ceti e
        "4b3d23f07fb02f047b9d3559f029dfc7", // 19-31_22-03-2026.png     -> Gliese 667Cc
        "62fb4c8bbeddbf240a98d0f621441936", // 19-34_13-04-2026.png     -> Wolf 1061c
        "9caf13ef16479b26f97bcf1662ed4a88", // 19-35_13-04-2026.png     -> Trappist-1e
        "02443f479836a967ba371b4762326c4c", // 19-51_22-03-2026.png     -> Luyten b
        "e787137199394f8418e8f73a90ed67b3", // 20-09_13-04-2026.png     -> Ross 128b
    };

    [MenuItem("Tools/Assign Planet Backgrounds")]
    static void Assign()
    {
        var mgr = Object.FindObjectOfType<PlanetEncounterManager>();
        if (mgr == null)
        {
            Debug.LogError("[BackgroundAutoAssigner] PlanetEncounterManager not found in scene.");
            return;
        }

        var so = new SerializedObject(mgr);
        var planetsProp = so.FindProperty("planets");

        if (planetsProp == null || planetsProp.arraySize != 8)
        {
            Debug.LogError("[BackgroundAutoAssigner] planets array missing or not size 8. " +
                           "Enter Play mode once so Start() populates it, then re-run.");
            return;
        }

        int assigned = 0;
        for (int i = 0; i < 8; i++)
        {
            var elem = planetsProp.GetArrayElementAtIndex(i);

            // Space (fighting) background
            var spaceProp = elem.FindPropertyRelative("spaceBackground");
            if (spaceProp != null)
            {
                string path = AssetDatabase.GUIDToAssetPath(SpaceGuids[i]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) { spaceProp.objectReferenceValue = sprite; assigned++; }
                else Debug.LogWarning("[BackgroundAutoAssigner] Space sprite not found for index " + i + " (guid: " + SpaceGuids[i] + ")");
            }

            // Planet surface background
            var surfProp = elem.FindPropertyRelative("surfaceBackground");
            if (surfProp != null)
            {
                string path = AssetDatabase.GUIDToAssetPath(PlanetGuids[i]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) { surfProp.objectReferenceValue = sprite; assigned++; }
                else Debug.LogWarning("[BackgroundAutoAssigner] Planet sprite not found for index " + i + " (guid: " + PlanetGuids[i] + ")");
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(mgr);

        Debug.Log("[BackgroundAutoAssigner] Done. " + assigned + "/16 sprites assigned.");
    }
}
#endif
