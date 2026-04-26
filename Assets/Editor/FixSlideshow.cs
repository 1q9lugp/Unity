#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FixSlideshow
{
    [MenuItem("Tools/Fix Slideshow Text")]
    public static void Run()
    {
        const string path = "Assets/Scripts/Cutscene/SlideshowController.cs";
        string src = File.ReadAllText(path);

        // Find Start() method and replace it entirely
        int start = src.IndexOf("    private void Start()");
        int braceStart = src.IndexOf('{', start);
        int depth = 0, end = braceStart;
        for (int i = braceStart; i < src.Length; i++)
        {
            if (src[i] == '{') depth++;
            else if (src[i] == '}') { depth--; if (depth == 0) { end = i + 1; break; } }
        }

        const string newStart =
"    private void Start()\n" +
"    {\n" +
"        if (musicSource != null && backgroundMusic != null)\n" +
"        {\n" +
"            musicSource.clip = backgroundMusic;\n" +
"            musicSource.loop = true;\n" +
"            musicSource.Play();\n" +
"        }\n" +
"\n" +
"        if (slideText != null)\n" +
"        {\n" +
"            slideText.color = Color.white;\n" +
"            slideText.fontStyle = FontStyle.Bold;\n" +
"\n" +
"            var ol = slideText.gameObject.GetComponent<Outline>();\n" +
"            if (ol == null) ol = slideText.gameObject.AddComponent<Outline>();\n" +
"            ol.effectColor = new Color(0f, 0f, 0f, 0.95f);\n" +
"            ol.effectDistance = new Vector2(2f, -2f);\n" +
"\n" +
"            var sh = slideText.gameObject.GetComponent<Shadow>();\n" +
"            if (sh == null) sh = slideText.gameObject.AddComponent<Shadow>();\n" +
"            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);\n" +
"            sh.effectDistance = new Vector2(3f, -3f);\n" +
"\n" +
"            // Dark semi-transparent backing behind text\n" +
"            var parent = slideText.transform.parent ?? slideText.transform;\n" +
"            if (parent.Find(\"TextBacking\") == null)\n" +
"            {\n" +
"                var bg = new GameObject(\"TextBacking\");\n" +
"                bg.transform.SetParent(parent, false);\n" +
"                var bgRt = bg.AddComponent<RectTransform>();\n" +
"                var txtRt = slideText.GetComponent<RectTransform>();\n" +
"                bgRt.anchorMin = txtRt.anchorMin;\n" +
"                bgRt.anchorMax = txtRt.anchorMax;\n" +
"                bgRt.offsetMin = txtRt.offsetMin + new Vector2(-12f, -10f);\n" +
"                bgRt.offsetMax = txtRt.offsetMax + new Vector2(12f, 10f);\n" +
"                var bgImg = bg.AddComponent<Image>();\n" +
"                bgImg.color = new Color(0f, 0f, 0f, 0.55f);\n" +
"                bgImg.raycastTarget = false;\n" +
"                bg.transform.SetSiblingIndex(slideText.transform.GetSiblingIndex());\n" +
"            }\n" +
"        }\n" +
"\n" +
"        StartCoroutine(PlaySlideshow());\n" +
"    }";

        src = src.Substring(0, start) + newStart + src.Substring(end);
        File.WriteAllText(path, src);
        AssetDatabase.Refresh();
        Debug.Log("[FixSlideshow] Done.");
    }
}
#endif
