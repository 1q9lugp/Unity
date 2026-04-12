#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildVariants
{
    [MenuItem("Tools/Build Enemy Variants")]
    public static void Run()
    {
        // ── Reimport sprites as Sprite type ─────────────────────────────────
        string[] pths = {
            "Assets/Sprites/Enemies/enemy.png",
            "Assets/Sprites/Enemies/enemy2.png",
            "Assets/Sprites/Enemies/enemy3.png",
            "Assets/Sprites/Enemies/boss.png",
            "Assets/Sprites/Ships/Sharaship.png",
        };
        foreach (var p in pths)
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

        // ── Load sprites ──────────────────────────────────────────────────
        var sE1    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/enemy.png");
        var sE2    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/enemy2.png");
        var sE3    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/enemy3.png");
        var sBoss  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/boss.png");
        var sShara = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ships/Sharaship.png");
        Debug.Log("[BV] Sprites: E1=" + (sE1!=null?sE1.name:"NULL") + " E2=" + (sE2!=null?sE2.name:"NULL")
            + " E3=" + (sE3!=null?sE3.name:"NULL") + " Boss=" + (sBoss!=null?sBoss.name:"NULL")
            + " Ship=" + (sShara!=null?sShara.name:"NULL"));

        // ── Assign sprites to prefabs ─────────────────────────────────────
        string[] pfbPaths = {
            "Assets/Prefabs/Enemy.prefab",
            "Assets/Prefabs/Enemy2.prefab",
            "Assets/Prefabs/Enemy3.prefab",
            "Assets/Prefabs/Boss.prefab",
        };
        Sprite[] pfbSprites = { sE1, sE2, sE3, sBoss };
        for (int i = 0; i < pfbPaths.Length; i++)
        {
            var pfb = AssetDatabase.LoadAssetAtPath<GameObject>(pfbPaths[i]);
            if (pfb == null) { Debug.LogError("[BV] Missing: " + pfbPaths[i]); continue; }
            var sr = pfb.GetComponent<SpriteRenderer>();
            if (sr != null && pfbSprites[i] != null) sr.sprite = pfbSprites[i];
            EditorUtility.SetDirty(pfb);
            AssetDatabase.SaveAssetIfDirty(pfb);
        }
        Debug.Log("[BV] Sprites assigned to prefabs.");

        // ── Assign Sharaship sprite to SharaShip scene object ─────────────
        var ship = GameObject.Find("SharaShip");
        if (ship != null && sShara != null)
        {
            var sr = ship.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sShara;
            EditorUtility.SetDirty(ship);
        }

        // ── Replace CircleCollider2D with PolygonCollider2D on SharaShip ──
        if (ship != null)
        {
            var circ = ship.GetComponent<CircleCollider2D>();
            if (circ != null) Object.DestroyImmediate(circ);
            var poly = ship.GetComponent<PolygonCollider2D>();
            if (poly == null) poly = ship.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;
            poly.SetPath(0, new Vector2[] {
                new Vector2( 0f,    0.6f),
                new Vector2( 0.4f, -0.3f),
                new Vector2( 0f,   -0.1f),
                new Vector2(-0.4f, -0.3f)
            });
            EditorUtility.SetDirty(ship);
            Debug.Log("[BV] SharaShip PolygonCollider2D set.");
        }

        // ── Write new FormationController.cs ──────────────────────────────
        File.WriteAllText("Assets/Scripts/Space/FormationController.cs",
"using System.Collections.Generic;\n" +
"using UnityEngine;\n" +
"public class FormationController : MonoBehaviour\n" +
"{\n" +
"    [Header(\"Grid\")]\n" +
"    public int columns = 5, rows = 3;\n" +
"    public float colSpacing = 1.8f, rowSpacing = 1.2f;\n" +
"    public float moveSpeed = 1.5f, driftSpeed = 0.05f, bounceX = 3.5f, driftFloor = -2f;\n" +
"    public int miniBossCount;\n" +
"    public float shootInterval = 3f;\n" +
"    public int roundIndex;\n" +
"    [Header(\"Prefabs\")]\n" +
"    public GameObject enemy1Prefab, enemy2Prefab, enemy3Prefab, bossPrefab;\n" +
"    public GameObject projectilePrefab;\n" +
"    [Header(\"Row Sprite Overrides\")]\n" +
"    public Sprite row0Sprite, row1Sprite, row2Sprite, row3Sprite, row4Sprite, bossSprite;\n" +
"    readonly List<EnemyShip> _enemies = new List<EnemyShip>();\n" +
"    float _dir = 1f, _diveTimer = 4f;\n" +
"    bool _active;\n" +
"    Transform _player;\n" +
"    PlanetEncounterManager _mgr;\n\n" +
"    public void Init(int c, int r, float spd, int mb, float shoot, int rndIdx,\n" +
"                     GameObject pp, Transform pl, PlanetEncounterManager m)\n" +
"    {\n" +
"        columns=c; rows=r; moveSpeed=spd; miniBossCount=mb;\n" +
"        shootInterval=shoot; roundIndex=rndIdx;\n" +
"        projectilePrefab=pp; _player=pl; _mgr=m;\n" +
"        SpawnGrid(); _active=true;\n" +
"    }\n\n" +
"    void Update()\n" +
"    {\n" +
"        if (!_active) return;\n" +
"        float dy = -driftSpeed * Time.deltaTime;\n" +
"        if (transform.position.y + dy < driftFloor) dy = 0f;\n" +
"        transform.position += new Vector3(_dir * moveSpeed * Time.deltaTime, dy, 0f);\n" +
"        float px = transform.position.x;\n" +
"        if (px >= bounceX) { transform.position = new Vector3(bounceX, transform.position.y, 0f); _dir = -1f; }\n" +
"        else if (px <= -bounceX) { transform.position = new Vector3(-bounceX, transform.position.y, 0f); _dir = 1f; }\n" +
"        _diveTimer -= Time.deltaTime;\n" +
"        if (_diveTimer <= 0f) { _diveTimer = 4f; TriggerDive(); }\n" +
"    }\n\n" +
"    public Vector2 GetFormationWorldPos(Vector2 lo) => (Vector2)transform.position + lo;\n\n" +
"    public void OnEnemyDestroyed(EnemyShip e)\n" +
"    {\n" +
"        _enemies.Remove(e);\n" +
"        if (_enemies.Count == 0) { _active = false; if (_mgr != null) _mgr.OnRoundCleared(); }\n" +
"    }\n\n" +
"    // Rd 0: all enemy1 | Rd 1: row<2 enemy1 else enemy2\n" +
"    // Rd 2: row<2 enemy2 else enemy3 | Rd 3+: all enemy3\n" +
"    GameObject PickPrefab(int row)\n" +
"    {\n" +
"        if (roundIndex <= 0) return enemy1Prefab;\n" +
"        if (roundIndex == 1) return row < 2 ? enemy1Prefab : enemy2Prefab;\n" +
"        if (roundIndex == 2) return row < 2 ? enemy2Prefab : enemy3Prefab;\n" +
"        return enemy3Prefab;\n" +
"    }\n\n" +
"    Sprite PickSprite(int row)\n" +
"    {\n" +
"        if (row == 0) return row0Sprite;\n" +
"        if (row == 1) return row1Sprite;\n" +
"        if (row == 2) return row2Sprite;\n" +
"        if (row == 3) return row3Sprite;\n" +
"        return row4Sprite;\n" +
"    }\n\n" +
"    void SpawnGrid()\n" +
"    {\n" +
"        foreach (var x in _enemies) if (x != null) Destroy(x.gameObject);\n" +
"        _enemies.Clear();\n" +
"        var bossCols = new HashSet<int>();\n" +
"        if (miniBossCount >= 1) bossCols.Add(columns / 2);\n" +
"        if (miniBossCount >= 2) bossCols.Add(columns / 2 - 1);\n" +
"        float sx = -(columns - 1) * colSpacing * 0.5f;\n" +
"        for (int r = 0; r < rows; r++)\n" +
"        {\n" +
"            for (int c = 0; c < columns; c++)\n" +
"            {\n" +
"                bool isBoss = (r == rows - 1) && bossCols.Contains(c);\n" +
"                var pfb = isBoss ? bossPrefab : PickPrefab(r);\n" +
"                if (pfb == null) pfb = enemy1Prefab;\n" +
"                if (pfb == null) continue;\n" +
"                var go = Instantiate(pfb, transform);\n" +
"                var off = new Vector2(sx + c * colSpacing, 3.5f - r * rowSpacing);\n" +
"                go.transform.localPosition = off;\n" +
"                if (!isBoss) go.transform.localScale = Vector3.one * 0.8f;\n" +
"                var sr = go.GetComponent<SpriteRenderer>();\n" +
"                if (sr != null) { var s = isBoss ? bossSprite : PickSprite(r); if (s != null) sr.sprite = s; }\n" +
"                var es = go.GetComponent<EnemyShip>();\n" +
"                if (es == null) { Destroy(go); continue; }\n" +
"                es.formation = this; es.formationOffset = off;\n" +
"                es.projectilePrefab = projectilePrefab;\n" +
"                es.shootInterval = shootInterval;\n" +
"                _enemies.Add(es);\n" +
"            }\n" +
"        }\n" +
"    }\n\n" +
"    void TriggerDive()\n" +
"    {\n" +
"        float minY = float.MaxValue;\n" +
"        foreach (var e in _enemies) if (e != null && !e.IsDiving()) minY = Mathf.Min(minY, e.formationOffset.y);\n" +
"        if (minY == float.MaxValue) return;\n" +
"        var cands = _enemies.FindAll(e => e != null && !e.IsDiving() && e.formationOffset.y <= minY + 0.01f);\n" +
"        if (cands.Count > 0) StartCoroutine(cands[Random.Range(0, cands.Count)].DiveCoroutine(_player));\n" +
"    }\n" +
"}\n");

        // ── Write new PlanetEncounterManager.cs ───────────────────────────
        File.WriteAllText("Assets/Scripts/Space/PlanetEncounterManager.cs",
"using System.Collections;\n" +
"using UnityEngine;\n" +
"using UnityEngine.UI;\n" +
"public class PlanetEncounterManager : MonoBehaviour\n" +
"{\n" +
"    [Header(\"Enemy Prefabs\")]\n" +
"    public GameObject enemy1Prefab;\n" +
"    public GameObject enemy2Prefab;\n" +
"    public GameObject enemy3Prefab;\n" +
"    public GameObject bossPrefab;\n" +
"    [Header(\"Other\")]\n" +
"    public GameObject playerBeamPrefab, enemyProjectilePrefab;\n" +
"    public SharaShipController player;\n" +
"    static readonly int[]   Cols   = { 5,   5,   6,   6   };\n" +
"    static readonly int[]   Rows   = { 3,   4,   4,   5   };\n" +
"    static readonly float[] Speeds = { 0.8f,1.0f,1.2f,1.4f };\n" +
"    static readonly int[]   Bosses = { 0,   0,   1,   2   };\n" +
"    static readonly float[] Shoots = { 3.5f,2.5f,2.0f,1.5f };\n" +
"    static readonly string[] PNames = { \"VENUS\",\"SATURN\",\"JUPITER\",\"ANDROMEDA\" };\n" +
"    static readonly string[] Reject = {\n" +
"        \"YOUR KIND IS NOT WELCOME ON VENUS\",\n" +
"        \"SATURN'S RINGS REJECT YOUR PASSAGE\",\n" +
"        \"JUPITER'S STORMS WILL CRUSH YOU\",\n" +
"        \"ANDROMEDA WILL NOT YIELD TO VIOLENCE\"\n" +
"    };\n" +
"    int _round; GameObject _fgo; Text _txt; Image _fade; bool _busy;\n" +
"    void Start() { BuildUI(); StartCoroutine(Launch()); }\n" +
"    public void OnRoundCleared() { if (!_busy) StartCoroutine(Transition()); }\n" +
"    IEnumerator Launch()\n" +
"    {\n" +
"        if (_round >= Cols.Length) { Show(\"SHARA HAS REACHED THE STARS\", new Color(0.4f,1f,1f)); yield break; }\n" +
"        if (_fgo != null) Destroy(_fgo);\n" +
"        _fgo = new GameObject(\"Formation_R\" + _round);\n" +
"        _fgo.transform.position = Vector3.zero;\n" +
"        var fc = _fgo.AddComponent<FormationController>();\n" +
"        fc.enemy1Prefab = enemy1Prefab;\n" +
"        fc.enemy2Prefab = enemy2Prefab;\n" +
"        fc.enemy3Prefab = enemy3Prefab;\n" +
"        fc.bossPrefab   = bossPrefab;\n" +
"        fc.Init(Cols[_round], Rows[_round], Speeds[_round], Bosses[_round],\n" +
"                Shoots[_round], _round, enemyProjectilePrefab,\n" +
"                player ? player.transform : null, this);\n" +
"    }\n" +
"    IEnumerator Transition()\n" +
"    {\n" +
"        _busy = true;\n" +
"        int i = Mathf.Clamp(_round, 0, PNames.Length - 1);\n" +
"        yield return StartCoroutine(DoFade(0f, 0.85f, 0.5f));\n" +
"        Show(PNames[i], Color.white);\n" +
"        yield return new WaitForSeconds(2f);\n" +
"        Show(Reject[i], new Color(1f,0.55f,0.15f));\n" +
"        yield return new WaitForSeconds(3f);\n" +
"        Show(\"\", Color.white);\n" +
"        yield return StartCoroutine(DoFade(0.85f, 0f, 0.5f));\n" +
"        _round++; _busy = false; StartCoroutine(Launch());\n" +
"    }\n" +
"    IEnumerator DoFade(float a, float b, float d)\n" +
"    {\n" +
"        for (float t = 0f; t < d; t += Time.deltaTime)\n" +
"        {\n" +
"            if (_fade) _fade.color = new Color(0,0,0, Mathf.Lerp(a,b,t/d));\n" +
"            yield return null;\n" +
"        }\n" +
"        if (_fade) _fade.color = new Color(0,0,0,b);\n" +
"    }\n" +
"    void Show(string s, Color c) { if (_txt) { _txt.text = s; _txt.color = c; } }\n" +
"    void BuildUI()\n" +
"    {\n" +
"        var cv = FindObjectOfType<Canvas>(); if (cv == null) return;\n" +
"        var fg = new GameObject(\"FadePanel\"); fg.transform.SetParent(cv.transform, false);\n" +
"        var fr = fg.AddComponent<RectTransform>();\n" +
"        fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one;\n" +
"        fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;\n" +
"        _fade = fg.AddComponent<Image>(); _fade.color = new Color(0,0,0,0); _fade.raycastTarget = false;\n" +
"        var tg = new GameObject(\"RoundText\"); tg.transform.SetParent(cv.transform, false);\n" +
"        var tr = tg.AddComponent<RectTransform>();\n" +
"        tr.anchorMin = new Vector2(.5f,.5f); tr.anchorMax = new Vector2(.5f,.5f);\n" +
"        tr.pivot = new Vector2(.5f,.5f); tr.anchoredPosition = Vector2.zero; tr.sizeDelta = new Vector2(640f,120f);\n" +
"        _txt = tg.AddComponent<Text>();\n" +
"        _txt.font = Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\");\n" +
"        _txt.fontSize = 32; _txt.fontStyle = FontStyle.Bold;\n" +
"        _txt.alignment = TextAnchor.MiddleCenter; _txt.color = Color.white; _txt.text = \"\";\n" +
"    }\n" +
"}\n");

        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BV] Done — compile will follow.");
    }
}
#endif
