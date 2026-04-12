#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SceneAssembler
{
    static readonly string[] Paths = {
        "Assets/Sprites/Backrounds/SpaceBackround.png",
        "Assets/Sprites/Ships/enemy.png",
        "Assets/Sprites/Ships/enemy2.png",
        "Assets/Sprites/Ships/enemy3.png",
        "Assets/Sprites/Ships/boss.png",
        "Assets/Sprites/Ships/Sharaship.png",
    };

    [MenuItem("Tools/Assemble Scene")]
    public static void Run()
    {
        // 1. Reimport all as Sprite
        foreach (var p in Paths)
        {
            var imp = AssetImporter.GetAtPath(p) as TextureImporter;
            if (imp == null) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.filterMode = FilterMode.Point;
            imp.alphaIsTransparency = true;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.Refresh();

        // 2. Load sprites
        var sBg    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Backrounds/SpaceBackround.png");
        var sE1    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ships/enemy.png");
        var sE2    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ships/enemy2.png");
        var sE3    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ships/enemy3.png");
        var sBoss  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ships/boss.png");
        var sShara = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ships/Sharaship.png");

        Debug.Log("[SA] sBg="    + (sBg    != null ? sBg.name    : "NULL"));
        Debug.Log("[SA] sE1="    + (sE1    != null ? sE1.name    : "NULL"));
        Debug.Log("[SA] sE2="    + (sE2    != null ? sE2.name    : "NULL"));
        Debug.Log("[SA] sE3="    + (sE3    != null ? sE3.name    : "NULL"));
        Debug.Log("[SA] sBoss="  + (sBoss  != null ? sBoss.name  : "NULL"));
        Debug.Log("[SA] sShara=" + (sShara != null ? sShara.name : "NULL"));

        // 3. Delete old starfield GOs
        var sf1 = GameObject.Find("Starfield");
        if (sf1 != null) Object.DestroyImmediate(sf1);
        var sf2 = GameObject.Find("StarfieldFar");
        if (sf2 != null) Object.DestroyImmediate(sf2);
        var old = GameObject.Find("Background");
        if (old != null) Object.DestroyImmediate(old);

        // 4. Create Background
        var bg = new GameObject("Background");
        bg.transform.position = new Vector3(0f, 0f, 1f);
        bg.transform.localScale = new Vector3(20f, 20f, 1f);
        var bgSR = bg.AddComponent<SpriteRenderer>();
        bgSR.sortingOrder = -10;
        bgSR.sprite = sBg;
        Debug.Log("[SA] Background created, sprite=" + (sBg != null ? sBg.name : "NULL"));

        // 5. SharaShip sprite
        var ship = GameObject.Find("SharaShip");
        if (ship != null)
        {
            var sr = ship.GetComponent<SpriteRenderer>();
            if (sr != null && sShara != null) { sr.sprite = sShara; Debug.Log("[SA] SharaShip = Sharaship"); }
        }

        // 6. FormationController row sprites
        var fc = Object.FindObjectOfType<FormationController>();
        if (fc != null)
        {
            fc.row0Sprite = sE1;
            fc.row1Sprite = sE2;
            fc.row2Sprite = sE3;
            fc.row3Sprite = sE1;
            fc.row4Sprite = sE2;
            fc.bossSprite = sBoss;
            EditorUtility.SetDirty(fc);
            Debug.Log("[SA] FormationController sprites assigned.");
        }

        // 7. Enemy prefab default sprite
        var pfb = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (pfb != null && sE1 != null)
        {
            var sr = pfb.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sprite = sE1; EditorUtility.SetDirty(pfb); }
            AssetDatabase.SaveAssetIfDirty(pfb);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[SA] Done.");
    }
}
#endif
