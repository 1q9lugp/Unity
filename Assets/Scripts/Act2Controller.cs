using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Act2Controller : MonoBehaviour
{
    public AudioClip music;

    static readonly string[] Frames =
    {
        "AŠTAR: Nastavte kurz...",
        "AŠTAR: ZEMĚ."
    };

    Text _label;

    void Start()
    {
        BuildUI();
        if (music != null)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.clip = music; a.loop = true; a.volume = 0.7f; a.Play();
        }
        StartCoroutine(ShowFrames());
    }

    void BuildUI()
    {
        var cvGo = new GameObject("Canvas");
        var cv   = cvGo.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cvGo.AddComponent<CanvasScaler>();
        cvGo.AddComponent<GraphicRaycaster>();

        var bgGo = new GameObject("BG");
        bgGo.transform.SetParent(cvGo.transform, false);
        var bgR = bgGo.AddComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one;
        bgR.offsetMin = Vector2.zero; bgR.offsetMax = Vector2.zero;
        bgGo.AddComponent<Image>().color = Color.black;

        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(cvGo.transform, false);
        var lblR = lblGo.AddComponent<RectTransform>();
        lblR.anchorMin = Vector2.zero; lblR.anchorMax = Vector2.one;
        lblR.offsetMin = Vector2.zero; lblR.offsetMax = Vector2.zero;
        _label = lblGo.AddComponent<Text>();
        _label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize  = 52;
        _label.fontStyle = FontStyle.Italic;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.color     = new Color(0.95f, 0.95f, 0.8f, 1f);
        _label.text      = "";
    }

    IEnumerator ShowFrames()
    {
        foreach (var line in Frames)
        {
            _label.text = line;
            yield return null;
            yield return new WaitUntil(() =>
                Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space)  ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.E));
        }
        SceneManager.LoadScene(4);
    }
}