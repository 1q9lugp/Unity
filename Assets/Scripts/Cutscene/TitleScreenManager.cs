using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleScreenManager : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite backgroundSprite;

    [Header("Audio")]
    public AudioClip backgroundMusic;

    [Header("Flow")]
    public string nextScene = "Act0_Briefing";

    Image _fadeImg;
    bool  _starting;

    void Awake()
    {
        EnsureEventSystem();
        BuildUI();
        StartAudio();
    }

    void Update()
    {
        if (!_starting && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            OnStartClicked();
    }

    public void OnStartClicked()
    {
        if (_starting) return;
        _starting = true;
        StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        for (float t = 0f; t < 0.8f; t += Time.deltaTime)
        {
            if (_fadeImg) _fadeImg.color = new Color(0f, 0f, 0f, t / 0.8f);
            yield return null;
        }
        if (_fadeImg) _fadeImg.color = Color.black;
        SceneManager.LoadScene(nextScene);
    }

    void BuildUI()
    {
        var cvGo = new GameObject("TitleCanvas");
        var cv   = cvGo.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 10;
        var scaler = cvGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cvGo.AddComponent<GraphicRaycaster>();

        var bgImg = MakeImg("BG", cv.transform, Vector2.zero, Vector2.one);
        if (backgroundSprite != null) { bgImg.sprite = backgroundSprite; bgImg.color = Color.white; bgImg.preserveAspect = false; }
        else bgImg.color = new Color(0.05f, 0.05f, 0.05f, 1f);

        Font fnt  = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var btnRT = MakeRT("StartBtn", cv.transform, new Vector2(0.35f, 0.12f), new Vector2(0.65f, 0.24f));
        var bi    = btnRT.gameObject.AddComponent<Image>();
        bi.color  = new Color(0.10f, 0.38f, 0.75f, 0.92f);
        var btn   = btnRT.gameObject.AddComponent<Button>();
        var cb    = ColorBlock.defaultColorBlock;
        cb.normalColor      = new Color(0.10f, 0.38f, 0.75f, 0.92f);
        cb.highlightedColor = new Color(0.22f, 0.55f, 1.00f, 1.00f);
        cb.pressedColor     = new Color(0.05f, 0.20f, 0.50f, 1.00f);
        btn.colors = cb; btn.targetGraphic = bi;
        btn.onClick.AddListener(OnStartClicked);
        var lbl = MakeRT("Label", btnRT, Vector2.zero, Vector2.one).gameObject.AddComponent<Text>();
        lbl.font = fnt; lbl.fontSize = 52; lbl.fontStyle = FontStyle.Bold;
        lbl.alignment = TextAnchor.MiddleCenter; lbl.color = Color.white; lbl.text = "START";

        var fade = MakeImg("FadeOverlay", cv.transform, Vector2.zero, Vector2.one);
        fade.color = new Color(0f, 0f, 0f, 0f); fade.raycastTarget = false;
        fade.transform.SetAsLastSibling();
        _fadeImg = fade;
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

    static RectTransform MakeRT(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }

    static Image MakeImg(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        return MakeRT(name, parent, aMin, aMax).gameObject.AddComponent<Image>();
    }
}
