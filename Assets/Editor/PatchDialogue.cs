#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PatchDialogue
{
    [MenuItem("Tools/Patch Dialogue System")]
    public static void Run()
    {
        const string p = "Assets/Scripts/Space/PlanetEncounterManager.cs";
        string s = File.ReadAllText(p);

        // ── 1. Add lizardIconSprite field next to the other icon fields ──────
        s = s.Replace(
            "public Sprite ashtarIconSprite, ptaahIconSprite, jesusSprite;",
            "public Sprite ashtarIconSprite, ptaahIconSprite, jesusSprite, lizardIconSprite;");

        // ── 2. ShowDlg: add LIZARD icon support ───────────────────────────────
        s = s.Replace(
            "bool a=msg.StartsWith(\"ASHTAR:\");bool b=msg.StartsWith(\"PTAAH:\");_speakerIcon.sprite=a?ashtarIconSprite:(b?ptaahIconSprite:null);",
            "bool a=msg.StartsWith(\"ASHTAR:\");bool b=msg.StartsWith(\"PTAAH:\");bool liz=msg.StartsWith(\"LIZARD:\");_speakerIcon.sprite=a?ashtarIconSprite:(b?ptaahIconSprite:(liz?lizardIconSprite:null));");

        // ── 3. GameLoop: call IntroDialogue before planet loop ────────────────
        s = s.Replace(
            "IEnumerator GameLoop()\n    {\n        for (_idx=0;_idx<planets.Length;_idx++) yield return StartCoroutine(RunPlanet());\n        yield return StartCoroutine(EndingSequence());\n    }",
            "IEnumerator GameLoop()\n    {\n        yield return StartCoroutine(IntroDialogue());\n        for (_idx=0;_idx<planets.Length;_idx++) yield return StartCoroutine(RunPlanet());\n        yield return StartCoroutine(EndingSequence());\n    }");

        // ── 4. Insert IntroDialogue method before EndingSequence ─────────────
        s = s.Replace(
            "IEnumerator EndingSequence()",
@"IEnumerator IntroDialogue()
    {
        // Pre-combat briefing: Ashtar, Ptaah, and the Lizard representative
        yield return StartCoroutine(FadeTo(0f, 1f, 0.5f));
        if (bgRenderer != null) bgRenderer.color = new Color(0.03f, 0.05f, 0.12f);
        yield return StartCoroutine(FadeTo(1f, 0f, 0.8f));
        yield return StartCoroutine(ShowDlg(""PTAAH: Commander Ashtar. Our long-range sensors have identified eight candidate worlds within reach. All show signs of potential habitability for the human species."", 0f));
        yield return StartCoroutine(ShowDlg(""ASHTAR: Understood, Ptaah. Earth's people need a new home. We will not fail them."", 0f));
        yield return StartCoroutine(ShowDlg(""LIZARD: You will not find what you seek, Sheran. These worlds are ours to control. Your mission ends here."", 0f));
        yield return StartCoroutine(ShowDlg(""ASHTAR: Then we will meet you at every world, and we will push through every one of them."", 0f));
        yield return StartCoroutine(ShowDlg(""PTAAH: Defensive formations incoming. Weapons hot. Commander — begin the approach."", 0f));
        if (_dialogueBg != null) _dialogueBg.gameObject.SetActive(false);
        if (_speakerIcon != null) _speakerIcon.enabled = false;
        if (_dialogueTxt != null) _dialogueTxt.text = """";
    }

    IEnumerator EndingSequence()");

        // ── 5. Larger dialogue panel: height 90 → 200 ────────────────────────
        s = s.Replace(
            "dr.sizeDelta=new Vector2(0f,90f);",
            "dr.sizeDelta=new Vector2(0f,200f);");

        // ── 6. Larger speaker icon: width ~80 → ~200 (offsetMax.x 90 → 210) ──
        s = s.Replace(
            "ir.offsetMin=new Vector2(10f,5f);ir.offsetMax=new Vector2(90f,-5f);",
            "ir.offsetMin=new Vector2(10f,5f);ir.offsetMax=new Vector2(210f,-5f);");

        // ── 7. Dialogue text: move right to clear bigger icon (105 → 225) ─────
        s = s.Replace(
            "dtr.offsetMin=new Vector2(105f,8f);dtr.offsetMax=new Vector2(-20f,-8f);",
            "dtr.offsetMin=new Vector2(225f,8f);dtr.offsetMax=new Vector2(-20f,-8f);");

        // ── 8. Dialogue text font size 22 → 44 ───────────────────────────────
        s = s.Replace(
            "_dialogueTxt.font=f;_dialogueTxt.fontSize=22;",
            "_dialogueTxt.font=f;_dialogueTxt.fontSize=44;");

        File.WriteAllText(p, s);
        AssetDatabase.Refresh();
        Debug.Log("[PatchDialogue] Done. Assign lizardIconSprite in PlanetEncounterManager Inspector.");
    }
}
#endif
