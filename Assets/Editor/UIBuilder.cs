
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class UIBuilder
{
    [MenuItem("Tools/SpaceSetup/2 Build UI")]
    public static void Build()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

        // Remove HealthBar (inactive)
        foreach (var g in Resources.FindObjectsOfTypeAll<GameObject>())
            if (g.name == "HealthBar" && g.scene.IsValid()) { Undo.DestroyObjectImmediate(g); break; }

        var cv = Object.FindObjectOfType<Canvas>();
        if (cv == null) { Debug.LogError("[UIBuilder] No Canvas found."); return; }

        // ── LivesDisplay ──────────────────────────────────────────────────────
        var ld = new GameObject("LivesDisplay"); ld.transform.SetParent(cv.transform, false);
        var ldR = ld.AddComponent<RectTransform>();
        ldR.anchorMin = Vector2.zero; ldR.anchorMax = Vector2.zero; ldR.pivot = Vector2.zero;
        ldR.sizeDelta = new Vector2(120f, 30f); ldR.anchoredPosition = new Vector2(10f, 10f);

        float[] hx = { 15f, 55f, 95f };
        string[] hn = { "Heart1", "Heart2", "Heart3" };
        for (int i = 0; i < 3; i++)
        {
            var h = new GameObject(hn[i]); h.transform.SetParent(ld.transform, false);
            var hR = h.AddComponent<RectTransform>();
            hR.anchorMin = Vector2.zero; hR.anchorMax = Vector2.zero; hR.pivot = new Vector2(0.5f, 0.5f);
            hR.sizeDelta = new Vector2(30f, 30f); hR.anchoredPosition = new Vector2(hx[i], 15f);
            h.AddComponent<Image>().color = new Color(1f, 0.196f, 0.196f, 1f);
        }

        // ── GameOverPanel ─────────────────────────────────────────────────────
        var panel = new GameObject("GameOverPanel"); panel.transform.SetParent(cv.transform, false);
        var pR = panel.AddComponent<RectTransform>();
        pR.anchorMin = Vector2.zero; pR.anchorMax = Vector2.one;
        pR.offsetMin = Vector2.zero; pR.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.706f);

        var mf = new GameObject("MissionFailedText"); mf.transform.SetParent(panel.transform, false);
        var mfR = mf.AddComponent<RectTransform>();
        mfR.anchorMin = new Vector2(0.5f, 0.5f); mfR.anchorMax = new Vector2(0.5f, 0.5f);
        mfR.pivot = new Vector2(0.5f, 0.5f); mfR.sizeDelta = new Vector2(640f, 100f);
        mfR.anchoredPosition = new Vector2(0f, 60f);
        var mfT = mf.AddComponent<Text>();
        mfT.text = "MISSION FAILED"; mfT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mfT.fontSize = 48; mfT.fontStyle = FontStyle.Bold;
        mfT.alignment = TextAnchor.MiddleCenter; mfT.color = Color.white;

        var btn = new GameObject("TryAgainButton"); btn.transform.SetParent(panel.transform, false);
        var bR = btn.AddComponent<RectTransform>();
        bR.anchorMin = new Vector2(0.5f, 0.5f); bR.anchorMax = new Vector2(0.5f, 0.5f);
        bR.pivot = new Vector2(0.5f, 0.5f); bR.sizeDelta = new Vector2(200f, 50f);
        bR.anchoredPosition = new Vector2(0f, -30f);
        btn.AddComponent<Image>().color = new Color(0.8f, 0.15f, 0.15f, 1f);
        btn.AddComponent<Button>();

        var lbl = new GameObject("Label"); lbl.transform.SetParent(btn.transform, false);
        var lR = lbl.AddComponent<RectTransform>();
        lR.anchorMin = Vector2.zero; lR.anchorMax = Vector2.one;
        lR.offsetMin = Vector2.zero; lR.offsetMax = Vector2.zero;
        var lT = lbl.AddComponent<Text>();
        lT.text = "TRY AGAIN"; lT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lT.fontSize = 22; lT.fontStyle = FontStyle.Bold;
        lT.alignment = TextAnchor.MiddleCenter; lT.color = Color.white;

        panel.SetActive(false);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[UIBuilder] LivesDisplay + GameOverPanel built.");
    }
}
#endif
