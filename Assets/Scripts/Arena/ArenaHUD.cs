using UnityEngine;
using UnityEngine.UI;

public class ArenaHUD : MonoBehaviour
{
    FPSController _player;
    Text _healthNum, _armorNum, _ammoNum, _killNum;
    int  _kills;

    public void AddKill()
    {
        _kills++;
    }

    void Start()
    {
        _player = FindObjectOfType<FPSController>();
        BuildHUD();
    }

    void Update()
    {
        if (_player == null) return;
        if (_healthNum != null) _healthNum.text = "HEALTH: " + _player.health;
        if (_armorNum  != null) _armorNum.text  = "ARMOR: "  + _player.armor + "%";
        if (_ammoNum   != null) _ammoNum.text   = "AMMO: "   + _player.ammo;
        if (_killNum   != null) _killNum.text   = "LIZARDS: " + _kills;
    }

    void BuildHUD()
    {
        var cv = GetComponent<Canvas>();
        if (cv == null) return;

        Color yellow    = new Color(0.95f, 0.85f, 0.10f);
        Color red       = new Color(0.95f, 0.25f, 0.20f);
        Color green     = new Color(0.20f, 0.90f, 0.30f);
        Color lightBlue = new Color(0.30f, 0.80f, 0.95f);
        Color panelBg   = new Color(0.02f, 0.02f, 0.02f, 0.75f);

        var left = MakePanel("VitalsModule", new Vector2(0, 0), new Vector2(260, 120), new Vector2(30, 30), panelBg);
        _armorNum  = MakeStat(left, "ARMOR: 0",   new Vector2(10,  30), 28, lightBlue, TextAnchor.MiddleLeft);
        _healthNum = MakeStat(left, "HEALTH: 0",  new Vector2(10, -20), 44, red,       TextAnchor.MiddleLeft);

        var right = MakePanel("CombatModule", new Vector2(1, 0), new Vector2(260, 120), new Vector2(-30, 30), panelBg);
        _killNum = MakeStat(right, "LIZARDS: 0", new Vector2(-10,  30), 28, green,  TextAnchor.MiddleRight);
        _ammoNum = MakeStat(right, "AMMO: 0",    new Vector2(-10, -20), 44, yellow, TextAnchor.MiddleRight);
    }

    GameObject MakePanel(string goName, Vector2 anchor, Vector2 size, Vector2 pos, Color bg)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    Text MakeStat(GameObject parent, string initial, Vector2 pos, int fontSize, Color col, TextAnchor align)
    {
        var go = new GameObject(initial + "_txt");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(240, 50);
        var txt       = go.AddComponent<Text>();
        txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize  = fontSize;
        txt.fontStyle = FontStyle.Bold;
        txt.color     = col;
        txt.alignment = align;
        txt.text      = initial;
        var outline = go.AddComponent<Outline>();
        outline.effectColor    = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        return txt;
    }
}