#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// Step 1: Tools/Write Encounter System  (writes game scripts)
// Step 2: Wait for compile
// Step 3: Tools/Wire PEM Scene          (wires inspector refs)
public static class WriteEncounterSystem
{
    [MenuItem("Tools/Write Encounter System")]
    public static void Run()
    {
        File.WriteAllText("Assets/Scripts/Space/PlanetEncounterManager.cs", PEM);
        File.WriteAllText("Assets/Scripts/Space/LivesManager.cs", LIVES);
        AssetDatabase.Refresh();
        Debug.Log("[WriteEncounterSystem] Scripts written. Compile then run Tools/Wire PEM Scene.");
    }

    const string PEM =
"using System.Collections;\n" +
"using UnityEngine;\n" +
"using UnityEngine.UI;\n" +
"using UnityEngine.SceneManagement;\n" +
"\n" +
"[System.Serializable]\n" +
"public struct PlanetEncounter\n" +
"{\n" +
"    public string planetName;\n" +
"    public string rejectionMessage;\n" +
"    public Sprite spaceBackground;\n" +
"    public Sprite surfaceBackground;\n" +
"    public int rows;\n" +
"    public int cols;\n" +
"    public float formationSpeed;\n" +
"    public float enemyFireRate;\n" +
"    public int enemyPrefabTier;\n" +
"    public bool hasBoss;\n" +
"}\n" +
"\n" +
"public class PlanetEncounterManager : MonoBehaviour\n" +
"{\n" +
"    [Header(\"Enemy Prefabs\")]\n" +
"    public GameObject enemy1Prefab, enemy2Prefab, enemy3Prefab, bossPrefab;\n" +
"    [Header(\"Projectiles\")]\n" +
"    public GameObject enemyProjectilePrefab;\n" +
"    [Header(\"Player\")]\n" +
"    public SharaShipController player;\n" +
"    [Header(\"Scene\")]\n" +
"    public SpriteRenderer bgRenderer;\n" +
"    [Header(\"Planets\")]\n" +
"    public PlanetEncounter[] planets = new PlanetEncounter[0];\n" +
"\n" +
"    Image _fade;\n" +
"    Text _counterTxt, _watermarkTxt, _announceTxt, _dialogueTxt;\n" +
"    Image _dialogueBg;\n" +
"    LivesManager _lives;\n" +
"    GameObject _fgo;\n" +
"    int _idx;\n" +
"    bool _roundCleared, _extraLifeUsed;\n" +
"\n" +
"    static readonly Color[] SC = {\n" +
"        new Color(1f,0.5f,0.1f),  new Color(0.5f,0.1f,0.8f),\n" +
"        new Color(0.1f,0.7f,0.7f),new Color(0.8f,0.1f,0.1f),\n" +
"        new Color(0.5f,0.3f,0.1f),new Color(0.1f,0.3f,0.9f),\n" +
"        new Color(0.5f,0.5f,0.5f),new Color(0.05f,0.2f,0.05f),\n" +
"    };\n" +
"\n" +
"    void Start()\n" +
"    {\n" +
"        _lives = FindObjectOfType<LivesManager>();\n" +
"        if (_lives != null) _lives.onAllLivesLost = OnLivesGone;\n" +
"        EnsurePlanets();\n" +
"        BuildUI();\n" +
"        if (bgRenderer == null) { var bg = GameObject.Find(\"Background\"); if (bg) bgRenderer = bg.GetComponent<SpriteRenderer>(); }\n" +
"        StartCoroutine(GameLoop());\n" +
"    }\n" +
"\n" +
"    void OnLivesGone()\n" +
"    {\n" +
"        if (!_extraLifeUsed) { _extraLifeUsed = true; if (_lives != null) { _lives.lives = 1; _lives.RefreshHearts(); } StartCoroutine(ExtraMsg()); }\n" +
"        else StartCoroutine(RetryPlanet());\n" +
"    }\n" +
"\n" +
"    IEnumerator ExtraMsg()\n" +
"    {\n" +
"        SetAnnounce(\"ASHTAR SHERAN — ONE FINAL CHANCE\", new Color(1f,0.85f,0.1f,1f), 36);\n" +
"        yield return new WaitForSeconds(2.5f);\n" +
"        SetAnnounce(\"\", Color.clear, 36);\n" +
"    }\n" +
"\n" +
"    IEnumerator RetryPlanet()\n" +
"    {\n" +
"        yield return StartCoroutine(DoFade(0f,1f,0.5f));\n" +
"        if (_fgo) { Destroy(_fgo); _fgo = null; }\n" +
"        _roundCleared = false; _extraLifeUsed = false;\n" +
"        if (_lives != null) _lives.ResetLives();\n" +
"        yield return new WaitForSeconds(0.3f);\n" +
"        yield return StartCoroutine(DoFade(1f,0f,0.5f));\n" +
"        StartCoroutine(CombatPhase(planets[_idx]));\n" +
"    }\n" +
"\n" +
"    IEnumerator GameLoop()\n" +
"    {\n" +
"        for (_idx = 0; _idx < planets.Length; _idx++)\n" +
"            yield return StartCoroutine(RunPlanet());\n" +
"        yield return StartCoroutine(EndingSequence());\n" +
"    }\n" +
"\n" +
"    IEnumerator RunPlanet()\n" +
"    {\n" +
"        _extraLifeUsed = false;\n" +
"        if (_lives != null) _lives.ResetLives();\n" +
"        var p = planets[_idx];\n" +
"        if (bgRenderer != null) { if (p.spaceBackground) bgRenderer.sprite = p.spaceBackground; bgRenderer.color = Color.white; }\n" +
"        UpdateCounter();\n" +
"        if (_watermarkTxt != null) _watermarkTxt.text = p.planetName;\n" +
"        yield return StartCoroutine(CombatPhase(p));\n" +
"        yield return StartCoroutine(LandingPhase(p));\n" +
"    }\n" +
"\n" +
"    IEnumerator CombatPhase(PlanetEncounter p)\n" +
"    {\n" +
"        _roundCleared = false;\n" +
"        yield return StartCoroutine(AnnounceAnim(p.planetName, Color.white, 0.5f, 2f, 0.5f));\n" +
"        SpawnFormation(p);\n" +
"        yield return new WaitUntil(() => _roundCleared);\n" +
"        if (_fgo) { Destroy(_fgo); _fgo = null; }\n" +
"    }\n" +
"\n" +
"    IEnumerator LandingPhase(PlanetEncounter p)\n" +
"    {\n" +
"        yield return StartCoroutine(DoFade(0f,1f,0.8f));\n" +
"        int ti = Mathf.Clamp(_idx, 0, SC.Length - 1);\n" +
"        if (bgRenderer != null)\n" +
"        {\n" +
"            if (p.surfaceBackground != null) { bgRenderer.sprite = p.surfaceBackground; bgRenderer.color = Color.white; }\n" +
"            else bgRenderer.color = SC[ti];\n" +
"        }\n" +
"        yield return StartCoroutine(DoFade(1f,0f,0.8f));\n" +
"        yield return StartCoroutine(Dlg(\"PTAAH: Sensors confirm \\u2014 \" + p.rejectionMessage, 3.5f));\n" +
"        yield return StartCoroutine(Dlg(\"ASHTAR: Understood. Set course for next coordinates.\", 2.5f));\n" +
"        HideDlg();\n" +
"        yield return StartCoroutine(DoFade(0f,1f,0.8f));\n" +
"        if (bgRenderer != null) { if (p.spaceBackground) bgRenderer.sprite = p.spaceBackground; bgRenderer.color = Color.white; }\n" +
"        yield return StartCoroutine(DoFade(1f,0f,0.8f));\n" +
"    }\n" +
"\n" +
"    IEnumerator EndingSequence()\n" +
"    {\n" +
"        if (_watermarkTxt != null) _watermarkTxt.text = \"\";\n" +
"        if (_counterTxt != null) _counterTxt.text = \"\";\n" +
"        yield return StartCoroutine(DoFade(0f,1f,0.8f));\n" +
"        if (bgRenderer != null) bgRenderer.color = Color.black;\n" +
"        yield return StartCoroutine(DoFade(1f,0f,0.5f));\n" +
"        yield return StartCoroutine(ShowPanel(\"Eight worlds. Eight failures.\", 3f, 36));\n" +
"        yield return StartCoroutine(ShowPanel(\"The data does not lie, Commander.\", 3f, 36));\n" +
"        yield return StartCoroutine(ShowPanel(\"There is only one planet whose air, water,\\ngravity and memory belong to this species.\", 4f, 28));\n" +
"        yield return StartCoroutine(ShowPanel(\"\", 2f, 36));\n" +
"        yield return StartCoroutine(ShowPanel(\"ASHTAR: Set course for Earth.\", 2f, 40));\n" +
"        yield return StartCoroutine(DoFade(0f,1f,1f));\n" +
"        SceneManager.LoadScene(2);\n" +
"    }\n" +
"\n" +
"    IEnumerator ShowPanel(string txt, float dur, int sz)\n" +
"    {\n" +
"        SetAnnounce(txt, Color.white, sz);\n" +
"        yield return new WaitForSeconds(dur);\n" +
"        SetAnnounce(\"\", Color.clear, sz);\n" +
"    }\n" +
"\n" +
"    public void OnRoundCleared() { _roundCleared = true; }\n" +
"\n" +
"    void SpawnFormation(PlanetEncounter p)\n" +
"    {\n" +
"        if (_fgo) Destroy(_fgo);\n" +
"        _fgo = new GameObject(\"Formation\"); _fgo.transform.position = Vector3.zero;\n" +
"        var fc = _fgo.AddComponent<FormationController>();\n" +
"        GameObject ep = p.enemyPrefabTier >= 3 ? enemy3Prefab : (p.enemyPrefabTier == 2 ? enemy2Prefab : enemy1Prefab);\n" +
"        fc.Init(p.cols, p.rows, p.formationSpeed, p.hasBoss ? 1 : 0, p.enemyFireRate,\n" +
"                ep, ep, ep, p.hasBoss ? bossPrefab : null,\n" +
"                enemyProjectilePrefab, player ? player.transform : null, this);\n" +
"    }\n" +
"\n" +
"    void UpdateCounter() { if (_counterTxt != null) _counterTxt.text = \"PLANET \" + (_idx + 1) + \" / \" + planets.Length; }\n" +
"\n" +
"    void SetAnnounce(string s, Color c, int sz) { if (_announceTxt == null) return; _announceTxt.text = s; _announceTxt.color = c; _announceTxt.fontSize = sz; }\n" +
"\n" +
"    IEnumerator AnnounceAnim(string txt, Color col, float fi, float hold, float fo)\n" +
"    {\n" +
"        if (_announceTxt == null) { yield return new WaitForSeconds(fi + hold + fo); yield break; }\n" +
"        _announceTxt.text = txt; _announceTxt.fontSize = 36;\n" +
"        for (float t = 0f; t < fi; t += Time.deltaTime) { _announceTxt.color = new Color(col.r,col.g,col.b,t/fi); yield return null; }\n" +
"        _announceTxt.color = col; yield return new WaitForSeconds(hold);\n" +
"        for (float t = 0f; t < fo; t += Time.deltaTime) { _announceTxt.color = new Color(col.r,col.g,col.b,1f-t/fo); yield return null; }\n" +
"        _announceTxt.text = \"\"; _announceTxt.color = Color.clear;\n" +
"    }\n" +
"\n" +
"    IEnumerator Dlg(string msg, float hold)\n" +
"    {\n" +
"        if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(true);\n" +
"        if (_dialogueTxt != null) _dialogueTxt.text = msg;\n" +
"        yield return new WaitForSeconds(hold);\n" +
"    }\n" +
"\n" +
"    void HideDlg() { if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(false); if (_dialogueTxt != null) _dialogueTxt.text = \"\"; }\n" +
"\n" +
"    IEnumerator DoFade(float from, float to, float dur)\n" +
"    {\n" +
"        if (_fade == null) { yield return new WaitForSeconds(dur); yield break; }\n" +
"        for (float t = 0f; t < dur; t += Time.deltaTime) { _fade.color = new Color(0,0,0,Mathf.Lerp(from,to,t/dur)); yield return null; }\n" +
"        _fade.color = new Color(0,0,0,to);\n" +
"    }\n" +
"\n" +
"    void BuildUI()\n" +
"    {\n" +
"        var cv = FindObjectOfType<Canvas>(); if (cv == null) return;\n" +
"        var fo = new GameObject(\"FadeOverlay\"); fo.transform.SetParent(cv.transform, false);\n" +
"        var fr = fo.AddComponent<RectTransform>(); fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;\n" +
"        _fade = fo.AddComponent<Image>(); _fade.color = new Color(0,0,0,0); _fade.raycastTarget = false; fo.transform.SetAsLastSibling();\n" +
"        var cg = new GameObject(\"PlanetCounter\"); cg.transform.SetParent(cv.transform, false);\n" +
"        var cr = cg.AddComponent<RectTransform>(); cr.anchorMin = new Vector2(1f,1f); cr.anchorMax = new Vector2(1f,1f); cr.pivot = new Vector2(1f,1f); cr.anchoredPosition = new Vector2(-15f,-15f); cr.sizeDelta = new Vector2(220f,30f);\n" +
"        _counterTxt = cg.AddComponent<Text>(); _counterTxt.font = Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _counterTxt.fontSize = 20; _counterTxt.fontStyle = FontStyle.Bold; _counterTxt.alignment = TextAnchor.UpperRight; _counterTxt.color = Color.white; _counterTxt.text = \"\";\n" +
"        var wg = new GameObject(\"PlanetWatermark\"); wg.transform.SetParent(cv.transform, false);\n" +
"        var wr = wg.AddComponent<RectTransform>(); wr.anchorMin = new Vector2(0.5f,0.5f); wr.anchorMax = new Vector2(0.5f,0.5f); wr.pivot = new Vector2(0.5f,0.5f); wr.anchoredPosition = Vector2.zero; wr.sizeDelta = new Vector2(900f,120f);\n" +
"        _watermarkTxt = wg.AddComponent<Text>(); _watermarkTxt.font = Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _watermarkTxt.fontSize = 60; _watermarkTxt.fontStyle = FontStyle.Bold; _watermarkTxt.alignment = TextAnchor.MiddleCenter; _watermarkTxt.color = new Color(1f,1f,1f,0.15f); _watermarkTxt.text = \"\";\n" +
"        var ag = new GameObject(\"AnnounceText\"); ag.transform.SetParent(cv.transform, false);\n" +
"        var ar = ag.AddComponent<RectTransform>(); ar.anchorMin = new Vector2(0.5f,0.5f); ar.anchorMax = new Vector2(0.5f,0.5f); ar.pivot = new Vector2(0.5f,0.5f); ar.anchoredPosition = new Vector2(0f,50f); ar.sizeDelta = new Vector2(900f,150f);\n" +
"        _announceTxt = ag.AddComponent<Text>(); _announceTxt.font = Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _announceTxt.fontSize = 36; _announceTxt.fontStyle = FontStyle.Bold; _announceTxt.alignment = TextAnchor.MiddleCenter; _announceTxt.color = Color.clear; _announceTxt.text = \"\";\n" +
"        var dp = new GameObject(\"DialoguePanel\"); dp.transform.SetParent(cv.transform, false);\n" +
"        var dr = dp.AddComponent<RectTransform>(); dr.anchorMin = new Vector2(0f,0f); dr.anchorMax = new Vector2(1f,0f); dr.pivot = new Vector2(0.5f,0f); dr.anchoredPosition = Vector2.zero; dr.sizeDelta = new Vector2(0f,90f);\n" +
"        _dialogueBg = dp.AddComponent<Image>(); _dialogueBg.color = new Color(0f,0f,0f,0.82f); dp.SetActive(false);\n" +
"        var dtg = new GameObject(\"DialogueText\"); dtg.transform.SetParent(dp.transform, false);\n" +
"        var dtr = dtg.AddComponent<RectTransform>(); dtr.anchorMin = Vector2.zero; dtr.anchorMax = Vector2.one; dtr.offsetMin = new Vector2(20f,8f); dtr.offsetMax = new Vector2(-20f,-8f);\n" +
"        _dialogueTxt = dtg.AddComponent<Text>(); _dialogueTxt.font = Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _dialogueTxt.fontSize = 22; _dialogueTxt.fontStyle = FontStyle.Italic; _dialogueTxt.alignment = TextAnchor.MiddleLeft; _dialogueTxt.color = new Color(0.95f,0.95f,0.8f,1f); _dialogueTxt.text = \"\";\n" +
"        fo.transform.SetAsLastSibling();\n" +
"    }\n" +
"\n" +
"    void EnsurePlanets()\n" +
"    {\n" +
"        if (planets != null && planets.Length == 8) return;\n" +
"        planets = new PlanetEncounter[]{\n" +
"            new PlanetEncounter{planetName=\"Kepler-186f\",rejectionMessage=\"Oxygen levels incompatible with human biology.\",rows=2,cols=4,formationSpeed=0.6f,enemyFireRate=3.5f,enemyPrefabTier=1,hasBoss=false},\n" +
"            new PlanetEncounter{planetName=\"Proxima b\",rejectionMessage=\"Surface radiation would prove fatal within hours.\",rows=2,cols=5,formationSpeed=0.7f,enemyFireRate=3.2f,enemyPrefabTier=1,hasBoss=false},\n" +
"            new PlanetEncounter{planetName=\"Tau Ceti e\",rejectionMessage=\"Atmospheric pressure far exceeds human tolerance.\",rows=3,cols=4,formationSpeed=0.8f,enemyFireRate=3.0f,enemyPrefabTier=1,hasBoss=false},\n" +
"            new PlanetEncounter{planetName=\"Gliese 667Cc\",rejectionMessage=\"Already colonized. We would not impose upon its people.\",rows=3,cols=5,formationSpeed=0.9f,enemyFireRate=2.8f,enemyPrefabTier=2,hasBoss=false},\n" +
"            new PlanetEncounter{planetName=\"Wolf 1061c\",rejectionMessage=\"Tidal locking makes half the surface uninhabitable.\",rows=3,cols=5,formationSpeed=1.0f,enemyFireRate=2.5f,enemyPrefabTier=2,hasBoss=false},\n" +
"            new PlanetEncounter{planetName=\"Trappist-1e\",rejectionMessage=\"Magnetic field too weak. Cosmic radiation is constant.\",rows=4,cols=5,formationSpeed=1.1f,enemyFireRate=2.2f,enemyPrefabTier=2,hasBoss=true},\n" +
"            new PlanetEncounter{planetName=\"Luyten b\",rejectionMessage=\"Gravity is 1.8x Earth normal. Humans would be crushed.\",rows=4,cols=6,formationSpeed=1.2f,enemyFireRate=2.0f,enemyPrefabTier=3,hasBoss=true},\n" +
"            new PlanetEncounter{planetName=\"Ross 128b\",rejectionMessage=\"No liquid water. No soil. No memory. Not our home.\",rows=4,cols=6,formationSpeed=1.4f,enemyFireRate=1.8f,enemyPrefabTier=3,hasBoss=true},\n" +
"        };\n" +
"    }\n" +
"}\n";

