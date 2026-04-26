using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Act2Controller : MonoBehaviour
{
    public static AudioSource CinematicMusic;

    public AudioClip cinematicSong;
    public Sprite    jesusSprite;
    public Sprite    cutsceneBg;

    Text        _label;
    AudioSource _src;
    Image       _dlgBg, _speakerIcon, _bgImg;
    Text        _dlgTxt, _promptTxt;

    void Start()
    {
        var cvGo = new GameObject("Canvas");
        var cv   = cvGo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cvGo.AddComponent<CanvasScaler>();
        cvGo.AddComponent<GraphicRaycaster>();

        // Setup Background
        var bgGo = new GameObject("BG");
        bgGo.transform.SetParent(cvGo.transform, false);
        var bgR  = bgGo.AddComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero; 
        bgR.anchorMax = Vector2.one;
        bgR.offsetMin = Vector2.zero; 
        bgR.offsetMax = Vector2.zero;

        _bgImg = bgGo.AddComponent<Image>();
        _bgImg.color = cutsceneBg != null ? Color.white : Color.black;
        _bgImg.sprite = cutsceneBg;
        _bgImg.type = Image.Type.Simple;
        _bgImg.preserveAspect = false;
        
        // Start with background hidden for the "drops"
        _bgImg.gameObject.SetActive(false);

        // Setup Label
        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(cvGo.transform, false);
        var lblR  = lblGo.AddComponent<RectTransform>();
        lblR.anchorMin = Vector2.zero; 
        lblR.anchorMax = Vector2.one;
        lblR.offsetMin = Vector2.zero; 
        lblR.offsetMax = Vector2.zero;
        
        _label = lblGo.AddComponent<Text>();
        _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize = 52;
        _label.fontStyle = FontStyle.Italic;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.color = new Color(0.95f, 0.95f, 0.8f, 1f);
        _label.text = "";

        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Setup Dialogue Panel
        var dp = new GameObject("DlgPanel");
        dp.transform.SetParent(cvGo.transform, false);
        var dr = dp.AddComponent<RectTransform>();
        dr.anchorMin = new Vector2(0f, 0f); 
        dr.anchorMax = new Vector2(1f, 0f);
        dr.pivot = new Vector2(0.5f, 0f);
        dr.anchoredPosition = Vector2.zero; 
        dr.sizeDelta = new Vector2(0f, 200f);
        
        _dlgBg = dp.AddComponent<Image>();
        _dlgBg.color = new Color(0f, 0f, 0f, 0.88f);
        dp.SetActive(false);

        // Setup Speaker Icon
        var ig = new GameObject("Icon");
        ig.transform.SetParent(dp.transform, false);
        var ir = ig.AddComponent<RectTransform>();
        ir.anchorMin = new Vector2(0f, 0f); 
        ir.anchorMax = new Vector2(0f, 1f);
        ir.pivot = new Vector2(0f, 0.5f);
        ir.offsetMin = new Vector2(10f, 5f); 
        ir.offsetMax = new Vector2(210f, -5f);
        
        _speakerIcon = ig.AddComponent<Image>();
        _speakerIcon.preserveAspect = true;
        _speakerIcon.enabled = false;

        // Setup Dialogue Text
        var tg = new GameObject("DlgText");
        tg.transform.SetParent(dp.transform, false);
        var tr = tg.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; 
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(225f, 8f); 
        tr.offsetMax = new Vector2(-20f, -8f);
        
        _dlgTxt = tg.AddComponent<Text>();
        _dlgTxt.font = f; 
        _dlgTxt.fontSize = 40; 
        _dlgTxt.fontStyle = FontStyle.Italic;
        _dlgTxt.alignment = TextAnchor.MiddleLeft;
        _dlgTxt.color = new Color(0.95f, 0.95f, 0.8f, 1f);

        // Setup Prompt Text
        var pg = new GameObject("Prompt");
        pg.transform.SetParent(dp.transform, false);
        var pr = pg.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(1f, 0f); 
        pr.anchorMax = new Vector2(1f, 0f);
        pr.pivot = new Vector2(1f, 0f);
        pr.anchoredPosition = new Vector2(-14f, 8f); 
        pr.sizeDelta = new Vector2(300f, 24f);
        
        _promptTxt = pg.AddComponent<Text>();
        _promptTxt.font = f; 
        _promptTxt.fontSize = 16; 
        _promptTxt.fontStyle = FontStyle.Bold;
        _promptTxt.alignment = TextAnchor.LowerRight;
        _promptTxt.color = Color.yellow;
        _promptTxt.text = "\u25ba  E";
        pg.SetActive(false);

        // Audio Setup
        if (cinematicSong != null)
        {
            _src = gameObject.AddComponent<AudioSource>();
            _src.clip = cinematicSong;
            _src.time = 35f;
            _src.volume = 0.8f;
            _src.loop = false;
            _src.Play();
        }

        StartCoroutine(ShowFrames());
    }

    IEnumerator ShowFrames()
    {
        // PHASE 1: The Drops (Black Screen)
        yield return new WaitForSeconds(1f);
        _label.text = "AŠTAR: Nastavte kurz...";

        yield return new WaitForSeconds(3f);
        _label.text = "AŠTAR: ZEMĚ!";

        yield return new WaitForSeconds(4f);
        _label.text = "";

        // PHASE 2: Reveal Background
        if (_bgImg != null) _bgImg.gameObject.SetActive(true);

        // PHASE 3: Jesus Dialogue
        yield return StartCoroutine(ShowDlg("JEŽÍŠ: A tak se vydal náš chrabrý hrdina Aštar Šeran vydobýt planetu Zemi zpátky pro lidstvo."));
yield return StartCoroutine(ShowDlg("JEŽÍŠ: Čeká ho strašlivá bitva... bude muset zdolat silné nepřátele."));
yield return StartCoroutine(ShowDlg("JEŽÍŠ: ...uvidíme, zdali se mu to podaří."));

        CloseDlg();

        SceneManager.LoadScene(4);
    }

    IEnumerator ShowDlg(string msg)
    {
        if (_dlgBg != null) _dlgBg.gameObject.SetActive(true);
        if (_speakerIcon != null)
        {
            _speakerIcon.sprite = jesusSprite;
            _speakerIcon.enabled = jesusSprite != null;
        }
        if (_dlgTxt != null) _dlgTxt.text = msg;
        if (_promptTxt != null) _promptTxt.gameObject.SetActive(true);
        
        yield return null;
        
        bool done = false;
        while (!done)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
                done = true;
            yield return null;
        }
        
        if (_promptTxt != null) _promptTxt.gameObject.SetActive(false);
    }

    void CloseDlg()
    {
        if (_dlgBg != null) _dlgBg.gameObject.SetActive(false);
        if (_speakerIcon != null) _speakerIcon.enabled = false;
        if (_dlgTxt != null) _dlgTxt.text = "";
        if (_promptTxt != null) _promptTxt.gameObject.SetActive(false);
    }
}