using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Act2Controller : MonoBehaviour
{
    public static AudioSource CinematicMusic; // kept so PlanetEncounterManager still compiles

    public AudioClip cinematicSong; // drag "At Doom's Gate" here in Inspector

    Text _label;

    void Start()
    {
        BuildUI();

        if (cinematicSong != null)
        {
            var src    = gameObject.AddComponent<AudioSource>();
            src.clip   = cinematicSong;
            src.time   = 32f;          // start at 33s into the track
            src.volume = 0.8f;
            src.loop   = false;
            src.Play();
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
        var bgR  = bgGo.AddComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one;
        bgR.offsetMin = Vector2.zero; bgR.offsetMax = Vector2.zero;
        bgGo.AddComponent<Image>().color = Color.black;

        var lblGo = new GameObject("Label");
        lblGo.transform.SetParent(cvGo.transform, false);
        var lblR  = lblGo.AddComponent<RectTransform>();
        lblR.anchorMin = Vector2.zero; lblR.anchorMax = Vector2.one;
        lblR.offsetMin = Vector2.zero; lblR.offsetMax = Vector2.zero;
        _label           = lblGo.AddComponent<Text>();
        _label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize  = 52;
        _label.fontStyle = FontStyle.Italic;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.color     = new Color(0.95f, 0.95f, 0.8f, 1f);
        _label.text      = "";
    }

    IEnumerator ShowFrames()
    {
        yield return new WaitForSeconds(2f);
        _label.text = "AŠTAR: Nastavte kurz...";

        yield return new WaitForSeconds(5f);
        _label.text = "AŠTAR: ZEMĚ!";

        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(4);
    }
}