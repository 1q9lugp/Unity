using UnityEngine;
using UnityEngine.UI;

public class ArenaHUD : MonoBehaviour
{
    [Header("Face Icon — drag AshtarIcon sprite here")]
    public Sprite faceSprite;

    FPSController _player;
    Text _ammoNum, _healthNum, _armorNum, _killNum;
    int  _kills;

    public void AddKill() { _kills++; }

    void Start()
    {
        _player = FindObjectOfType<FPSController>();
        BuildHUD();
    }

   void BuildHUD()
    {
        var cv = GetComponent<Canvas>();
        if (cv == null) return;

        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Color yellow = new Color(0.95f, 0.85f, 0.10f);
        Color red    = new Color(0.95f, 0.25f, 0.20f);
        Color green  = new Color(0.20f, 0.90f, 0.30f);
        Color lightBlue = new Color(0.30f, 0.80f, 0.95f);
        Color panelBg = new Color(0.02f, 0.02f, 0.02f, 0.75f); // Darker for better contrast

        // ── LEFT MODULE: VITALS ──────────────────────────────────────────
        var leftGroup = CreateModule("VitalsModule", new Vector2(0, 0), new Vector2(260, 120), new Vector2(30, 30), panelBg);
        
        _armorNum = CreateStat(leftGroup, "ARMOR: ", new Vector2(10, 30), 28, lightBlue, TextAnchor.MiddleLeft);
        _healthNum = CreateStat(leftGroup, "HEALTH: ", new Vector2(10, -20), 44, red, TextAnchor.MiddleLeft);

        // ── RIGHT MODULE: COMBAT ─────────────────────────────────────────
        var rightGroup = CreateModule("CombatModule", new Vector2(1, 0), new Vector2(260, 120), new Vector2(-30, 30), panelBg);

        _killNum = CreateStat(rightGroup, "LIZARDS: ", new Vector2(-10, 30), 28, green, TextAnchor.MiddleRight);
        _ammoNum = CreateStat(rightGroup, "AMMO: ", new Vector2(-10, -20), 44, yellow, TextAnchor.MiddleRight);
    }

    GameObject CreateModule(string name, Vector2 anchor, Vector2 size, Vector2 pos, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = bg;
        return go;
    }

    Text CreateStat(GameObject parent, string labelPrefix, Vector2 pos, int fontSize, Color col, TextAnchor align)
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var go = new GameObject(labelPrefix + "Text");
        go.transform.SetParent(parent.transform, false);
        
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(240, 50);

        var txt = go.AddComponent<Text>();
        txt.font = f;
        txt.fontSize = fontSize;
        txt.fontStyle = FontStyle.Bold;
        txt.color = col;
        txt.alignment = align;
        txt.text = labelPrefix + "0"; // Initial text format

        // Visibility Upgrade: Add a black outline to make text pop
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return txt;
    }}