    const string LIVES =
"using System.Collections;\n" +
"using UnityEngine;\n" +
"using UnityEngine.UI;\n" +
"using UnityEngine.SceneManagement;\n" +
"\n" +
"public class LivesManager : MonoBehaviour\n" +
"{\n" +
"    [Header(\"Lives\")]\n" +
"    public int lives = 3;\n" +
"    public Image heart1, heart2, heart3;\n" +
"    [Header(\"UI\")]\n" +
"    public GameObject gameOverPanel;\n" +
"    public System.Action onAllLivesLost;\n" +
"    Image[] _hearts;\n" +
"\n" +
"    static void FindHearts(ref Image h1, ref Image h2, ref Image h3)\n" +
"    {\n" +
"        var ld = GameObject.Find(\"LivesDisplay\");\n" +
"        if (ld == null) return;\n" +
"        if (h1 == null) { var t = ld.transform.Find(\"Heart1\"); if (t) h1 = t.GetComponent<Image>(); }\n" +
"        if (h2 == null) { var t = ld.transform.Find(\"Heart2\"); if (t) h2 = t.GetComponent<Image>(); }\n" +
"        if (h3 == null) { var t = ld.transform.Find(\"Heart3\"); if (t) h3 = t.GetComponent<Image>(); }\n" +
"    }\n" +
"\n" +
"    void Awake() { FindHearts(ref heart1, ref heart2, ref heart3); _hearts = new Image[]{heart1,heart2,heart3}; }\n" +
"\n" +
"    void Start()\n" +
"    {\n" +
"        FindHearts(ref heart1, ref heart2, ref heart3);\n" +
"        _hearts = new Image[]{heart1,heart2,heart3};\n" +
"        if (gameOverPanel == null) gameOverPanel = GameObject.Find(\"GameOverPanel\");\n" +
"        ResetLives();\n" +
"        if (gameOverPanel == null) return;\n" +
"        var btn = gameOverPanel.GetComponentInChildren<Button>();\n" +
"        if (btn == null) return;\n" +
"        btn.onClick.RemoveAllListeners();\n" +
"        btn.onClick.AddListener(() => { ResetLives(); SceneManager.LoadScene(1); });\n" +
"    }\n" +
"\n" +
"    public void LoseLife()\n" +
"    {\n" +
"        if (lives <= 0) return;\n" +
"        lives--;\n" +
"        for (int i = _hearts.Length - 1; i >= 0; i--)\n" +
"            if (_hearts[i] != null && _hearts[i].gameObject.activeSelf) { _hearts[i].gameObject.SetActive(false); break; }\n" +
"        if (lives <= 0) OnGameOver();\n" +
"    }\n" +
"\n" +
"    public void OnGameOver()\n" +
"    {\n" +
"        if (onAllLivesLost != null) onAllLivesLost.Invoke();\n" +
"        else if (gameOverPanel != null) gameOverPanel.SetActive(true);\n" +
"    }\n" +
"\n" +
"    public void RefreshHearts()\n" +
"    {\n" +
"        if (_hearts == null) _hearts = new Image[]{heart1,heart2,heart3};\n" +
"        for (int i = 0; i < _hearts.Length; i++)\n" +
"            if (_hearts[i] != null) _hearts[i].gameObject.SetActive(i < lives);\n" +
"    }\n" +
"\n" +
"    public void ResetLives()\n" +
"    {\n" +
"        lives = 3;\n" +
"        if (_hearts == null) _hearts = new Image[]{heart1,heart2,heart3};\n" +
"        foreach (var h in _hearts) if (h != null) h.gameObject.SetActive(true);\n" +
"        if (gameOverPanel != null) gameOverPanel.SetActive(false);\n" +
"    }\n" +
"}\n";
}
#endif
