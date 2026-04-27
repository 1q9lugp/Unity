using UnityEngine;
using UnityEngine.UI;

public class ArenaHUD : MonoBehaviour
{
    FPSController _player;
    Text _ammoNum, _healthNum, _armorNum, _killNum;
    int _kills;

    public void AddKill() { _kills++; }

    void Start()
    {
        _player = FindObjectOfType<FPSController>();
        
        if (_player != null)
        {
            _player.health = 100;
            _player.ammo = 150;
            _player.armor = 50; 
        }

        BuildHUD();
    }

    void Update()
    {
        if (_player == null) return;

        // Updating the full strings as seen in your preferred version
        if (_healthNum != null) _healthNum.text = "HEALTH: " + _player.health;
        if (_armorNum  != null) _armorNum.text  = "ARMOR: "  + _player.armor + "%";
        if (_ammoNum   != null) _ammoNum.text   = "AMMO: "   + _player.ammo;
        if (_killNum   != null) _killNum.text   = "LIZARDS: " + _kills;
    }

    void BuildHUD()
    {
        var cv = GetComponent<Canvas>();
        if (cv == null) return;

        Color yellow = new Color(0.95f, 0.85f, 0.10f);
        Color red    = new Color(0.95f, 0.20f, 0.15f);
        Color green  = new Color(0.15f, 0.85f, 0.25f);
        Color lightBlue = new Color(0.30f, 0.80f, 0.95f);
        Color panelBg = new Color(0.12f, 0.12f, 0.12f, 0.65f);

        // 20% Smaller Scale: 350x140 -> 280x112
        Vector2 moduleSize = new Vector2(280, 112);

        // Positioned perfectly at (0,0) for the bottom edges
        var left = CreateModule("Vitals", new Vector2(0, 0), moduleSize, Vector2.zero, panelBg);
        _armorNum  = CreateStat(left, "ARMOR: ",  new Vector2(10, 25), 22, lightBlue, TextAnchor.MiddleLeft);
        _healthNum = CreateStat(left, "HEALTH: ", new Vector2(10, -22), 36, red,       TextAnchor.MiddleLeft);

        var right = CreateModule("Combat", new Vector2(1, 0), moduleSize, Vector2.zero, panelBg);
        _killNum = CreateStat(right, "LIZARDS: ", new Vector2(-10, 25), 22, green,  TextAnchor.MiddleRight);
        _ammoNum = CreateStat(right, "AMMO: ",    new Vector2(-10, -22), 36, yellow, TextAnchor.MiddleRight);
    }

    GameObject CreateModule(string name, Vector2 anchor, Vector2 size, Vector2 pos, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    Text CreateStat(GameObject parent, string label, Vector2 pos, int fontSize, Color col, TextAnchor align)
    {
        var go = new GameObject(label + "Text");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(260, 60); // Sufficient width to prevent Health from clipping

        var txt = go.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.fontStyle = FontStyle.Bold;
        txt.color = col;
        txt.alignment = align;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow; 
        txt.text = label + "0";

        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2); 

        return txt;
    }
}