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
    public Sprite ashtarIconSprite, ptaahIconSprite, jesusSprite;

    [Header("Backgrounds")]
    public Sprite[] spaceBackgrounds  = new Sprite[8];  // one per planet — combat phase
    public Sprite[] planetBackgrounds = new Sprite[8];  // one per planet — landing phase

    Image  _fade, _dialogueBg, _speakerIcon, _jesusImg;
    Text   _counterTxt, _announceTxt, _dialogueTxt, _planetNameTxt, _promptTxt, _jesusPromptTxt;
    LivesManager _lives;
    GameObject _fgo;
    int  _idx;
    bool _roundCleared, _extraLifeUsed;

    // Game Over UI Fields
    GameObject _gameOverPanel;
    Image      _gameOverBgImg;
    Text       _gameOverTitleTxt, _gameOverSubTxt;
    Button     _retryBtn, _menuBtn;

    static readonly Color[] SC = {
        Color.red, Color.blue, Color.cyan, Color.red,
        Color.yellow, Color.blue, Color.gray, Color.green
    };

    void Awake() { BuildRuntimeUI(); }

    void Start()
    {
        _lives = FindObjectOfType<LivesManager>();
        if (_lives != null) _lives.onAllLivesLost = OnLivesGone;
        if (bgRenderer == null) { var bg = GameObject.Find("Background"); if (bg) bgRenderer = bg.GetComponent<SpriteRenderer>(); }
        if (planets == null || planets.Length != 8)
        {
            planets = new PlanetEncounter[]
            {
                new PlanetEncounter{planetName="Kepler-186f",  rejectionMessage="Oxygen levels incompatible with human biology.",                 rows=2,cols=4,formationSpeed=1.2f,enemyFireRate=3.5f,enemyPrefabTier=1,hasBoss=false},
                new PlanetEncounter{planetName="Proxima b",    rejectionMessage="Surface radiation would prove fatal within hours.",              rows=2,cols=5,formationSpeed=1.4f,enemyFireRate=3.2f,enemyPrefabTier=1,hasBoss=false},
                new PlanetEncounter{planetName="Tau Ceti e",   rejectionMessage="Atmospheric pressure far exceeds human tolerance.",              rows=3,cols=4,formationSpeed=1.6f,enemyFireRate=3.0f,enemyPrefabTier=1,hasBoss=false},
                new PlanetEncounter{planetName="Gliese 667Cc", rejectionMessage="Already colonized. We would not impose upon its people.",       rows=3,cols=5,formationSpeed=1.8f,enemyFireRate=2.8f,enemyPrefabTier=2,hasBoss=false},
                new PlanetEncounter{planetName="Wolf 1061c",   rejectionMessage="Tidal locking makes half the surface uninhabitable.",           rows=3,cols=5,formationSpeed=2.0f,enemyFireRate=2.5f,enemyPrefabTier=2,hasBoss=false},
                new PlanetEncounter{planetName="Trappist-1e",  rejectionMessage="Magnetic field too weak. Cosmic radiation is constant.",        rows=4,cols=5,formationSpeed=2.2f,enemyFireRate=2.2f,enemyPrefabTier=2,hasBoss=true},
                new PlanetEncounter{planetName="Luyten b",     rejectionMessage="Gravity is 1.8x Earth normal. Humans would be crushed.",       rows=4,cols=6,formationSpeed=2.4f,enemyFireRate=2.0f,enemyPrefabTier=3,hasBoss=true},
                new PlanetEncounter{planetName="Ross 128b",    rejectionMessage="No liquid water. No soil. No memory. Not our home.",            rows=4,cols=6,formationSpeed=2.8f,enemyFireRate=1.8f,enemyPrefabTier=3,hasBoss=true},
            };
        }
        // Stamp sprites into the struct data at runtime
        for (int i = 0; i < planets.Length; i++)
        {
            var tmp = planets[i];
            if (spaceBackgrounds  != null && i < spaceBackgrounds.Length  && spaceBackgrounds[i]  != null) tmp.spaceBackground   = spaceBackgrounds[i];
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

    IEnumerator ShowGameOver()
    {
        Time.timeScale = 0f;
        if (player != null) player.gameObject.SetActive(false);
        if (_gameOverPanel != null)
        {
            if (_gameOverBgImg != null && jesusSprite != null)
            {
                _gameOverBgImg.sprite = jesusSprite;
                _gameOverBgImg.color = new Color(1f, 1f, 1f, 0.75f); // Increased solidity
            }

            if (_gameOverSubTxt != null)
                _gameOverSubTxt.text = "Mé dítě, vstaň a zkus to znovu. Dávám ti život — žij a poraz ty ještěry!";

            _gameOverPanel.SetActive(true);
            _gameOverPanel.transform.SetAsLastSibling();

            if (_retryBtn != null)
            {
                _retryBtn.onClick.RemoveAllListeners();
                _retryBtn.onClick.AddListener(() => {
                    _gameOverPanel.SetActive(false);
                    Time.timeScale = 1f;
                    if (player != null) player.gameObject.SetActive(true);
                    _extraLifeUsed = false;
                    StartCoroutine(RetryPlanet());
                });
            }

            if (_menuBtn != null)
            {
                _menuBtn.onClick.RemoveAllListeners();
                _menuBtn.onClick.AddListener(() => { Time.timeScale = 1f; SceneManager.LoadScene(0); });
            }
        }
        yield return null;
    }

    IEnumerator ExtraMsg()
    {
        if (_announceTxt!=null){_announceTxt.text="ASHTAR SHERAN \u2014 ONE FINAL CHANCE";_announceTxt.color=new Color(1f,0.85f,0.1f,1f);_announceTxt.fontSize=36;}
        yield return new WaitForSeconds(2.5f);
        if (_announceTxt!=null){_announceTxt.text="";_announceTxt.color=Color.clear;}
    }

    IEnumerator RetryPlanet()
    {
        yield return StartCoroutine(FadeTo(0f,1f,0.5f));
        if (_fgo){Destroy(_fgo);_fgo=null;}
        _roundCleared=false; _extraLifeUsed=false;
        if (_lives!=null) _lives.ResetLives();
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeTo(1f,0f,0.5f));
        StartCoroutine(CombatPhase(planets[_idx]));
    }

    IEnumerator GameLoop()
    {
        for (_idx=0;_idx<planets.Length;_idx++) yield return StartCoroutine(RunPlanet());
        yield return StartCoroutine(EndingSequence());
    }

    IEnumerator RunPlanet()
    {
        _extraLifeUsed=false;
        if (_lives!=null) _lives.ResetLives();
        var p=planets[_idx];
        if (bgRenderer!=null){if (p.spaceBackground) bgRenderer.sprite=p.spaceBackground; bgRenderer.color=Color.white;}
        if (_counterTxt!=null) _counterTxt.text="PLANET "+(_idx+1)+" / "+planets.Length;
        if (_planetNameTxt!=null) _planetNameTxt.gameObject.SetActive(false);
        yield return StartCoroutine(CombatPhase(p));
        yield return StartCoroutine(LandingPhase(p));
    }

    IEnumerator JesusReveal()
    {
        Time.timeScale = 0f;
        if (_jesusImg != null)
        {
            if (jesusSprite != null) _jesusImg.sprite = jesusSprite;
            _jesusImg.preserveAspect = false;
            var jrt = _jesusImg.rectTransform;
            jrt.anchorMin = Vector2.zero; jrt.anchorMax = Vector2.one;
            jrt.offsetMin = Vector2.zero; jrt.offsetMax = Vector2.zero;
            _jesusImg.transform.SetAsLastSibling();
            _jesusImg.gameObject.SetActive(true);
            _jesusImg.color = new Color(1f, 1f, 1f, 0f);
            const float targetAlpha = 0.75f; // Increased solidity
            for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
            {
                _jesusImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, targetAlpha, t / 0.6f));
                yield return null;
            }
            _jesusImg.color = new Color(1f, 1f, 1f, targetAlpha);
        }
        if (_announceTxt != null)
        {
            _announceTxt.text = "ASHTAR SHERAN \u2014 ONE FINAL CHANCE";
            _announceTxt.color = new Color(1f, 0.85f, 0.1f, 1f);
            _announceTxt.fontSize = 34;
            _announceTxt.transform.SetAsLastSibling();
        }
        if (_jesusPromptTxt != null)
        {
            _jesusPromptTxt.transform.SetAsLastSibling();
            _jesusPromptTxt.gameObject.SetActive(true);
        }
        yield return null;
        yield return new WaitUntil(() =>
            Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.E));
        Time.timeScale = 1f;
        if (_jesusPromptTxt != null) _jesusPromptTxt.gameObject.SetActive(false);
        if (_announceTxt != null) { _announceTxt.text = ""; _announceTxt.color = Color.clear; }
        if (_jesusImg != null)
        {
            float startA = _jesusImg.color.a;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                _jesusImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(startA, 0f, t / 0.5f));
                yield return null;
            }
            _jesusImg.color = new Color(1f, 1f, 1f, 0f);
            _jesusImg.gameObject.SetActive(false);
        }
        _extraLifeUsed = true;
        if (_lives != null) { _lives.lives = 1; _lives.RefreshHearts(); }
    }

    IEnumerator CombatPhase(PlanetEncounter p) { _roundCleared=false; LaunchFormation(p); yield return new WaitUntil(()=>_roundCleared); if(_fgo){Destroy(_fgo);_fgo=null;} }

    IEnumerator LandingPhase(PlanetEncounter p)
    {
        yield return StartCoroutine(FadeTo(0f,1f,0.8f));
        int ti=Mathf.Clamp(_idx,0,SC.Length-1);
        if (bgRenderer!=null){if(p.surfaceBackground!=null){bgRenderer.sprite=p.surfaceBackground;bgRenderer.color=Color.white;}else bgRenderer.color=SC[ti];}
        yield return StartCoroutine(FadeTo(1f,0f,0.8f));
        if (_planetNameTxt!=null){_planetNameTxt.text=p.planetName;_planetNameTxt.gameObject.SetActive(true);}
        yield return StartCoroutine(ShowDlg("PTAAH: Sensors confirm \u2014 "+p.rejectionMessage,3.5f));
        yield return StartCoroutine(ShowDlg("ASHTAR: Understood. Set course for next coordinates.",2.5f));
        if (_dialogueBg!=null)_dialogueBg.gameObject.SetActive(false);
        if (_speakerIcon!=null)_speakerIcon.enabled=false;
        if (_dialogueTxt!=null)_dialogueTxt.text="";
        if (_planetNameTxt!=null)_planetNameTxt.gameObject.SetActive(false);
        yield return StartCoroutine(FadeTo(0f,1f,0.8f));
        if (bgRenderer!=null){if(p.spaceBackground)bgRenderer.sprite=p.spaceBackground;bgRenderer.color=Color.white;}
        yield return StartCoroutine(FadeTo(1f,0f,0.8f));
    }

    IEnumerator EndingSequence()
    {
        if (_planetNameTxt!=null)_planetNameTxt.gameObject.SetActive(false);
        if (_counterTxt!=null)_counterTxt.text="";
        yield return StartCoroutine(FadeTo(0f,1f,0.8f));
        if (bgRenderer!=null)bgRenderer.color=Color.black;
        yield return StartCoroutine(FadeTo(1f,0f,0.5f));
        yield return StartCoroutine(Panel("Eight worlds. Eight failures.",3f,36));
        yield return StartCoroutine(Panel("The data does not lie, Commander.",3f,36));
        yield return StartCoroutine(Panel("There is only one planet whose air, water,\ngravity and memory belong to this species.",4f,28));
        yield return StartCoroutine(Panel("",2f,36));
        yield return StartCoroutine(Panel("ASHTAR: Set course for Earth.",2f,40));
        yield return StartCoroutine(FadeTo(0f,1f,1f));
        SceneManager.LoadScene(2);
    }

    IEnumerator Panel(string txt,float dur,int sz) { if(_announceTxt!=null){_announceTxt.text=txt;_announceTxt.color=Color.white;_announceTxt.fontSize=sz;} yield return new WaitForSeconds(dur); if(_announceTxt!=null){_announceTxt.text="";_announceTxt.color=Color.clear;} }

    IEnumerator ShowDlg(string msg,float hold)
    {
        if (_dialogueBg!=null)_dialogueBg.gameObject.SetActive(true);
        if (_speakerIcon!=null){bool a=msg.StartsWith("ASHTAR:");bool b=msg.StartsWith("PTAAH:");_speakerIcon.sprite=a?ashtarIconSprite:(b?ptaahIconSprite:null);_speakerIcon.enabled=_speakerIcon.sprite!=null;}
        if (_dialogueTxt!=null)_dialogueTxt.text=msg;
        if (_promptTxt!=null)_promptTxt.gameObject.SetActive(true);
        yield return null;
        yield return new WaitUntil(()=>Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.E));
        if (_promptTxt!=null)_promptTxt.gameObject.SetActive(false);
    }

    IEnumerator FadeTo(float from,float to,float dur) { if(_fade==null){yield return new WaitForSeconds(dur);yield break;} for(float t=0f;t<dur;t+=Time.deltaTime){_fade.color=new Color(0,0,0,Mathf.Lerp(from,to,t/dur));yield return null;} _fade.color=new Color(0,0,0,to); }

    void LaunchFormation(PlanetEncounter p) { if(_fgo)Destroy(_fgo); _fgo=new GameObject("Formation"); var fc=_fgo.AddComponent<FormationController>(); fc.Init(p.cols,p.rows,p.formationSpeed,p.hasBoss?1:0,p.enemyFireRate,enemy1Prefab,enemy2Prefab,enemy3Prefab,p.hasBoss?bossPrefab:null,enemyProjectilePrefab,player?player.transform:null,this); fc.SetTierAndBoss(p.enemyPrefabTier,p.hasBoss); }

    void BuildRuntimeUI()
    {
        var cv=FindObjectOfType<Canvas>(); if(cv==null)return;
        Font f=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var fo=new GameObject("FadeOverlay");fo.transform.SetParent(cv.transform,false);
        var fr=fo.AddComponent<RectTransform>();fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=Vector2.zero;fr.offsetMax=Vector2.zero;
        _fade=fo.AddComponent<Image>();_fade.color=new Color(0,0,0,0);_fade.raycastTarget=false;
        var cg=new GameObject("PlanetCounter");cg.transform.SetParent(cv.transform,false);
        var cr=cg.AddComponent<RectTransform>();cr.anchorMin=new Vector2(1f,1f);cr.anchorMax=new Vector2(1f,1f);cr.pivot=new Vector2(1f,1f);cr.anchoredPosition=new Vector2(-15f,-15f);cr.sizeDelta=new Vector2(220f,30f);
        _counterTxt=cg.AddComponent<Text>();_counterTxt.font=f;_counterTxt.fontSize=20;_counterTxt.fontStyle=FontStyle.Bold;_counterTxt.alignment=TextAnchor.UpperRight;_counterTxt.color=Color.white;_counterTxt.text="";
        var ng=new GameObject("PlanetNameText");ng.transform.SetParent(cv.transform,false);
        var nr=ng.AddComponent<RectTransform>();nr.anchorMin=new Vector2(0f,1f);nr.anchorMax=new Vector2(0f,1f);nr.pivot=new Vector2(0f,1f);nr.anchoredPosition=new Vector2(15f,-15f);nr.sizeDelta=new Vector2(400f,36f);
        _planetNameTxt=ng.AddComponent<Text>();_planetNameTxt.font=f;_planetNameTxt.fontSize=24;_planetNameTxt.fontStyle=FontStyle.Bold;_planetNameTxt.color=Color.white;_planetNameTxt.text="";
        ng.SetActive(false);
        var dp=new GameObject("DialoguePanel");dp.transform.SetParent(cv.transform,false);
        var dr=dp.AddComponent<RectTransform>();dr.anchorMin=new Vector2(0f,0f);dr.anchorMax=new Vector2(1f,0f);dr.pivot=new Vector2(0.5f,0f);dr.anchoredPosition=Vector2.zero;dr.sizeDelta=new Vector2(0f,90f);
        _dialogueBg=dp.AddComponent<Image>();_dialogueBg.color=new Color(0f,0f,0f,0.85f);dp.SetActive(false);
        var ig=new GameObject("SpeakerIcon");ig.transform.SetParent(dp.transform,false);
        var ir=ig.AddComponent<RectTransform>();ir.anchorMin=new Vector2(0f,0f);ir.anchorMax=new Vector2(0f,1f);ir.pivot=new Vector2(0f,0.5f);ir.offsetMin=new Vector2(10f,5f);ir.offsetMax=new Vector2(90f,-5f);
        _speakerIcon=ig.AddComponent<Image>();_speakerIcon.preserveAspect=true;_speakerIcon.enabled=false;
        var dtg=new GameObject("DialogueText");dtg.transform.SetParent(dp.transform,false);
        var dtr=dtg.AddComponent<RectTransform>();dtr.anchorMin=Vector2.zero;dtr.anchorMax=Vector2.one;dtr.offsetMin=new Vector2(105f,8f);dtr.offsetMax=new Vector2(-20f,-8f);
        _dialogueTxt=dtg.AddComponent<Text>();_dialogueTxt.font=f;_dialogueTxt.fontSize=22;_dialogueTxt.fontStyle=FontStyle.Italic;_dialogueTxt.alignment=TextAnchor.MiddleLeft;_dialogueTxt.color=new Color(0.95f,0.95f,0.8f,1f);_dialogueTxt.text="";
        var ptg=new GameObject("PromptText");ptg.transform.SetParent(dp.transform,false);
        var ptr=ptg.AddComponent<RectTransform>();ptr.anchorMin=new Vector2(1f,0f);ptr.anchorMax=new Vector2(1f,0f);ptr.pivot=new Vector2(1f,0f);ptr.anchoredPosition=new Vector2(-14f,8f);ptr.sizeDelta=new Vector2(300f,24f);
        _promptTxt=ptg.AddComponent<Text>();_promptTxt.font=f;_promptTxt.fontSize=16;_promptTxt.fontStyle=FontStyle.Bold;_promptTxt.alignment=TextAnchor.LowerRight;_promptTxt.color=Color.yellow;_promptTxt.text="\u25ba  click / space / E";
        ptg.SetActive(false);
        var ag=new GameObject("AnnounceText");ag.transform.SetParent(cv.transform,false);
        var ar=ag.AddComponent<RectTransform>();ar.anchorMin=new Vector2(0f,0.35f);ar.anchorMax=new Vector2(1f,0.65f);ar.offsetMin=Vector2.zero;ar.offsetMax=Vector2.zero;
        _announceTxt=ag.AddComponent<Text>();_announceTxt.font=f;_announceTxt.fontSize=36;_announceTxt.fontStyle=FontStyle.Bold;_announceTxt.alignment=TextAnchor.MiddleCenter;_announceTxt.color=Color.clear;_announceTxt.text="";
        var jg=new GameObject("JesusOverlay");jg.transform.SetParent(cv.transform,false);
        var jr=jg.AddComponent<RectTransform>();jr.anchorMin=Vector2.zero;jr.anchorMax=Vector2.one;jr.offsetMin=Vector2.zero;jr.offsetMax=Vector2.zero;
        _jesusImg=jg.AddComponent<Image>();_jesusImg.preserveAspect=false;_jesusImg.color=Color.clear;_jesusImg.raycastTarget=false;
        jg.SetActive(false);
        var jpg=new GameObject("JesusPrompt");jpg.transform.SetParent(cv.transform,false);
        var jpr=jpg.AddComponent<RectTransform>();jpr.anchorMin=new Vector2(0f,0f);jpr.anchorMax=new Vector2(1f,0f);jpr.pivot=new Vector2(0.5f,0f);jpr.anchoredPosition=new Vector2(0f,20f);jpr.sizeDelta=new Vector2(0f,30f);
        _jesusPromptTxt=jpg.AddComponent<Text>();_jesusPromptTxt.font=f;_jesusPromptTxt.fontSize=20;_jesusPromptTxt.fontStyle=FontStyle.Bold;_jesusPromptTxt.alignment=TextAnchor.MiddleCenter;_jesusPromptTxt.color=Color.yellow;_jesusPromptTxt.text="\u25ba  click / space / E to continue";
        jpg.SetActive(false);

        BuildGameOverPanel(cv.transform, f);
        fo.transform.SetAsLastSibling();
    }

    void BuildGameOverPanel(Transform canvasRoot, Font f)
    {
        var gop = new GameObject("GameOverPanel");
        gop.transform.SetParent(canvasRoot, false);
        _gameOverPanel = gop;
        var gopr = gop.AddComponent<RectTransform>();
        gopr.anchorMin = Vector2.zero; gopr.anchorMax = Vector2.one;
        gopr.offsetMin = Vector2.zero; gopr.offsetMax = Vector2.zero;
        
        _gameOverBgImg = gop.AddComponent<Image>();
        _gameOverBgImg.preserveAspect = false;
        _gameOverBgImg.color = Color.clear;
        
        // Title: Top Center
        _gameOverTitleTxt = MakeText(gop.transform, "GOTitle",
            new Vector2(0f,0.80f), new Vector2(1f,0.95f), f, 72,
            FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f,0.18f,0.18f,1f), "GAME OVER");

        // SubText (Custom Text): Set to Black and pushed to absolute bottom area
        _gameOverSubTxt = MakeText(gop.transform, "GOSub",
            new Vector2(0.05f,0.10f), new Vector2(0.95f,0.20f), f, 22,
            FontStyle.Italic, TextAnchor.MiddleCenter, Color.black, "");

        // Buttons: Absolute Bottom, solid opacity (0.85f)
        _retryBtn = MakeButton(gop.transform, "RETRY",
            new Vector2(0.42f,0.02f), new Vector2(0.49f,0.08f),
            new Color(0.10f,0.45f,0.10f,0.85f), f);

        _menuBtn = MakeButton(gop.transform, "MAIN MENU",
            new Vector2(0.51f,0.02f), new Vector2(0.58f,0.08f),
            new Color(0.40f,0.10f,0.10f,0.85f), f);

        gop.SetActive(false);
        gop.transform.SetAsLastSibling();
    }

    static Text MakeText(Transform parent, string name, Vector2 aMin, Vector2 aMax,
        Font font, int size, FontStyle style, TextAnchor align, Color col, string text)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>();
        t.font=font; t.fontSize=size; t.fontStyle=style; t.alignment=align; t.color=col; t.text=text;
        return t;
    }

    static Button MakeButton(Transform parent, string label, Vector2 aMin, Vector2 aMax, Color bgCol, Font font)
    {
        var go = new GameObject(label+"Btn"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin=aMin; rt.anchorMax=aMax; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = bgCol;
        var btn = go.AddComponent<Button>();
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor = bgCol;
        cb.highlightedColor = new Color(bgCol.r+.1f, bgCol.g+.1f, bgCol.b+.1f, 0.95f);
        cb.pressedColor     = Color.white;
        btn.colors=cb; btn.targetGraphic=img;
        MakeText(go.transform,"Lbl",Vector2.zero,Vector2.one,font,18,FontStyle.Bold,TextAnchor.MiddleCenter,Color.white,label);
        return btn;
    }
}