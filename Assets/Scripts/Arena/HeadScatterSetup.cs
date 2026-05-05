#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class HeadScatterSetup
{
    [MenuItem("Tools/Setup Head Scatter")]
    static void Setup()
    {
        var scatter = Object.FindObjectOfType<HeadScatter>();
        if (scatter == null) { Debug.LogError("No HeadScatter in scene."); return; }

        string[] paths =
        {
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_1.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_2.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_3.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_4.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_5.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_6.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType1_FBX/Head_Type1_7.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType2_FBX/Head_Type2_1.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType2_FBX/Head_Type2_2.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType2_FBX/Head_Type2_3.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType2_FBX/Head_Type2_4.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType3_FBX/Head_Type3_1.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType3_FBX/Head_Type3_2.fbx",
            "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads/HeadType3_FBX/Head_Type3_3.fbx",
        };

        var list = new System.Collections.Generic.List<GameObject>();
        foreach (var p in paths)
        {
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (obj != null) list.Add(obj);
            else Debug.LogWarning("HeadScatterSetup: missing " + p);
        }

        Undo.RecordObject(scatter, "Setup Head Scatter");
        scatter.headPrefabs = list.ToArray();
        EditorUtility.SetDirty(scatter);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scatter.gameObject.scene);

        Debug.Log("HeadScatterSetup: assigned " + list.Count + " prefabs. Save the scene now.");
    }
}
#endif