using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip backgroundMusic;

    [Header("Handoff")]
    public GameObject slideshowManager;
    public string     nextScene = "Act1_Space";

    Canvas        _cv;
    Image         _fadeImg;
    RectTransform _titleRT;
    bool          _starting;

    const float PULSE_MIN   = 0.92f;
    const float PULSE_MAX   = 1.08f;
    const float PULSE_SPEED = 1.5f;

    void Awake()
    {
        EnsureEventSystem();
        BuildTitleUI();
        StartAudio();
        if (slideshowManager != null) slideshowManager.SetActive(false);
    }

    void Update()
    {
        if (_titleRT != null)
        {
            float s = Mathf.Lerp(PULSE_MIN, PULSE_MAX,
                (Mathf.Sin(Time.time * PULSE_SPEED * Mathf.PI * 2f) + 1f) * 0.5f);
            _titleRT.localScale = new Vector3(s, s, 1f);
        }
        if (!_starting && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            OnStartClicked();
    }

    public void OnStartClicked()
    {
        if (_starting) return;
        _starting = true;
        StartCoroutine(FadeAndHandOff());
    }

    IEnumerator FadeAndHandOff()
    {
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            if (_fadeImg) _fadeImg.color = new Color(0f, 0f, 0f, t / 0.8f);
            yield return null;
        }
        if (_fadeImg) _fadeImg.color = Color.black;
        if (slideshowManager != null)
        {
            slideshowManager.SetActive(true);
            _cv.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        else SceneManager.LoadScene(nextScene);
    }

    void BuildTitleUI()
    {
        var cvGo = new GameObject("TitleCanvas");
        _cv = cvGo.AddComponent<Canvas>();
        _cv.renderMode = RenderMode.ScreenSpaceOverlay; _cv.sortingOrder = 10;
        var scaler = cvGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cvGo.AddComponent<GraphicRaycaster>();
        Font fnt = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Add<Image>("BG", _cv.transform, Vector2.zero, Vector2.one).color = new Color(0.03f, 0.03f, 0.10f, 1f);
        _titleRT = AddRT("TitleHolder", _cv.transform, new Vector2(0f,0.54f), new Vector2(1f,0.90f));
        var tTxt = _titleRT.gameObject.AddComponent<Text>();
        tTxt.font = fnt; tTxt.fontSize = 118; tTxt.fontStyle = FontStyle.Bold;
        tTxt.alignment = TextAnchor.MiddleCenter; tTxt.color = new Color(1f,0.88f,0.28f,1f); tTxt.text = "ASHTAR";
        var sTxt = Add<Text>("Subtitle", _cv.transform, new Vector2(0.1f,0.46f), new Vector2(0.9f,0.55f));
        sTxt.font = fnt; sTxt.fontSize = 30; sTxt.fontStyle = FontStyle.Italic;
        sTxt.alignment = TextAnchor.MiddleCenter; sTxt.color = new Color(0.70f,0.80f,1f,1f);
        sTxt.text = "A GALACTIC MISSION"; sTxt.raycastTarget = false;
        var btnRT = AddRT("StartBtn", _cv.transform, new Vector2(0.36f,0.26f), new Vector2(0.64f,0.38f));
        var btnImg = btnRT.gameObject.AddComponent<Image>(); btnImg.color = new Color(0.10f,0.38f,0.75f,0.92f);
        var btn = btnRT.gameObject.AddComponent<Button>();
        var bCB = ColorBlock.defaultColorBlock;
        bCB.normalColor = new Color(0.10f,0.38f,0.75f,0.92f);
        bCB.highlightedColor = new Color(0.22f,0.55f,1.00f,1.00f);
        bCB.pressedColor = new Color(0.05f,0.20f,0.50f,1.00f);
        btn.colors = bCB; btn.targetGraphic = btnImg; btn.onClick.AddListener(OnStartClicked);
        var lblTxt = Add<Text>("Label", btnRT, Vector2.zero, Vector2.one);
        lblTxt.font = fnt; lblTxt.fontSize = 46; lblTxt.fontStyle = FontStyle.Bold;
        lblTxt.alignment = TextAnchor.MiddleCenter; lblTxt.color = Color.white; lblTxt.text = "START";
        var hTxt = Add<Text>("Hint", _cv.transform, new Vector2(0.2f,0.14f), new Vector2(0.8f,0.22f));
        hTxt.font = fnt; hTxt.fontSize = 20; hTxt.fontStyle = FontStyle.Italic;
        hTxt.alignment = TextAnchor.MiddleCenter; hTxt.color = new Color(0.5f,0.5f,0.5f,0.9f);
        hTxt.text = "click START  \u2022  Space  \u2022  Enter"; hTxt.raycastTarget = false;
        var foImg = Add<Image>("FadeOverlay", _cv.transform, Vector2.zero, Vector2.one);
        foImg.color = new Color(0f,0f,0f,0f); foImg.raycastTarget = false;
        foImg.transform.SetAsLastSibling(); _fadeImg = foImg;
    }

    void StartAudio()
    {
        if (backgroundMusic == null) return;
        var src = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        src.clip = backgroundMusic; src.loop = true; src.volume = 0.65f; src.Play();
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
    }

    static RectTransform AddRT(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }

    static T Add<T>(string name, Transform parent, Vector2 aMin, Vector2 aMax) where T : Component
    {
        return AddRT(name, parent, aMin, aMax).gameObject.AddComponent<T>();
    }
}
