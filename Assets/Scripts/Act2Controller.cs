using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Act2Controller : MonoBehaviour
{
    // Assigned by PlanetEncounterManager before the scene transition
    public static AudioSource CinematicMusic;

    // Original track timestamps
    const float Drop1 = 25f; // "Nastavte kurz..."
    const float Drop2 = 30f; // "ZEMĚ!"

    Text _label;

    void Start()
    {
        BuildUI();
        StartCoroutine(ShowFrames());
    }

    void BuildUI()
    {
        var cvGo = new GameObject("Canvas");
        var cv = cvGo.AddComponent<Canvas>();
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
        if (CinematicMusic != null)
        {
            // Wait for drop 1 (track hits 25s) → show frame 1
            yield return new WaitUntil(() => CinematicMusic.time >= Drop1);
            _label.text = "AŠTAR: Nastavte kurz...";

            // Wait for drop 2 (track hits 30s) → show frame 2
            yield return new WaitUntil(() => CinematicMusic.time >= Drop2);
            _label.text = "AŠTAR: ZEMĚ!";

            yield return new WaitForSeconds(3f);
        }
        else
        {
            // Fallback if launched without going through Act1
            _label.text = "AŠTAR: Nastavte kurz...";
            yield return new WaitForSeconds(5f);
            _label.text = "AŠTAR: ZEMĚ!";
            yield return new WaitForSeconds(3f);
        }

        SceneManager.LoadScene(4);
    }
}