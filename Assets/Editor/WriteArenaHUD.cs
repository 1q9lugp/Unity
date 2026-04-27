#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WriteArenaHUD
{
    [MenuItem("Tools/Write Arena HUD")]
    public static void Run()
    {
        File.WriteAllText("Assets/Scripts/Arena/ArenaHUD.cs", CODE);
        AssetDatabase.Refresh();
        Debug.Log("[WriteArenaHUD] Done.");
    }

    const string CODE =
"using UnityEngine;\n" +
"using UnityEngine.UI;\n" +
"\n" +
"public class ArenaHUD : MonoBehaviour\n" +
"{\n" +
"    public Sprite faceSprite;\n" +
"\n" +
"    FPSController _player;\n" +
"    Text _ammoVal, _healthVal, _armorVal;\n" +
"\n" +
"    void Start()\n" +
"    {\n" +
"        _player = FindObjectOfType<FPSController>();\n" +
"        var cv = GetComponent<Canvas>();\n" +
"        if (cv == null) { cv = gameObject.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceOverlay; }\n" +
"        if (GetComponent<CanvasScaler>() == null) { var cs = gameObject.AddComponent<CanvasScaler>(); cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; cs.referenceResolution = new Vector2(1920f,1080f); }\n" +
"        if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();\n" +
"        Font f = Resources.GetBuiltinResource<Font>(\"LegacyRuntime.ttf\");\n" +
"        var strip = new GameObject(\"HUDStrip\");\n" +
"        strip.transform.SetParent(transform, false);\n" +
"        var sr = strip.AddComponent<RectTransform>();\n" +
"        sr.anchorMin = new Vector2(0f,0f); sr.anchorMax = new Vector2(1f,0f);\n" +
"        sr.pivot = new Vector2(0.5f,0f); sr.anchoredPosition = Vector2.zero; sr.sizeDelta = new Vector2(0f,90f);\n" +
"        strip.AddComponent<Image>().color = new Color(0.08f,0.08f,0.08f,0.96f);\n" +
"        _ammoVal   = MakeSection(strip.transform, f, \"AMMO\",   0.12f, new Color(0f,0.82f,1f));\n" +
"        _healthVal = MakeSection(strip.transform, f, \"HEALTH\", 0.35f, new Color(0.2f,1f,0.2f));\n" +
"        _armorVal  = MakeSection(strip.transform, f, \"ARMOR\",  0.65f, new Color(1f,0.65f,0f));\n" +
"        var ig = new GameObject(\"FaceIcon\"); ig.transform.SetParent(strip.transform, false);\n" +
"        var ir = ig.AddComponent<RectTransform>();\n" +
"        ir.anchorMin = new Vector2(0.5f,0f); ir.anchorMax = new Vector2(0.5f,1f);\n" +
"        ir.pivot = new Vector2(0.5f,0.5f); ir.anchoredPosition = Vector2.zero; ir.sizeDelta = new Vector2(76f,-10f);\n" +
"        var fi = ig.AddComponent<Image>(); fi.preserveAspect = true;\n" +
"        if (faceSprite != null) fi.sprite = faceSprite; else fi.color = new Color(0.35f,0.35f,0.35f);\n" +
"        MakeDivider(strip.transform, 0.24f); MakeDivider(strip.transform, 0.46f);\n" +
"        MakeDivider(strip.transform, 0.54f); MakeDivider(strip.transform, 0.76f);\n" +
"    }\n" +
"\n" +
"    Text MakeSection(Transform parent, Font f, string label, float xAnchor, Color col)\n" +
"    {\n" +
"        var root = new GameObject(\"Sec_\" + label); root.transform.SetParent(parent, false);\n" +
"        var rt = root.AddComponent<RectTransform>();\n" +
"        rt.anchorMin = new Vector2(xAnchor,0f); rt.anchorMax = new Vector2(xAnchor,1f);\n" +
"        rt.pivot = new Vector2(0.5f,0.5f); rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(180f,0f);\n" +
"        var vg = new GameObject(\"Val\"); vg.transform.SetParent(root.transform, false);\n" +
"        var vr = vg.AddComponent<RectTransform>();\n" +
"        vr.anchorMin = new Vector2(0f,0.28f); vr.anchorMax = Vector2.one; vr.offsetMin = Vector2.zero; vr.offsetMax = Vector2.zero;\n" +
"        var vt = vg.AddComponent<Text>(); vt.font = f; vt.fontSize = 46; vt.fontStyle = FontStyle.Bold;\n" +
"        vt.alignment = TextAnchor.MiddleCenter; vt.color = col; vt.text = \"---\";\n" +
"        var lg = new GameObject(\"Lbl\"); lg.transform.SetParent(root.transform, false);\n" +
"        var lr = lg.AddComponent<RectTransform>();\n" +
"        lr.anchorMin = Vector2.zero; lr.anchorMax = new Vector2(1f,0.3f); lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;\n" +
"        var lt = lg.AddComponent<Text>(); lt.font = f; lt.fontSize = 15; lt.fontStyle = FontStyle.Bold;\n" +
"        lt.alignment = TextAnchor.MiddleCenter; lt.color = new Color(0.55f,0.55f,0.55f); lt.text = label;\n" +
"        return vt;\n" +
"    }\n" +
"\n" +
"    void MakeDivider(Transform parent, float xAnchor)\n" +
"    {\n" +
"        var dg = new GameObject(\"Div\"); dg.transform.SetParent(parent, false);\n" +
"        var dr = dg.AddComponent<RectTransform>();\n" +
"        dr.anchorMin = new Vector2(xAnchor,0.08f); dr.anchorMax = new Vector2(xAnchor,0.92f);\n" +
"        dr.pivot = new Vector2(0.5f,0.5f); dr.anchoredPosition = Vector2.zero; dr.sizeDelta = new Vector2(2f,0f);\n" +
"        dg.AddComponent<Image>().color = new Color(0.28f,0.28f,0.28f,0.9f);\n" +
"    }\n" +
"\n" +
"    void Update()\n" +
"    {\n" +
"        if (_player == null) return;\n" +
"        if (_ammoVal   != null) _ammoVal.text   = _player.ammo.ToString();\n" +
"        if (_healthVal != null) _healthVal.text = _player.health.ToString() + \"%\";\n" +
"        if (_armorVal  != null) _armorVal.text  = _player.armor.ToString()  + \"%\";\n" +
"    }\n" +
"}\n";
}
#endif
