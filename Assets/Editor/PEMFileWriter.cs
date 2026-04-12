#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public static class PEMFileWriter
{
    [MenuItem("Tools/WritePEM")]
    public static void Run()
    {
        string path = "Assets/Scripts/Space/PlanetEncounterManager.cs";
        string code =
"using System.Collections;\n" +
"using UnityEngine;\n" +
"using UnityEngine.UI;\n" +
"using UnityEngine.SceneManagement;\n" +
"\n" +
"[System.Serializable]\n" +
"public struct PlanetEncounter\n" +
"{\n" +
"    public string planetName, rejectionMessage;\n" +
"    public Sprite spaceBackground, surfaceBackground;\n" +
"    public int rows, cols, enemyPrefabTier;\n" +
"    public float formationSpeed, enemyFireRate;\n" +
"    public bool hasBoss;\n" +
"}\n" +
"\n" +
"public class PlanetEncounterManager : MonoBehaviour\n" +
"{\n" +
"    public GameObject enemy1Prefab, enemy2Prefab, enemy3Prefab, bossPrefab, enemyProjectilePrefab;\n" +
"    public SharaShipController player;\n" +
"    public SpriteRenderer bgRenderer;\n" +
"    public PlanetEncounter[] planets = new PlanetEncounter[0];\n" +
"\n" +
"    Image _fade; Text _counterTxt, _announceTxt, _dialogueTxt, _planetNameTxt; Image _dialogueBg;\n" +
"    LivesManager _lives; GameObject _fgo; int _idx; bool _roundCleared, _extraLifeUsed;\n" +
"\n" +
"    static readonly Color[] SC = { new Color(1f,0.5f,0.1f), new Color(0.5f,0.1f,0.8f), new Color(0.1f,0.7f,0.7f), new Color(0.8f,0.1f,0.1f), new Color(0.5f,0.3f,0.1f), new Color(0.1f,0.3f,0.9f), new Color(0.5f,0.5f,0.5f), new Color(0.05f,0.2f,0.05f) };\n" +
"\n" +
"    void Awake() { BuildRuntimeUI(); }\n" +
"\n" +
"    void Start()\n" +
"    {\n" +
"        _lives = FindObjectOfType<LivesManager>();\n" +
"        if (_lives != null) _lives.onAllLivesLost = OnLivesGone;\n" +
"        if (planets == null || planets.Length != 8)\n" +
"        {\n" +
"            planets = new PlanetEncounter[] {\n" +
"                new PlanetEncounter{planetName=\"Kepler-186f\", rejectionMessage=\"Oxygen levels incompatible with human biology.\",        rows=2,cols=4,formationSpeed=1.2f,enemyFireRate=3.5f,enemyPrefabTier=1,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Proxima b\",   rejectionMessage=\"Surface radiation would prove fatal within hours.\",      rows=2,cols=5,formationSpeed=1.4f,enemyFireRate=3.2f,enemyPrefabTier=1,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Tau Ceti e\",  rejectionMessage=\"Atmospheric pressure far exceeds human tolerance.\",      rows=3,cols=4,formationSpeed=1.6f,enemyFireRate=3.0f,enemyPrefabTier=1,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Gliese 667Cc\",rejectionMessage=\"Already colonized. We would not impose upon its people.\",rows=3,cols=5,formationSpeed=1.8f,enemyFireRate=2.8f,enemyPrefabTier=2,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Wolf 1061c\",  rejectionMessage=\"Tidal locking makes half the surface uninhabitable.\",    rows=3,cols=5,formationSpeed=2.0f,enemyFireRate=2.5f,enemyPrefabTier=2,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Trappist-1e\", rejectionMessage=\"Magnetic field too weak. Cosmic radiation is constant.\", rows=4,cols=5,formationSpeed=2.2f,enemyFireRate=2.2f,enemyPrefabTier=2,hasBoss=true},\n" +
"                new PlanetEncounter{planetName=\"Luyten b\",    rejectionMessage=\"Gravity is 1.8x Earth normal. Humans would be crushed.\", rows=4,cols=6,formationSpeed=2.4f,enemyFireRate=2.0f,enemyPrefabTier=3,hasBoss=true},\n" +
"                new PlanetEncounter{planetName=\"Ross 128b\",   rejectionMessage=\"No liquid water. No soil. No memory. Not our home.\",     rows=4,cols=6,formationSpeed=2.8f,enemyFireRate=1.8f,enemyPrefabTier=3,hasBoss=true},\n" +
"            };\n" +
"        }\n" +
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
"        if (_announceTxt != null) { _announceTxt.text = \"ASHTAR SHERAN \\u2014 ONE FINAL CHANCE\"; _announceTxt.color = new Color(1f,0.85f,0.1f,1f); _announceTxt.fontSize = 36; }\n" +
"        yield return new WaitForSeconds(2.5f);\n" +
"        if (_announceTxt != null) { _announceTxt.text = \"\"; _announceTxt.color = Color.clear; }\n" +
"    }\n" +
"\n" +
"    IEnumerator RetryPlanet()\n" +
"    {\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.5f));\n" +
"        if (_fgo) { Destroy(_fgo); _fgo = null; }\n" +
"        _roundCleared = false; _extraLifeUsed = false;\n" +
"        if (_lives != null) _lives.ResetLives();\n" +
"        yield return new WaitForSeconds(0.3f);\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.5f));\n" +
"        StartCoroutine(CombatPhase(planets[_idx]));\n" +
"    }\n" +
"\n" +
"    IEnumerator GameLoop()\n" +
"    {\n" +
"        for (_idx = 0; _idx < planets.Length; _idx++) yield return StartCoroutine(RunPlanet());\n" +
"        yield return StartCoroutine(EndingSequence());\n" +
"    }\n" +
"\n" +
"    IEnumerator RunPlanet()\n" +
"    {\n" +
"        _extraLifeUsed = false;\n" +
"        if (_lives != null) _lives.ResetLives();\n" +
"        var p = planets[_idx];\n" +
"        if (bgRenderer != null) { if (p.spaceBackground) bgRenderer.sprite = p.spaceBackground; bgRenderer.color = Color.white; }\n" +
"        if (_counterTxt != null) _counterTxt.text = \"PLANET \" + (_idx + 1) + \" / \" + planets.Length;\n" +
"        if (_planetNameTxt != null) _planetNameTxt.gameObject.SetActive(false);\n" +
"        yield return StartCoroutine(CombatPhase(p));\n" +
"        yield return StartCoroutine(LandingPhase(p));\n" +
"    }\n" +
"\n" +
"    IEnumerator CombatPhase(PlanetEncounter p)\n" +
"    {\n" +
"        _roundCleared = false;\n" +
"        LaunchFormation(p);\n" +
"        yield return new WaitUntil(() => _roundCleared);\n" +
"        if (_fgo) { Destroy(_fgo); _fgo = null; }\n" +
"    }\n" +
"\n" +
"    IEnumerator LandingPhase(PlanetEncounter p)\n" +
"    {\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.8f));\n" +
"        int ti = Mathf.Clamp(_idx, 0, SC.Length - 1);\n" +
"        if (bgRenderer != null) { if (p.surfaceBackground != null) { bgRenderer.sprite = p.surfaceBackground; bgRenderer.color = Color.white; } else bgRenderer.color = SC[ti]; }\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.8f));\n" +
"        if (_planetNameTxt != null) { _planetNameTxt.text = p.planetName; _planetNameTxt.gameObject.SetActive(true); }\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: Sensors confirm \\u2014 \" + p.rejectionMessage, 3.5f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: Understood. Set course for next coordinates.\", 2.5f));\n" +
"        if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(false);\n" +
"        if (_dialogueTxt != null) _dialogueTxt.text = \"\";\n" +
"        if (_planetNameTxt != null) _planetNameTxt.gameObject.SetActive(false);\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.8f));\n" +
"        if (bgRenderer != null) { if (p.spaceBackground) bgRenderer.sprite = p.spaceBackground; bgRenderer.color = Color.white; }\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.8f));\n" +
"    }\n" +
"\n" +
"    IEnumerator EndingSequence()\n" +
"    {\n" +
"        if (_planetNameTxt != null) _planetNameTxt.gameObject.SetActive(false);\n" +
"        if (_counterTxt != null) _counterTxt.text = \"\";\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.8f));\n" +
"        if (bgRenderer != null) bgRenderer.color = Color.black;\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.5f));\n" +
"        yield return StartCoroutine(Panel(\"Eight worlds. Eight failures.\", 3f, 36));\n" +
"        yield return StartCoroutine(Panel(\"The data does not lie, Commander.\", 3f, 36));\n" +
"        yield return StartCoroutine(Panel(\"There is only one planet whose air, water,\\ngravity and memory belong to this species.\", 4f, 28));\n" +
"        yield return StartCoroutine(Panel(\"\", 2f, 36));\n" +
"        yield return StartCoroutine(Panel(\"ASHTAR: Set course for Earth.\", 2f, 40));\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,1f));\n" +
"        SceneManager.LoadScene(2);\n" +
"    }\n" +
"\n" +
"    IEnumerator Panel(string txt, float dur, int sz)\n" +
"    {\n" +
"        if (_announceTxt != null) { _announceTxt.text = txt; _announceTxt.color = Color.white; _announceTxt.fontSize = sz; }\n" +
"        yield return new WaitForSeconds(dur);\n" +
"        if (_announceTxt != null) { _announceTxt.text = \"\"; _announceTxt.color = Color.clear; }\n" +
"    }\n" +
"\n" +
"    public void OnRoundCleared() { _roundCleared = true; }\n" +
"\n" +
"    void LaunchFormation(PlanetEncounter p)\n" +
"    {\n" +
"        if (_fgo) Destroy(_fgo);\n" +
"        _fgo = new GameObject(\"Formation\"); _fgo.transform.position = Vector3.zero;\n" +
"        var fc = _fgo.AddComponent<FormationController>();\n" +
"        fc.Init(p.cols,p.rows,p.formationSpeed,p.hasBoss?1:0,p.enemyFireRate,enemy1Prefab,enemy2Prefab,enemy3Prefab,p.hasBoss?bossPrefab:null,enemyProjectilePrefab,player?player.transform:null,this);\n" +
"        fc.SetTierAndBoss(p.enemyPrefabTier, p.hasBoss);\n" +
"    }\n" +
"\n" +
"    IEnumerator ShowDlg(string msg, float hold)\n" +
"    {\n" +
"        if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(true);\n" +
"        if (_dialogueTxt != null) _dialogueTxt.text = msg;\n" +
"        yield return new WaitForSeconds(hold);\n" +
"    }\n" +
"\n" +
"    IEnumerator FadeTo(float from, float to, float dur)\n" +
"    {\n" +
"        if (_fade == null) { yield return new WaitForSeconds(dur); yield break; }\n" +
"        for (float t = 0f; t < dur; t += Time.deltaTime) { _fade.color = new Color(0,0,0,Mathf.Lerp(from,to,t/dur)); yield return null; }\n" +
"        _fade.color = new Color(0,0,0,to);\n" +
"    }\n" +
"\n" +
"    void BuildRuntimeUI()\n" +
"    {\n" +
"        var cv = FindObjectOfType<Canvas>(); if (cv == null) return;\n" +
"        var fo = new GameObject(\"FadeOverlay\"); fo.transform.SetParent(cv.transform,false);\n" +
"        var fr = fo.AddComponent<RectTransform>(); fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero;\n" +
"        _fade = fo.AddComponent<Image>(); _fade.color=new Color(0,0,0,0); _fade.raycastTarget=false;\n" +
"        var cg = new GameObject(\"PlanetCounter\"); cg.transform.SetParent(cv.transform,false);\n" +
"        var cr=cg.AddComponent<RectTransform>(); cr.anchorMin=new Vector2(1f,1f); cr.anchorMax=new Vector2(1f,1f); cr.pivot=new Vector2(1f,1f); cr.anchoredPosition=new Vector2(-15f,-15f); cr.sizeDelta=new Vector2(220f,30f);\n" +
"        _counterTxt=cg.AddComponent<Text>(); _counterTxt.font=Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _counterTxt.fontSize=20; _counterTxt.fontStyle=FontStyle.Bold; _counterTxt.alignment=TextAnchor.UpperRight; _counterTxt.color=Color.white; _counterTxt.text=\"\";\n" +
"        var ng = new GameObject(\"PlanetNameText\"); ng.transform.SetParent(cv.transform,false);\n" +
"        var nr=ng.AddComponent<RectTransform>(); nr.anchorMin=new Vector2(0f,1f); nr.anchorMax=new Vector2(0f,1f); nr.pivot=new Vector2(0f,1f); nr.anchoredPosition=new Vector2(15f,-15f); nr.sizeDelta=new Vector2(400f,36f);\n" +
"        _planetNameTxt=ng.AddComponent<Text>(); _planetNameTxt.font=Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _planetNameTxt.fontSize=24; _planetNameTxt.fontStyle=FontStyle.Bold; _planetNameTxt.alignment=TextAnchor.UpperLeft; _planetNameTxt.color=Color.white; _planetNameTxt.text=\"\";\n" +
"        ng.SetActive(false);\n" +
"        var ag = new GameObject(\"AnnounceText\"); ag.transform.SetParent(cv.transform,false);\n" +
"        var ar=ag.AddComponent<RectTransform>(); ar.anchorMin=new Vector2(0.5f,0.5f); ar.anchorMax=new Vector2(0.5f,0.5f); ar.pivot=new Vector2(0.5f,0.5f); ar.anchoredPosition=new Vector2(0f,50f); ar.sizeDelta=new Vector2(900f,150f);\n" +
"        _announceTxt=ag.AddComponent<Text>(); _announceTxt.font=Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _announceTxt.fontSize=36; _announceTxt.fontStyle=FontStyle.Bold; _announceTxt.alignment=TextAnchor.MiddleCenter; _announceTxt.color=Color.clear; _announceTxt.text=\"\";\n" +
"        var dp = new GameObject(\"DialoguePanel\"); dp.transform.SetParent(cv.transform,false);\n" +
"        var dr=dp.AddComponent<RectTransform>(); dr.anchorMin=new Vector2(0f,0f); dr.anchorMax=new Vector2(1f,0f); dr.pivot=new Vector2(0.5f,0f); dr.anchoredPosition=Vector2.zero; dr.sizeDelta=new Vector2(0f,90f);\n" +
"        _dialogueBg=dp.AddComponent<Image>(); _dialogueBg.color=new Color(0f,0f,0f,0.82f); dp.SetActive(false);\n" +
"        var dtg = new GameObject(\"DialogueText\"); dtg.transform.SetParent(dp.transform,false);\n" +
"        var dtr=dtg.AddComponent<RectTransform>(); dtr.anchorMin=Vector2.zero; dtr.anchorMax=Vector2.one; dtr.offsetMin=new Vector2(20f,8f); dtr.offsetMax=new Vector2(-20f,-8f);\n" +
"        _dialogueTxt=dtg.AddComponent<Text>(); _dialogueTxt.font=Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\"); _dialogueTxt.fontSize=22; _dialogueTxt.fontStyle=FontStyle.Italic; _dialogueTxt.alignment=TextAnchor.MiddleLeft; _dialogueTxt.color=new Color(0.95f,0.95f,0.8f,1f); _dialogueTxt.text=\"\";\n" +
"        fo.transform.SetAsLastSibling();\n" +
"    }\n" +
"}\n";

        File.WriteAllText(path, code);
        AssetDatabase.ImportAsset(path);
        UnityEngine.Debug.Log("PEM written OK - speeds doubled, AnnounceAnim removed");
    }
}
#endif
