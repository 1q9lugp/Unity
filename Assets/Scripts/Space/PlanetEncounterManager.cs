using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct PlanetEncounter
{
    public string planetName, rejectionMessage;
    public Sprite spaceBackground, surfaceBackground;
    public int rows, cols, enemyPrefabTier;
    public float formationSpeed, enemyFireRate;
    public bool hasBoss;
}

public class PlanetEncounterManager : MonoBehaviour
{
    public GameObject enemy1Prefab, enemy2Prefab, enemy3Prefab, bossPrefab, enemyProjectilePrefab;
    public SharaShipController player;
    public SpriteRenderer bgRenderer;
    public PlanetEncounter[] planets = new PlanetEncounter[0];
    public Sprite ashtarIconSprite, ptaahIconSprite, jesusSprite, lizardIconSprite;

    [Header("Backgrounds")]
    public Sprite[] spaceBackgrounds = new Sprite[8];
    public Sprite[] planetBackgrounds = new Sprite[8];

    [Header("Music")]
    public AudioClip combatMusic;
    public AudioClip dialogueMusic;
    public AudioClip cinematicSong;    AudioSource _mus;

    Image _fade, _dialogueBg, _speakerIcon, _jesusImg;
    Text _counterTxt, _announceTxt, _dialogueTxt, _planetNameTxt, _promptTxt;
    Button _retryBtn, _menuBtn;
    LivesManager _lives;
    GameObject _fgo;
    int _idx;
    bool _roundCleared, _extraLifeUsed;

    static readonly Color[] SC = {
        Color.red, Color.blue, Color.cyan, Color.red,
        Color.yellow, Color.blue, Color.gray, Color.green
    };

    void Awake() { BuildRuntimeUI(); }

    void Start()
    {
        _mus = GetComponent<AudioSource>();
        if (_mus == null) _mus = gameObject.AddComponent<AudioSource>();
        _mus.loop = true;
        _mus.playOnAwake = false;

        _lives = FindObjectOfType<LivesManager>();
        if (_lives != null) _lives.onAllLivesLost = OnLivesGone;
        if (bgRenderer == null) { var bg = GameObject.Find("Background"); if (bg) bgRenderer = bg.GetComponent<SpriteRenderer>(); }
        
        if (planets == null || planets.Length != 8)
        {
            planets = new PlanetEncounter[]
            {
                new PlanetEncounter{planetName="Kepler-186f",  rejectionMessage="Hladina kyslíku není slučitelná s lidskou biologií.",                 rows=2,cols=4,formationSpeed=1.2f,enemyFireRate=3.5f,enemyPrefabTier=1,hasBoss=false},
                new PlanetEncounter{planetName="Proxima b",    rejectionMessage="Povrchová radiace by se během několika hodin ukázala jako smrtelná.", rows=2,cols=5,formationSpeed=1.4f,enemyFireRate=3.2f,enemyPrefabTier=1,hasBoss=false},
                new PlanetEncounter{planetName="Tau Ceti e",   rejectionMessage="Atmosférický tlak zdaleka přesahuje lidskou toleranci.",              rows=3,cols=4,formationSpeed=1.6f,enemyFireRate=3.0f,enemyPrefabTier=1,hasBoss=false},
                new PlanetEncounter{planetName="Gliese 667Cc", rejectionMessage="Již kolonizováno. Nebudeme se vnucovat tamním obyvatelům.",           rows=3,cols=5,formationSpeed=1.8f,enemyFireRate=2.8f,enemyPrefabTier=2,hasBoss=false},
                new PlanetEncounter{planetName="Wolf 1061c",   rejectionMessage="Vázaná rotace činí polovinu povrchu neobyvatelnou.",                 rows=3,cols=5,formationSpeed=2.0f,enemyFireRate=2.5f,enemyPrefabTier=2,hasBoss=false},
                new PlanetEncounter{planetName="Trappist-1e",  rejectionMessage="Magnetické pole je příliš slabé. Kosmické záření je konstantní.",      rows=4,cols=5,formationSpeed=2.2f,enemyFireRate=2.2f,enemyPrefabTier=2,hasBoss=true},
                new PlanetEncounter{planetName="Luyten b",     rejectionMessage="Gravitace je silnější než na Zemi. Lidé by byli rozdrceni.",     rows=4,cols=6,formationSpeed=2.4f,enemyFireRate=2.0f,enemyPrefabTier=3,hasBoss=true},
                new PlanetEncounter{planetName="Ross 128b",    rejectionMessage="Žádná kapalná voda. Žádná půda.",  rows=4,cols=6,formationSpeed=2.8f,enemyFireRate=1.8f,enemyPrefabTier=3,hasBoss=true},
            };
        }
        
        for (int i = 0; i < planets.Length; i++)
        {
            var tmp = planets[i];
            if (spaceBackgrounds != null && i < spaceBackgrounds.Length && spaceBackgrounds[i] != null) tmp.spaceBackground = spaceBackgrounds[i];
            if (planetBackgrounds != null && i < planetBackgrounds.Length && planetBackgrounds[i] != null) tmp.surfaceBackground = planetBackgrounds[i];
            planets[i] = tmp;
        }
        StartCoroutine(GameLoop());
    }

