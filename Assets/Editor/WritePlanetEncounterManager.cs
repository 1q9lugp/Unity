using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class WritePlanetEncounterManager
{
    [MenuItem("Tools/Write PlanetEncounterManager")]
    static void Run()
    {
        string assetPath = "Assets/Scripts/Space/PlanetEncounterManager.cs";
        string fullPath  = Path.Combine(Application.dataPath, "Scripts/Space/PlanetEncounterManager.cs");
        File.WriteAllText(fullPath, GetCode());
        AssetDatabase.ImportAsset(assetPath);
        Debug.Log("[PEM] Written. Wiring music next.");

        // Wire music on PlanetEncounterManager in Act1_Space
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Act1_Space.unity", OpenSceneMode.Single);
        MonoBehaviour pem = null;
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            if (mb.GetType().Name == "PlanetEncounterManager") { pem = mb; break; }

        if (pem != null)
        {
            var dialogueClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pikmin 3 OST_ S.S. Drake.mp3");
            var combatClip   = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Otherworldly Foe.mp3");
            var so = new SerializedObject(pem);
            var dlgProp = so.FindProperty("dialogueMusic");
            var cbtProp = so.FindProperty("combatMusic");
            if (dlgProp != null) dlgProp.objectReferenceValue = dialogueClip;
            if (cbtProp != null) cbtProp.objectReferenceValue = combatClip;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pem);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[PEM] Music wired: dialogue=" + (dialogueClip ? dialogueClip.name : "NULL") + " combat=" + (combatClip ? combatClip.name : "NULL"));
        }
        else Debug.LogWarning("[PEM] PlanetEncounterManager not found in Act1_Space after recompile.");
    }

    static string GetCode() =>
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
"    [Header(\"Prefabs\")]\n" +
"    public GameObject enemy1Prefab, enemy2Prefab, enemy3Prefab, bossPrefab, enemyProjectilePrefab;\n" +
"    public SharaShipController player;\n" +
"    public SpriteRenderer bgRenderer;\n" +
"    [Header(\"Data\")]\n" +
"    public PlanetEncounter[] planets = new PlanetEncounter[0];\n" +
"    public Sprite ashtarIconSprite, ptaahIconSprite, jesusSprite, lizardIconSprite;\n" +
"    [Header(\"Music\")]\n" +
"    public AudioClip dialogueMusic;\n" +
"    public AudioClip combatMusic;\n" +
"    [Header(\"Backgrounds\")]\n" +
"    public Sprite[] spaceBackgrounds  = new Sprite[8];\n" +
"    public Sprite[] planetBackgrounds = new Sprite[8];\n" +
"\n" +
"    Image  _fade, _dialogueBg, _speakerIcon, _jesusImg;\n" +
"    Text   _counterTxt, _announceTxt, _dialogueTxt, _planetNameTxt, _promptTxt;\n" +
"    LivesManager _lives;\n" +
"    AudioSource  _music;\n" +
"    GameObject   _fgo;\n" +
"    int  _idx;\n" +
"    bool _roundCleared, _extraLifeUsed;\n" +
"    GameObject _gameOverPanel;\n" +
"    Image      _gameOverBgImg;\n" +
"    Button     _retryBtn, _menuBtn;\n" +
"    Text       _gameOverSubTxt;\n" +
"\n" +
"    static readonly Color[] SC = {\n" +
"        new Color(1f,0.5f,0.1f), new Color(0.5f,0.1f,0.8f),\n" +
"        new Color(0.1f,0.7f,0.7f), new Color(0.8f,0.1f,0.1f),\n" +
"        new Color(0.5f,0.3f,0.1f), new Color(0.1f,0.3f,0.9f),\n" +
"        new Color(0.5f,0.5f,0.5f), new Color(0.05f,0.2f,0.05f)\n" +
"    };\n" +
"\n" +
"    void Awake()\n" +
"    {\n" +
"        _music = gameObject.AddComponent<AudioSource>();\n" +
"        _music.loop = true; _music.volume = 0.7f;\n" +
"        BuildRuntimeUI();\n" +
"    }\n" +
"\n" +
"    void Start()\n" +
"    {\n" +
"        _lives = FindFirstObjectByType<LivesManager>();\n" +
"        if (_lives != null) _lives.onAllLivesLost = OnLivesGone;\n" +
"        if (bgRenderer == null) { var bg = GameObject.Find(\"Background\"); if (bg) bgRenderer = bg.GetComponent<SpriteRenderer>(); }\n" +
"        if (planets == null || planets.Length == 0)\n" +
"        {\n" +
"            planets = new PlanetEncounter[] {\n" +
"                new PlanetEncounter{planetName=\"Kepler-186f\",  rejectionMessage=\"Oxygen levels incompatible with human biology.\",         rows=2,cols=4,formationSpeed=1.2f,enemyFireRate=3.5f,enemyPrefabTier=1,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Proxima b\",    rejectionMessage=\"Surface radiation would prove fatal within hours.\",       rows=2,cols=5,formationSpeed=1.4f,enemyFireRate=3.2f,enemyPrefabTier=1,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Tau Ceti e\",   rejectionMessage=\"Atmospheric pressure far exceeds human tolerance.\",       rows=3,cols=4,formationSpeed=1.6f,enemyFireRate=3.0f,enemyPrefabTier=1,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Gliese 667Cc\", rejectionMessage=\"Already colonized. We would not impose upon its people.\",rows=3,cols=5,formationSpeed=1.8f,enemyFireRate=2.8f,enemyPrefabTier=2,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Wolf 1061c\",   rejectionMessage=\"Tidal locking makes half the surface uninhabitable.\",     rows=3,cols=5,formationSpeed=2.0f,enemyFireRate=2.5f,enemyPrefabTier=2,hasBoss=false},\n" +
"                new PlanetEncounter{planetName=\"Trappist-1e\",  rejectionMessage=\"Magnetic field too weak. Cosmic radiation is constant.\",  rows=4,cols=5,formationSpeed=2.2f,enemyFireRate=2.2f,enemyPrefabTier=2,hasBoss=true},\n" +
"                new PlanetEncounter{planetName=\"Luyten b\",     rejectionMessage=\"Gravity is 1.8x Earth normal. Humans would be crushed.\",  rows=4,cols=6,formationSpeed=2.4f,enemyFireRate=2.0f,enemyPrefabTier=3,hasBoss=true},\n" +
"                new PlanetEncounter{planetName=\"Ross 128b\",    rejectionMessage=\"No liquid water. No soil. No memory. Not our home.\",      rows=4,cols=6,formationSpeed=2.8f,enemyFireRate=1.8f,enemyPrefabTier=3,hasBoss=true},\n" +
"            };\n" +
"        }\n" +
"        for (int i = 0; i < planets.Length; i++)\n" +
"        {\n" +
"            var tmp = planets[i];\n" +
"            if (spaceBackgrounds  != null && i < spaceBackgrounds.Length  && spaceBackgrounds[i]  != null) tmp.spaceBackground   = spaceBackgrounds[i];\n" +
"            if (planetBackgrounds != null && i < planetBackgrounds.Length && planetBackgrounds[i] != null) tmp.surfaceBackground = planetBackgrounds[i];\n" +
"            planets[i] = tmp;\n" +
"        }\n" +
"        StartCoroutine(GameLoop());\n" +
"    }\n" +
"\n" +
"    void PlayMusic(AudioClip clip)\n" +
"    {\n" +
"        if (_music == null || clip == null) return;\n" +
"        if (_music.clip == clip && _music.isPlaying) return;\n" +
"        _music.clip = clip; _music.Play();\n" +
"    }\n" +
"\n" +
"    void OnLivesGone()\n" +
"    {\n" +
"        if (!_extraLifeUsed) StartCoroutine(JesusReveal());\n" +
"        else StartCoroutine(ShowGameOver());\n" +
"    }\n" +
"\n" +
"    IEnumerator ShowGameOver()\n" +
"    {\n" +
"        Time.timeScale = 0f;\n" +
"        if (player != null) player.gameObject.SetActive(false);\n" +
"        if (_gameOverPanel != null)\n" +
"        {\n" +
"            if (_gameOverBgImg != null && jesusSprite != null) _gameOverBgImg.sprite = jesusSprite;\n" +
"            if (_gameOverSubTxt != null) _gameOverSubTxt.text = \"Me dite, vstna a zkus to znovu.\";\n" +
"            _gameOverPanel.SetActive(true);\n" +
"            _gameOverPanel.transform.SetAsLastSibling();\n" +
"            if (_retryBtn != null) { _retryBtn.onClick.RemoveAllListeners(); _retryBtn.onClick.AddListener(() => { _gameOverPanel.SetActive(false); Time.timeScale=1f; if(player!=null)player.gameObject.SetActive(true); _extraLifeUsed=false; StartCoroutine(RetryPlanet()); }); }\n" +
"            if (_menuBtn  != null) { _menuBtn.onClick.RemoveAllListeners();  _menuBtn.onClick.AddListener(() => { Time.timeScale=1f; SceneManager.LoadScene(0); }); }\n" +
"        }\n" +
"        yield return null;\n" +
"    }\n" +
"\n" +
"    IEnumerator JesusReveal()\n" +
"    {\n" +
"        Time.timeScale = 0f;\n" +
"        if (_jesusImg != null)\n" +
"        {\n" +
"            if (jesusSprite != null) _jesusImg.sprite = jesusSprite;\n" +
"            _jesusImg.preserveAspect = false;\n" +
"            var jrt = _jesusImg.rectTransform;\n" +
"            jrt.anchorMin = Vector2.zero; jrt.anchorMax = Vector2.one; jrt.offsetMin = Vector2.zero; jrt.offsetMax = Vector2.zero;\n" +
"            _jesusImg.transform.SetAsLastSibling();\n" +
"            _jesusImg.gameObject.SetActive(true);\n" +
"            _jesusImg.color = Color.clear;\n" +
"            for (float t=0f;t<0.6f;t+=Time.unscaledDeltaTime) { _jesusImg.color=new Color(1f,1f,1f,Mathf.Lerp(0f,0.85f,t/0.6f)); yield return null; }\n" +
"            _jesusImg.color = new Color(1f,1f,1f,0.85f);\n" +
"        }\n" +
"        if (_dialogueBg != null) _dialogueBg.transform.SetAsLastSibling();\n" +
"        yield return StartCoroutine(ShowDlg(\"JESUS: Ashtar Sheran. I grant thee one final chance. Rise, and defend thy people.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: Thank you, Lord. I will not fail them.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: Commander \\u2014 your life signs are returning. We have you.\",0f));\n" +
"        if (_dialogueBg  !=null) _dialogueBg.gameObject.SetActive(false);\n" +
"        if (_speakerIcon !=null) _speakerIcon.enabled=false;\n" +
"        if (_dialogueTxt !=null) _dialogueTxt.text=\"\";\n" +
"        if (_promptTxt   !=null) _promptTxt.gameObject.SetActive(false);\n" +
"        if (_jesusImg    !=null) { float a0=_jesusImg.color.a; for(float t=0f;t<0.5f;t+=Time.unscaledDeltaTime){_jesusImg.color=new Color(1f,1f,1f,Mathf.Lerp(a0,0f,t/0.5f));yield return null;} _jesusImg.color=Color.clear; _jesusImg.gameObject.SetActive(false); }\n" +
"        Time.timeScale = 1f;\n" +
"        _extraLifeUsed = true;\n" +
"        if (_lives != null) { _lives.lives=1; _lives.RefreshHearts(); }\n" +
"    }\n" +
"\n" +
"    IEnumerator RetryPlanet()\n" +
"    {\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.5f));\n" +
"        if (_fgo) { Destroy(_fgo); _fgo=null; }\n" +
"        _roundCleared=false; _extraLifeUsed=false;\n" +
"        if (_lives!=null) _lives.ResetLives();\n" +
"        yield return new WaitForSeconds(0.3f);\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.5f));\n" +
"        StartCoroutine(CombatPhase(planets[_idx]));\n" +
"    }\n" +
"\n" +
"    IEnumerator GameLoop()\n" +
"    {\n" +
"        PlayMusic(dialogueMusic);\n" +
"        yield return StartCoroutine(IntroDialogue());\n" +
"        for (_idx=0;_idx<planets.Length;_idx++) yield return StartCoroutine(RunPlanet());\n" +
"        yield return StartCoroutine(EndingSequence());\n" +
"    }\n" +
"\n" +
"    IEnumerator IntroDialogue()\n" +
"    {\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.5f));\n" +
"        if (bgRenderer!=null) bgRenderer.color=new Color(0.03f,0.05f,0.12f);\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.8f));\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: Commander Ashtar. Our sensors have identified eight candidate worlds. All show signs of potential habitability.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: Understood, Ptaah. Earth's people need a new home. We will not fail them.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"LIZARD: You will not find what you seek, Sheran. These worlds are ours. Your mission ends here.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: Then we will meet you at every world, and push through every one of them.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: Defensive formations incoming. Weapons hot. Commander \\u2014 begin the approach.\",0f));\n" +
"        if (_dialogueBg  !=null) _dialogueBg.gameObject.SetActive(false);\n" +
"        if (_speakerIcon !=null) _speakerIcon.enabled=false;\n" +
"        if (_dialogueTxt !=null) _dialogueTxt.text=\"\";\n" +
"    }\n" +
"\n" +
"    IEnumerator RunPlanet()\n" +
"    {\n" +
"        _extraLifeUsed=false;\n" +
"        if (_lives!=null) _lives.ResetLives();\n" +
"        var p=planets[_idx];\n" +
"        if (bgRenderer!=null) { if(p.spaceBackground) bgRenderer.sprite=p.spaceBackground; bgRenderer.color=Color.white; }\n" +
"        if (_counterTxt   !=null) _counterTxt.text=\"PLANET \"+(_idx+1)+\" / \"+planets.Length;\n" +
"        if (_planetNameTxt!=null) _planetNameTxt.gameObject.SetActive(false);\n" +
"        yield return StartCoroutine(CombatPhase(p));\n" +
"        yield return StartCoroutine(LandingPhase(p));\n" +
"    }\n" +
"\n" +
"    IEnumerator CombatPhase(PlanetEncounter p)\n" +
"    {\n" +
"        PlayMusic(combatMusic);\n" +
"        _roundCleared=false;\n" +
"        LaunchFormation(p);\n" +
"        yield return new WaitUntil(()=>_roundCleared);\n" +
"        if (_fgo) { Destroy(_fgo); _fgo=null; }\n" +
"        PlayMusic(dialogueMusic);\n" +
"    }\n" +
"\n" +
"    IEnumerator LandingPhase(PlanetEncounter p)\n" +
"    {\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.8f));\n" +
"        int ti=Mathf.Clamp(_idx,0,SC.Length-1);\n" +
"        if (bgRenderer!=null) { if(p.surfaceBackground!=null){bgRenderer.sprite=p.surfaceBackground;bgRenderer.color=Color.white;}else bgRenderer.color=SC[ti]; }\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.8f));\n" +
"        if (_planetNameTxt!=null) { _planetNameTxt.text=p.planetName; _planetNameTxt.gameObject.SetActive(true); }\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: Sensors confirm \\u2014 \"+p.rejectionMessage,0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: Understood. Set course for next coordinates.\",0f));\n" +
"        if (_dialogueBg   !=null) _dialogueBg.gameObject.SetActive(false);\n" +
"        if (_speakerIcon  !=null) _speakerIcon.enabled=false;\n" +
"        if (_dialogueTxt  !=null) _dialogueTxt.text=\"\";\n" +
"        if (_planetNameTxt!=null) _planetNameTxt.gameObject.SetActive(false);\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.8f));\n" +
"        if (bgRenderer!=null) { if(p.spaceBackground) bgRenderer.sprite=p.spaceBackground; bgRenderer.color=Color.white; }\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.8f));\n" +
"    }\n" +
"\n" +
"    IEnumerator EndingSequence()\n" +
"    {\n" +
"        if (_planetNameTxt!=null) _planetNameTxt.gameObject.SetActive(false);\n" +
"        if (_counterTxt   !=null) _counterTxt.text=\"\";\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,0.8f));\n" +
"        if (bgRenderer!=null) bgRenderer.color=Color.black;\n" +
"        yield return StartCoroutine(FadeTo(1f,0f,0.5f));\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: Commander. Eight worlds surveyed. Eight rejections recorded.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: There is only one planet whose air, water, gravity and memory belong to this species.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: I know, old friend. I have always known.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"ASHTAR: Set course for Earth.\",0f));\n" +
"        yield return StartCoroutine(ShowDlg(\"PTAAH: With pleasure, Commander. Setting course for Earth.\",0f));\n" +
"        if (_dialogueBg  !=null) _dialogueBg.gameObject.SetActive(false);\n" +
"        if (_speakerIcon !=null) _speakerIcon.enabled=false;\n" +
"        if (_dialogueTxt !=null) _dialogueTxt.text=\"\";\n" +
"        yield return StartCoroutine(FadeTo(0f,1f,1f));\n" +
"        SceneManager.LoadScene(3);\n" +
"    }\n" +
"\n" +
"    public void OnRoundCleared() { _roundCleared=true; }\n" +
"\n" +
"    void LaunchFormation(PlanetEncounter p)\n" +
"    {\n" +
"        if (_fgo) Destroy(_fgo);\n" +
"        _fgo=new GameObject(\"Formation\");\n" +
"        var fc=_fgo.AddComponent<FormationController>();\n" +
"        fc.Init(p.cols,p.rows,p.formationSpeed,p.hasBoss?1:0,p.enemyFireRate,enemy1Prefab,enemy2Prefab,enemy3Prefab,p.hasBoss?bossPrefab:null,enemyProjectilePrefab,player?player.transform:null,this);\n" +
"        fc.SetTierAndBoss(p.enemyPrefabTier,p.hasBoss);\n" +
"    }\n" +
"\n" +
"    IEnumerator ShowDlg(string msg,float hold)\n" +
"    {\n" +
"        if (_dialogueBg!=null) _dialogueBg.gameObject.SetActive(true);\n" +
"        if (_speakerIcon!=null)\n" +
"        {\n" +
"            Sprite ico=null;\n" +
"            if      (msg.StartsWith(\"ASHTAR:\")) ico=ashtarIconSprite;\n" +
"            else if (msg.StartsWith(\"PTAAH:\"))  ico=ptaahIconSprite;\n" +
"            else if (msg.StartsWith(\"LIZARD:\")) ico=lizardIconSprite;\n" +
"            else if (msg.StartsWith(\"JESUS:\"))  ico=jesusSprite;\n" +
"            _speakerIcon.sprite=ico; _speakerIcon.enabled=ico!=null;\n" +
"        }\n" +
"        if (_dialogueTxt!=null) _dialogueTxt.text=msg;\n" +
"        if (_promptTxt  !=null) _promptTxt.gameObject.SetActive(true);\n" +
"        yield return null;\n" +
"        yield return new WaitUntil(()=>Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.E));\n" +
"        if (_promptTxt!=null) _promptTxt.gameObject.SetActive(false);\n" +
"    }\n" +
"\n" +
"    IEnumerator FadeTo(float from,float to,float dur)\n" +
"    {\n" +
"        if (_fade==null) { yield return new WaitForSeconds(dur); yield break; }\n" +
"        for (float t=0f;t<dur;t+=Time.deltaTime) { _fade.color=new Color(0,0,0,Mathf.Lerp(from,to,t/dur)); yield return null; }\n" +
"        _fade.color=new Color(0,0,0,to);\n" +
"    }\n" +
"\n" +
"    void BuildRuntimeUI()\n" +
"    {\n" +
"        var cv=FindFirstObjectByType<Canvas>(); if(cv==null)return;\n" +
"        Font f=Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\");\n" +
"        var fo=new GameObject(\"FadeOverlay\"); fo.transform.SetParent(cv.transform,false);\n" +
"        var fr=fo.AddComponent<RectTransform>(); fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero;\n" +
"        _fade=fo.AddComponent<Image>(); _fade.color=new Color(0,0,0,0); _fade.raycastTarget=false;\n" +
"        var cg=new GameObject(\"PlanetCounter\"); cg.transform.SetParent(cv.transform,false);\n" +
"        var cr=cg.AddComponent<RectTransform>(); cr.anchorMin=new Vector2(1f,1f); cr.anchorMax=new Vector2(1f,1f); cr.pivot=new Vector2(1f,1f); cr.anchoredPosition=new Vector2(-15f,-15f); cr.sizeDelta=new Vector2(220f,30f);\n" +
"        _counterTxt=cg.AddComponent<Text>(); _counterTxt.font=f; _counterTxt.fontSize=20; _counterTxt.fontStyle=FontStyle.Bold; _counterTxt.alignment=TextAnchor.UpperRight; _counterTxt.color=Color.white; _counterTxt.text=\"\";\n" +
"        var ng=new GameObject(\"PlanetNameText\"); ng.transform.SetParent(cv.transform,false);\n" +
"        var nr=ng.AddComponent<RectTransform>(); nr.anchorMin=new Vector2(0f,1f); nr.anchorMax=new Vector2(0f,1f); nr.pivot=new Vector2(0f,1f); nr.anchoredPosition=new Vector2(15f,-15f); nr.sizeDelta=new Vector2(400f,36f);\n" +
"        _planetNameTxt=ng.AddComponent<Text>(); _planetNameTxt.font=f; _planetNameTxt.fontSize=24; _planetNameTxt.fontStyle=FontStyle.Bold; _planetNameTxt.color=Color.white; _planetNameTxt.text=\"\";\n" +
"        ng.SetActive(false);\n" +
"        var dp=new GameObject(\"DialoguePanel\"); dp.transform.SetParent(cv.transform,false);\n" +
"        var dr=dp.AddComponent<RectTransform>(); dr.anchorMin=new Vector2(0f,0f); dr.anchorMax=new Vector2(1f,0f); dr.pivot=new Vector2(0.5f,0f); dr.anchoredPosition=Vector2.zero; dr.sizeDelta=new Vector2(0f,200f);\n" +
"        _dialogueBg=dp.AddComponent<Image>(); _dialogueBg.color=new Color(0f,0f,0f,0.85f); dp.SetActive(false);\n" +
"        var ig=new GameObject(\"SpeakerIcon\"); ig.transform.SetParent(dp.transform,false);\n" +
"        var ir=ig.AddComponent<RectTransform>(); ir.anchorMin=new Vector2(0f,0f); ir.anchorMax=new Vector2(0f,1f); ir.pivot=new Vector2(0f,0.5f); ir.offsetMin=new Vector2(10f,5f); ir.offsetMax=new Vector2(210f,-5f);\n" +
"        _speakerIcon=ig.AddComponent<Image>(); _speakerIcon.preserveAspect=true; _speakerIcon.enabled=false;\n" +
"        var dtg=new GameObject(\"DialogueText\"); dtg.transform.SetParent(dp.transform,false);\n" +
"        var dtr=dtg.AddComponent<RectTransform>(); dtr.anchorMin=Vector2.zero; dtr.anchorMax=Vector2.one; dtr.offsetMin=new Vector2(225f,8f); dtr.offsetMax=new Vector2(-20f,-8f);\n" +
"        _dialogueTxt=dtg.AddComponent<Text>(); _dialogueTxt.font=f; _dialogueTxt.fontSize=26; _dialogueTxt.fontStyle=FontStyle.Italic; _dialogueTxt.alignment=TextAnchor.MiddleLeft; _dialogueTxt.color=new Color(0.95f,0.95f,0.8f,1f); _dialogueTxt.text=\"\";\n" +
"        var ptg=new GameObject(\"PromptText\"); ptg.transform.SetParent(dp.transform,false);\n" +
"        var ptr=ptg.AddComponent<RectTransform>(); ptr.anchorMin=new Vector2(1f,0f); ptr.anchorMax=new Vector2(1f,0f); ptr.pivot=new Vector2(1f,0f); ptr.anchoredPosition=new Vector2(-14f,8f); ptr.sizeDelta=new Vector2(300f,24f);\n" +
"        _promptTxt=ptg.AddComponent<Text>(); _promptTxt.font=f; _promptTxt.fontSize=16; _promptTxt.fontStyle=FontStyle.Bold; _promptTxt.alignment=TextAnchor.LowerRight; _promptTxt.color=Color.yellow; _promptTxt.text=\"\\u25ba  click / space / E\";\n" +
"        ptg.SetActive(false);\n" +
"        var ag=new GameObject(\"AnnounceText\"); ag.transform.SetParent(cv.transform,false);\n" +
"        var ar=ag.AddComponent<RectTransform>(); ar.anchorMin=new Vector2(0f,0.35f); ar.anchorMax=new Vector2(1f,0.65f); ar.offsetMin=Vector2.zero; ar.offsetMax=Vector2.zero;\n" +
"        _announceTxt=ag.AddComponent<Text>(); _announceTxt.font=f; _announceTxt.fontSize=36; _announceTxt.fontStyle=FontStyle.Bold; _announceTxt.alignment=TextAnchor.MiddleCenter; _announceTxt.color=Color.clear; _announceTxt.text=\"\";\n" +
"        var jg=new GameObject(\"JesusOverlay\"); jg.transform.SetParent(cv.transform,false);\n" +
"        var jr=jg.AddComponent<RectTransform>(); jr.anchorMin=Vector2.zero; jr.anchorMax=Vector2.one; jr.offsetMin=Vector2.zero; jr.offsetMax=Vector2.zero;\n" +
"        _jesusImg=jg.AddComponent<Image>(); _jesusImg.preserveAspect=false; _jesusImg.color=Color.clear; _jesusImg.raycastTarget=false;\n" +
"        jg.SetActive(false);\n" +
"        var gop=new GameObject(\"GameOverPanel\"); gop.transform.SetParent(cv.transform,false); _gameOverPanel=gop;\n" +
"        var gopr=gop.AddComponent<RectTransform>(); gopr.anchorMin=Vector2.zero; gopr.anchorMax=Vector2.one; gopr.offsetMin=Vector2.zero; gopr.offsetMax=Vector2.zero;\n" +
"        _gameOverBgImg=gop.AddComponent<Image>(); _gameOverBgImg.color=Color.white;\n" +
"        var tGO=new GameObject(\"GOTitle\"); tGO.transform.SetParent(gop.transform,false);\n" +
"        var tRT=tGO.AddComponent<RectTransform>(); tRT.anchorMin=new Vector2(0f,0.80f); tRT.anchorMax=new Vector2(1f,0.95f); tRT.offsetMin=Vector2.zero; tRT.offsetMax=Vector2.zero;\n" +
"        var tT=tGO.AddComponent<Text>(); tT.font=f; tT.fontSize=72; tT.fontStyle=FontStyle.Bold; tT.alignment=TextAnchor.MiddleCenter; tT.color=new Color(1f,0.18f,0.18f,1f); tT.text=\"GAME OVER\";\n" +
"        var sGO=new GameObject(\"GOSub\"); sGO.transform.SetParent(gop.transform,false);\n" +
"        var sRT=sGO.AddComponent<RectTransform>(); sRT.anchorMin=new Vector2(0.05f,0.10f); sRT.anchorMax=new Vector2(0.95f,0.20f); sRT.offsetMin=Vector2.zero; sRT.offsetMax=Vector2.zero;\n" +
"        _gameOverSubTxt=sGO.AddComponent<Text>(); _gameOverSubTxt.font=f; _gameOverSubTxt.fontSize=22; _gameOverSubTxt.fontStyle=FontStyle.Italic; _gameOverSubTxt.alignment=TextAnchor.MiddleCenter; _gameOverSubTxt.color=Color.black; _gameOverSubTxt.text=\"\";\n" +
"        var rbGO=new GameObject(\"RetryBtn\"); rbGO.transform.SetParent(gop.transform,false);\n" +
"        var rbRT=rbGO.AddComponent<RectTransform>(); rbRT.anchorMin=new Vector2(0.35f,0.02f); rbRT.anchorMax=new Vector2(0.49f,0.09f); rbRT.offsetMin=Vector2.zero; rbRT.offsetMax=Vector2.zero;\n" +
"        var rbI=rbGO.AddComponent<Image>(); rbI.color=new Color(0.1f,0.45f,0.1f,0.9f); _retryBtn=rbGO.AddComponent<Button>(); _retryBtn.targetGraphic=rbI;\n" +
"        var rL=new GameObject(\"Lbl\"); rL.transform.SetParent(rbGO.transform,false); var rLRT=rL.AddComponent<RectTransform>(); rLRT.anchorMin=Vector2.zero; rLRT.anchorMax=Vector2.one; rLRT.offsetMin=Vector2.zero; rLRT.offsetMax=Vector2.zero;\n" +
"        var rT=rL.AddComponent<Text>(); rT.font=f; rT.fontSize=20; rT.fontStyle=FontStyle.Bold; rT.alignment=TextAnchor.MiddleCenter; rT.color=Color.white; rT.text=\"RETRY\";\n" +
"        var mbGO=new GameObject(\"MenuBtn\"); mbGO.transform.SetParent(gop.transform,false);\n" +
"        var mbRT=mbGO.AddComponent<RectTransform>(); mbRT.anchorMin=new Vector2(0.51f,0.02f); mbRT.anchorMax=new Vector2(0.65f,0.09f); mbRT.offsetMin=Vector2.zero; mbRT.offsetMax=Vector2.zero;\n" +
"        var mbI=mbGO.AddComponent<Image>(); mbI.color=new Color(0.4f,0.1f,0.1f,0.9f); _menuBtn=mbGO.AddComponent<Button>(); _menuBtn.targetGraphic=mbI;\n" +
"        var mL=new GameObject(\"Lbl\"); mL.transform.SetParent(mbGO.transform,false); var mLRT=mL.AddComponent<RectTransform>(); mLRT.anchorMin=Vector2.zero; mLRT.anchorMax=Vector2.one; mLRT.offsetMin=Vector2.zero; mLRT.offsetMax=Vector2.zero;\n" +
"        var mT=mL.AddComponent<Text>(); mT.font=f; mT.fontSize=18; mT.fontStyle=FontStyle.Bold; mT.alignment=TextAnchor.MiddleCenter; mT.color=Color.white; mT.text=\"MAIN MENU\";\n" +
"        gop.SetActive(false);\n" +
"        fo.transform.SetAsLastSibling();\n" +
"    }\n" +
"}\n";
}
