#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PatchJesus
{
    [MenuItem("Tools/Patch Jesus Reveal")]
    public static void Run()
    {
        const string path = "Assets/Scripts/Space/PlanetEncounterManager.cs";
        string src = File.ReadAllText(path);

        // Find and replace the entire JesusReveal method.
        // Anchor on the method signature and replace up to its closing brace.
        int start = src.IndexOf("IEnumerator JesusReveal()");
        if (start < 0) { Debug.LogError("[PatchJesus] JesusReveal not found."); return; }

        // Find the matching closing brace of JesusReveal
        int braceStart = src.IndexOf('{', start);
        int depth = 0; int end = braceStart;
        for (int i = braceStart; i < src.Length; i++)
        {
            if (src[i] == '{') depth++;
            else if (src[i] == '}') { depth--; if (depth == 0) { end = i + 1; break; } }
        }

        string newMethod =
"IEnumerator JesusReveal()\n" +
"{\n" +
"    // FREEZE enemies immediately — before the image even appears\n" +
"    Time.timeScale = 0f;\n" +
"\n" +
"    if (_jesusImg != null)\n" +
"    {\n" +
"        if (jesusSprite != null) _jesusImg.sprite = jesusSprite;\n" +
"        // Stretch to fill entire canvas with no padding\n" +
"        _jesusImg.preserveAspect = false;\n" +
"        var jrt = _jesusImg.rectTransform;\n" +
"        jrt.anchorMin = Vector2.zero;\n" +
"        jrt.anchorMax = Vector2.one;\n" +
"        jrt.offsetMin = Vector2.zero;\n" +
"        jrt.offsetMax = Vector2.zero;\n" +
"        // Bring above the FadeOverlay so nothing clips it\n" +
"        _jesusImg.transform.SetAsLastSibling();\n" +
"        _jesusImg.gameObject.SetActive(true);\n" +
"        _jesusImg.color = new Color(1f, 1f, 1f, 0f);\n" +
"        // Fade in to SEMI-TRANSPARENT (alpha 0.78) using unscaled time\n" +
"        const float targetAlpha = 0.78f;\n" +
"        for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)\n" +
"        {\n" +
"            _jesusImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, targetAlpha, t / 0.6f));\n" +
"            yield return null;\n" +
"        }\n" +
"        _jesusImg.color = new Color(1f, 1f, 1f, targetAlpha);\n" +
"    }\n" +
"\n" +
"    if (_announceTxt != null)\n" +
"    {\n" +
"        _announceTxt.text = \"ASHTAR SHERAN \\u2014 ONE FINAL CHANCE\";\n" +
"        _announceTxt.color = new Color(1f, 0.85f, 0.1f, 1f);\n" +
"        _announceTxt.fontSize = 34;\n" +
"        _announceTxt.transform.SetAsLastSibling();\n" +
"    }\n" +
"    if (_jesusPromptTxt != null)\n" +
"    {\n" +
"        _jesusPromptTxt.transform.SetAsLastSibling();\n" +
"        _jesusPromptTxt.gameObject.SetActive(true);\n" +
"    }\n" +
"\n" +
"    // Wait for player click/key — unscaled so input works while timeScale=0\n" +
"    yield return null;\n" +
"    yield return new WaitUntil(() =>\n" +
"        Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||\n" +
"        Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||\n" +
"        Input.GetKeyDown(KeyCode.E));\n" +
"\n" +
"    // Restore time BEFORE fade-out so normal deltaTime works\n" +
"    Time.timeScale = 1f;\n" +
"    if (_jesusPromptTxt != null) _jesusPromptTxt.gameObject.SetActive(false);\n" +
"    if (_announceTxt != null) { _announceTxt.text = \"\"; _announceTxt.color = Color.clear; }\n" +
"\n" +
"    if (_jesusImg != null)\n" +
"    {\n" +
"        float startA = _jesusImg.color.a;\n" +
"        for (float t = 0f; t < 0.5f; t += Time.deltaTime)\n" +
"        {\n" +
"            _jesusImg.color = new Color(1f, 1f, 1f, Mathf.Lerp(startA, 0f, t / 0.5f));\n" +
"            yield return null;\n" +
"        }\n" +
"        _jesusImg.color = new Color(1f, 1f, 1f, 0f);\n" +
"        _jesusImg.gameObject.SetActive(false);\n" +
"    }\n" +
"\n" +
"    _extraLifeUsed = true;\n" +
"    if (_lives != null) { _lives.lives = 1; _lives.RefreshHearts(); }\n" +
"}";

        src = src.Substring(0, start) + newMethod + src.Substring(end);
        File.WriteAllText(path, src);
        AssetDatabase.Refresh();
        Debug.Log("[PatchJesus] JesusReveal patched successfully.");
    }
}
#endif
