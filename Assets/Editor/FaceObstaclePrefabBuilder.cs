using UnityEngine;
using UnityEditor;

public static class FaceObstaclePrefabBuilder
{
    [MenuItem("Tools/Build FaceObstacle Prefabs")]
    public static void BuildPrefabs()
    {
        // Gather all head FBX paths
        string[] fbxGuids = AssetDatabase.FindAssets("Head_Type t:GameObject",
            new[] { "Assets/Sprites/Backrounds/BERSERK-EclipsePack/Models/Heads" });

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MiddleMaterial.mat");
        if (mat == null) { Debug.LogError("MiddleMaterial not found!"); return; }

        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/FaceObstacles"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "FaceObstacles");

        int created = 0;
        foreach (string guid in fbxGuids)
        {
            string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
            // Only process FBX files
            if (!fbxPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;

            string assetName = System.IO.Path.GetFileNameWithoutExtension(fbxPath);
            string prefabPath = $"Assets/Prefabs/FaceObstacles/{assetName}.prefab";

            // Skip if already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) continue;

            GameObject sourceFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (sourceFbx == null) continue;

            // Instantiate into scene temporarily
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourceFbx);
            instance.name = assetName;

            // Reset transform
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.transform.localScale = Vector3.one;

            // Assign material and configure renderer on root or first child
            MeshRenderer mr = instance.GetComponentInChildren<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Add convex MeshCollider on the GameObject that has the MeshFilter
            MeshFilter mf = instance.GetComponentInChildren<MeshFilter>();
            if (mf != null)
            {
                MeshCollider mc = mf.gameObject.GetComponent<MeshCollider>();
                if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = true;
            }

            // Unlink from source FBX, then save as standalone prefab
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"FaceObstaclePrefabBuilder: Created {created} prefabs in Assets/Prefabs/FaceObstacles/");
    }
}