    void OnLivesGone()
    {
        if (!_extraLifeUsed) StartCoroutine(JesusReveal());
        else StartCoroutine(ShowGameOver());
    }

    public void OnRoundCleared() { _roundCleared = true; }

    IEnumerator JesusReveal()
    {
        Time.timeScale = 0f;
        yield return StartCoroutine(FadeInJesus());
        if (_dialogueBg != null) _dialogueBg.transform.SetAsLastSibling();
        yield return StartCoroutine(ShowDlg("JEŽÍŠ: Aštare Šerane. Stojíš na prahu. Uděluji Ti poslední šanci.", 0f));
        yield return StartCoroutine(ShowDlg("JEŽÍŠ: Povstaň, veliteli. Tvůj lid tě stále potřebuje.", 0f));
        yield return StartCoroutine(ShowDlg("AŠTAR: Děkuji, Pane. Nezklamu je.", 0f));
        yield return StartCoroutine(ShowDlg("PTAAH: Veliteli — Vaše životní funkce se vrací. Máme Vás.", 0f));
        CloseDlg();
        yield return StartCoroutine(FadeOutJesus());
        Time.timeScale = 1f;
        _extraLifeUsed = true;
        if (_lives != null) { _lives.lives = 1; _lives.RefreshHearts(); }
    }

    IEnumerator ShowGameOver()
    {
        Time.timeScale = 0f;
        if (player != null) player.gameObject.SetActive(false);
        yield return StartCoroutine(FadeInJesus());
        if (_dialogueBg != null) _dialogueBg.transform.SetAsLastSibling();
        yield return StartCoroutine(ShowDlg("JEŽÍŠ: Aštare Šerane. Bitva je ztracena.", 0f));
        yield return StartCoroutine(ShowDlg("JEŽÍŠ: Bojoval jsi se ctí. Nyní si zvol — návrat k odpočinku, nebo opětovné povstání.", 0f));
        yield return StartCoroutine(ShowDlg("AŠTAR: Neopustím je. Ne teď. Nikdy.", 0f));
        CloseDlg();
        if (_retryBtn != null) _retryBtn.gameObject.SetActive(true);
        if (_menuBtn != null) _menuBtn.gameObject.SetActive(true);
    }

    IEnumerator FadeInJesus()
    {
        if (_jesusImg == null) yield break;
        if (jesusSprite != null) _jesusImg.sprite = jesusSprite;
        _jesusImg.preserveAspect = false;
        var jrt = _jesusImg.rectTransform;
        jrt.anchorMin = Vector2.zero; jrt.anchorMax = Vector2.one;
        jrt.offsetMin = Vector2.zero; jrt.offsetMax = Vector2.zero;
        _jesusImg.gameObject.SetActive(true);
        _jesusImg.color = new Color(1f, 1f, 1f, 0f);
        for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
        {
            _jesusImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 0.85f, t / 0.6f));
            yield return null;
        }
        _jesusImg.color = new Color(1f, 1f, 1f, 0.85f);
    }

    IEnumerator FadeOutJesus()
    {
        if (_jesusImg == null) yield break;
        float a0 = _jesusImg.color.a;
        for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime)
        {
            _jesusImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(a0, 0f, t / 0.5f));
            yield return null;
        }
        _jesusImg.color = Color.clear;
        _jesusImg.gameObject.SetActive(false);
    }

    void CloseDlg()
    {
        if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(false);
        if (_speakerIcon != null) _speakerIcon.enabled = false;
        if (_dialogueTxt != null) _dialogueTxt.text = "";
        if (_promptTxt != null) _promptTxt.gameObject.SetActive(false);
    }

    IEnumerator RetryPlanet()
    {
        yield return StartCoroutine(FadeTo(0f, 1f, 0.5f));
        if (_fgo) { Destroy(_fgo); _fgo = null; }
        _roundCleared = false; _extraLifeUsed = false;
        if (_lives != null) _lives.ResetLives();
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeTo(1f, 0f, 0.5f));
        StartCoroutine(CombatPhase(planets[_idx]));
    }

    IEnumerator GameLoop()
    {
        yield return StartCoroutine(IntroDialogue());
        for (_idx = 0; _idx < planets.Length; _idx++) yield return StartCoroutine(RunPlanet());
        yield return StartCoroutine(EndingSequence());
    }

    IEnumerator RunPlanet()
    {
        _extraLifeUsed = false;
        if (_lives != null) _lives.ResetLives();
        var p = planets[_idx];
        if (bgRenderer != null) { if (p.spaceBackground) bgRenderer.sprite = p.spaceBackground; bgRenderer.color = Color.white; }
        if (_counterTxt != null) _counterTxt.text = "PLANET " + (_idx + 1) + " / " + planets.Length;
        if (_planetNameTxt != null) _planetNameTxt.gameObject.SetActive(false);
        yield return StartCoroutine(CombatPhase(p));
        yield return StartCoroutine(LandingPhase(p));
    }

    IEnumerator CombatPhase(PlanetEncounter p) 
    { 
        SwitchMusic(combatMusic);
        _roundCleared = false; 
        LaunchFormation(p); 
        yield return new WaitUntil(() => _roundCleared); 
        if (_fgo) { Destroy(_fgo); _fgo = null; } 
    }

    IEnumerator LandingPhase(PlanetEncounter p)
    {
        yield return StartCoroutine(FadeTo(0f, 1f, 0.8f));
        int ti = Mathf.Clamp(_idx, 0, SC.Length - 1);
        if (bgRenderer != null)
        {
            if (p.surfaceBackground != null)
            {
                bgRenderer.sprite = p.surfaceBackground;
                bgRenderer.color = Color.white;
            }
            else bgRenderer.color = SC[ti];
        }
        yield return StartCoroutine(FadeTo(1f, 0f, 0.8f));

        SwitchMusic(dialogueMusic);
        if (_planetNameTxt != null)
        {
            _planetNameTxt.text = p.planetName;
            _planetNameTxt.gameObject.SetActive(true);
        }

        yield return StartCoroutine(ShowDlg("PTAAH: Senzory potvrzují... " + p.rejectionMessage, 0f));
        yield return StartCoroutine(ShowDlg("AŠTAR: Chápu. Nastavte koordináty na další planetu.", 0f));

        CloseDlg();

        if (_planetNameTxt != null) _planetNameTxt.gameObject.SetActive(false);
        yield return StartCoroutine(FadeTo(0f, 1f, 0.8f));
        if (bgRenderer != null)
        {
            if (p.spaceBackground) bgRenderer.sprite = p.spaceBackground;
            bgRenderer.color = Color.white;
        }
        yield return StartCoroutine(FadeTo(1f, 0f, 0.8f));
    }

    IEnumerator IntroDialogue()
    {
        yield return StartCoroutine(FadeTo(0f, 1f, 0.5f));
        if (bgRenderer != null) bgRenderer.color = new Color(0.03f, 0.05f, 0.12f);
        yield return StartCoroutine(FadeTo(1f, 0f, 0.8f));
        SwitchMusic(dialogueMusic);
        yield return StartCoroutine(ShowDlg("PTAAH: Veliteli Aštare. Naše senzory dlouhého dosahu identifikovaly osm kandidátních světů v dosahu. Všechny vykazují známky potenciální obyvatelnosti pro lidský druh.", 0f));
        yield return StartCoroutine(ShowDlg("AŠTAR: Rozumím, Ptaahu. Lidé ze Země potřebují nový domov. Nezklameme je.", 0f));
        yield return StartCoroutine(ShowDlg("JEŠTĚR: Nenajdeš to, co hledáš, Šerane. Tyto světy patří pod naši kontrolu. Tvoje mise zde končí.", 0f));
        yield return StartCoroutine(ShowDlg("AŠTAR: Pak se s vámi střetneme u každého z těch světů a každým z nich se probojujeme!", 0f));
        yield return StartCoroutine(ShowDlg("PTAAH: Nepřátelské formace na obzoru. Zbraně odjištěny. Veliteli — zahajte přiblížení.", 0f));
        CloseDlg();
    }

    IEnumerator EndingSequence()
{
    if (_planetNameTxt != null) _planetNameTxt.gameObject.SetActive(false);
    if (_counterTxt != null) _counterTxt.text = "";
    yield return StartCoroutine(FadeTo(0f, 1f, 0.8f));
    if (bgRenderer != null) bgRenderer.color = Color.black;
    yield return StartCoroutine(FadeTo(1f, 0f, 0.5f));
    SwitchMusic(dialogueMusic);
    yield return StartCoroutine(ShowDlg("PTAAH: Veliteli. Osm světů prozkoumáno. Osm zamítnutí zaznamenáno.", 0f));
    yield return StartCoroutine(ShowDlg("PTAAH: Existuje pouze jedna planeta, která patří tomuto druhu.", 0f));
    yield return StartCoroutine(ShowDlg("AŠTAR: Máte pravdu...", 0f));
    yield return StartCoroutine(ShowDlg("AŠTAR: Nastavte kurz...", 0f));

    // Start cinematic song on the last line — persists into Act2 via DontDestroyOnLoad
    if (cinematicSong != null)
    {
        var carrier = new GameObject("CinematicMusic");
        DontDestroyOnLoad(carrier);
        var src = carrier.AddComponent<AudioSource>();
        src.clip = cinematicSong; src.time = 15f; src.volume = 0.8f; src.loop = false;
        src.Play();
        if (_mus != null) _mus.Stop();
    }

    yield return StartCoroutine(ShowDlg("AŠTAR: ZEMĚ.", 0f));
    CloseDlg();
    yield return StartCoroutine(FadeTo(0f, 1f, 1f));
    SceneManager.LoadScene(3); // was 2 (bug) — now correctly goes to Act2_Transition
}


    IEnumerator ShowDlg(string msg, float hold)
    {
        if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(true);
        if (_speakerIcon != null)
        {
            bool isA = msg.StartsWith("AŠTAR:");
            bool isP = msg.StartsWith("PTAAH:");
            bool isL = msg.StartsWith("JEŠTĚR:");
            bool isJ = msg.StartsWith("JEŽÍŠ:");

            _speakerIcon.sprite = isA ? ashtarIconSprite
                                 : isP ? ptaahIconSprite
                                 : isL ? lizardIconSprite
                                 : isJ ? jesusSprite
                                 : null;
            _speakerIcon.enabled = _speakerIcon.sprite != null;
        }
        if (_dialogueTxt != null) _dialogueTxt.text = msg;
        if (_promptTxt != null) _promptTxt.gameObject.SetActive(true);
        yield return null;
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E));
        if (_promptTxt != null) _promptTxt.gameObject.SetActive(false);
    }

    IEnumerator FadeTo(float from, float to, float dur) { if (_fade == null) { yield return new WaitForSeconds(dur); yield break; } for (float t = 0f; t < dur; t += Time.deltaTime) { _fade.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t / dur)); yield return null; } _fade.color = new Color(0, 0, 0, to); }

    void LaunchFormation(PlanetEncounter p) { if (_fgo) Destroy(_fgo); _fgo = new GameObject("Formation"); var fc = _fgo.AddComponent<FormationController>(); fc.Init(p.cols, p.rows, p.formationSpeed, p.hasBoss ? 1 : 0, p.enemyFireRate, enemy1Prefab, enemy2Prefab, enemy3Prefab, p.hasBoss ? bossPrefab : null, enemyProjectilePrefab, player ? player.transform : null, this); fc.SetTierAndBoss(p.enemyPrefabTier, p.hasBoss); }

    void SwitchMusic(AudioClip clip)
    {
        if (_mus == null || clip == null) return;
        if (_mus.clip == clip && _mus.isPlaying) return;
        _mus.clip = clip;
        _mus.Play();
    }

    void BuildRuntimeUI()
    {
        var cv = FindObjectOfType<Canvas>(); if (cv == null) return;
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Transform root = cv.transform;

        var fo = new GameObject("FadeOverlay"); fo.transform.SetParent(root, false);
        var fr = fo.AddComponent<RectTransform>(); fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero;
        _fade = fo.AddComponent<Image>(); _fade.color = new Color(0, 0, 0, 0); _fade.raycastTarget = false;

        var cg = new GameObject("PlanetCounter"); cg.transform.SetParent(root, false);
        var cr = cg.AddComponent<RectTransform>(); cr.anchorMin = new Vector2(1f, 1f); cr.anchorMax = new Vector2(1f, 1f); cr.pivot = new Vector2(1f, 1f); cr.anchoredPosition = new Vector2(-15f, -15f); cr.sizeDelta = new Vector2(220f, 30f);
        _counterTxt = cg.AddComponent<Text>(); _counterTxt.font = f; _counterTxt.fontSize = 20; _counterTxt.fontStyle = FontStyle.Bold; _counterTxt.alignment = TextAnchor.UpperRight; _counterTxt.color = Color.white; _counterTxt.text = "";

        var ng = new GameObject("PlanetNameText"); ng.transform.SetParent(root, false);
        var nr = ng.AddComponent<RectTransform>(); nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(0f, 1f); nr.pivot = new Vector2(0f, 1f); nr.anchoredPosition = new Vector2(15f, -15f); nr.sizeDelta = new Vector2(400f, 36f);
        _planetNameTxt = ng.AddComponent<Text>(); _planetNameTxt.font = f; _planetNameTxt.fontSize = 24; _planetNameTxt.fontStyle = FontStyle.Bold; _planetNameTxt.color = Color.white; _planetNameTxt.text = "";
        ng.SetActive(false);

        var dp = new GameObject("DialoguePanel"); dp.transform.SetParent(root, false);
        var dr = dp.AddComponent<RectTransform>(); dr.anchorMin = new Vector2(0f, 0f); dr.anchorMax = new Vector2(1f, 0f); dr.pivot = new Vector2(0.5f, 0f); dr.anchoredPosition = Vector2.zero; dr.sizeDelta = new Vector2(0f, 200f);
        _dialogueBg = dp.AddComponent<Image>(); _dialogueBg.color = new Color(0f, 0f, 0f, 0.85f); dp.SetActive(false);

        var ig = new GameObject("SpeakerIcon"); ig.transform.SetParent(dp.transform, false);
        var ir = ig.AddComponent<RectTransform>(); ir.anchorMin = new Vector2(0f, 0f); ir.anchorMax = new Vector2(0f, 1f); ir.pivot = new Vector2(0f, 0.5f); ir.offsetMin = new Vector2(10f, 5f); ir.offsetMax = new Vector2(210f, -5f);
        _speakerIcon = ig.AddComponent<Image>(); _speakerIcon.preserveAspect = true; _speakerIcon.enabled = false;

        var dtg = new GameObject("DialogueText"); dtg.transform.SetParent(dp.transform, false);
        var dtr = dtg.AddComponent<RectTransform>(); dtr.anchorMin = Vector2.zero; dtr.anchorMax = Vector2.one; dtr.offsetMin = new Vector2(225f, 8f); dtr.offsetMax = new Vector2(-20f, -8f);
        _dialogueTxt = dtg.AddComponent<Text>(); _dialogueTxt.font = f; _dialogueTxt.fontSize = 44; _dialogueTxt.fontStyle = FontStyle.Italic; _dialogueTxt.alignment = TextAnchor.MiddleLeft; _dialogueTxt.color = new Color(0.95f, 0.95f, 0.8f, 1f); _dialogueTxt.text = "";

        var ptg = new GameObject("PromptText"); ptg.transform.SetParent(dp.transform, false);
        var ptr = ptg.AddComponent<RectTransform>(); ptr.anchorMin = new Vector2(1f, 0f); ptr.anchorMax = new Vector2(1f, 0f); ptr.pivot = new Vector2(1f, 0f); ptr.anchoredPosition = new Vector2(-14f, 8f); ptr.sizeDelta = new Vector2(300f, 24f);
        _promptTxt = ptg.AddComponent<Text>(); _promptTxt.font = f; _promptTxt.fontSize = 16; _promptTxt.fontStyle = FontStyle.Bold; _promptTxt.alignment = TextAnchor.LowerRight; _promptTxt.color = Color.yellow; _promptTxt.text = "\u25ba  click / space / E";
        ptg.SetActive(false);

        var ag = new GameObject("AnnounceText"); ag.transform.SetParent(root, false);
        var ar = ag.AddComponent<RectTransform>(); ar.anchorMin = new Vector2(0f, 0.35f); ar.anchorMax = new Vector2(1f, 0.65f); ar.offsetMin = Vector2.zero; ar.offsetMax = Vector2.zero;
        _announceTxt = ag.AddComponent<Text>(); _announceTxt.font = f; _announceTxt.fontSize = 36; _announceTxt.fontStyle = FontStyle.Bold; _announceTxt.alignment = TextAnchor.MiddleCenter; _announceTxt.color = Color.clear; _announceTxt.text = "";

        var jg = new GameObject("JesusOverlay"); jg.transform.SetParent(root, false);
        var jr = jg.AddComponent<RectTransform>(); jr.anchorMin = Vector2.zero; jr.anchorMax = Vector2.one; jr.offsetMin = Vector2.zero; jr.offsetMax = Vector2.zero;
        _jesusImg = jg.AddComponent<Image>(); _jesusImg.preserveAspect = false; _jesusImg.color = Color.clear; _jesusImg.raycastTarget = false;
        jg.SetActive(false);

        var rgo = new GameObject("RetryButton"); rgo.transform.SetParent(root, false);
        var rrt = rgo.AddComponent<RectTransform>(); rrt.anchorMin = new Vector2(0.25f, 0.1f); rrt.anchorMax = new Vector2(0.48f, 0.19f); rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
        var rimg = rgo.AddComponent<Image>(); rimg.color = new Color(0.10f, 0.45f, 0.10f, 0.92f);
        _retryBtn = rgo.AddComponent<Button>();
        var rcb = ColorBlock.defaultColorBlock; rcb.normalColor = new Color(0.10f, 0.45f, 0.10f, 0.92f); rcb.highlightedColor = new Color(0.15f, 0.60f, 0.15f, 1f); rcb.pressedColor = Color.white;
        _retryBtn.colors = rcb; _retryBtn.targetGraphic = rimg;
        _retryBtn.onClick.AddListener(() => {
            rgo.SetActive(false);
            if (_menuBtn != null) _menuBtn.gameObject.SetActive(false);
            if (_jesusImg != null) { _jesusImg.color = Color.clear; _jesusImg.gameObject.SetActive(false); }
            Time.timeScale = 1f;
            if (player != null) { player.gameObject.SetActive(true); var ren = player.GetComponent<SpriteRenderer>(); if (ren != null) ren.enabled = true; }
            _extraLifeUsed = false;
            if (_lives != null) _lives.ResetLives();
            StartCoroutine(RetryPlanet());
        });
        var rlg = new GameObject("Lbl"); rlg.transform.SetParent(rgo.transform, false);
        var rlr = rlg.AddComponent<RectTransform>(); rlr.anchorMin = Vector2.zero; rlr.anchorMax = Vector2.one; rlr.offsetMin = Vector2.zero; rlr.offsetMax = Vector2.zero;
        var rlt = rlg.AddComponent<Text>(); rlt.font = f; rlt.fontSize = 30; rlt.fontStyle = FontStyle.Bold; rlt.alignment = TextAnchor.MiddleCenter; rlt.color = Color.white; rlt.text = "RETRY";
        rgo.SetActive(false);

        var mgo = new GameObject("MenuButton"); mgo.transform.SetParent(root, false);
        var mrt = mgo.AddComponent<RectTransform>(); mrt.anchorMin = new Vector2(0.52f, 0.1f); mrt.anchorMax = new Vector2(0.75f, 0.19f); mrt.offsetMin = Vector2.zero; mrt.offsetMax = Vector2.zero;
        var mimg = mgo.AddComponent<Image>(); mimg.color = new Color(0.45f, 0.10f, 0.10f, 0.92f);
        _menuBtn = mgo.AddComponent<Button>();
        var mcb = ColorBlock.defaultColorBlock; mcb.normalColor = new Color(0.45f, 0.10f, 0.10f, 0.92f); mcb.highlightedColor = new Color(0.60f, 0.15f, 0.15f, 1f); mcb.pressedColor = Color.white;
        _menuBtn.colors = mcb; _menuBtn.targetGraphic = mimg;
        _menuBtn.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(0); });
        var mlg = new GameObject("Lbl"); mlg.transform.SetParent(mgo.transform, false);
        var mlr = mlg.AddComponent<RectTransform>(); mlr.anchorMin = Vector2.zero; mlr.anchorMax = Vector2.one; mlr.offsetMin = Vector2.zero; mlr.offsetMax = Vector2.zero;
        var mlt = mlg.AddComponent<Text>(); mlt.font = f; mlt.fontSize = 30; mlt.fontStyle = FontStyle.Bold; mlt.alignment = TextAnchor.MiddleCenter; mlt.color = Color.white; mlt.text = "MAIN MENU";
        mgo.SetActive(false);

        fo.transform.SetAsLastSibling();
    }
}