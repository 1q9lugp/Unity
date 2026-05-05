using UnityEngine;
using UnityEditor;

public static class Act3LightingSetup
{
    [MenuItem("Tools/Apply Act3 Eclipse Lighting")]
    public static void Apply()
    {
        // Remove skybox material (solid color background)
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Near-black red tint background reflected in ambient
        // #1A0500
        RenderSettings.ambientLight = new Color(0.102f, 0.020f, 0.000f, 1f);

        // Fog off
        RenderSettings.fog = false;

        // Camera background handled per-camera; set scene background color via Camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            // #0D0000
            cam.backgroundColor = new Color(0.051f, 0.000f, 0.000f, 1f);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("Act3 Eclipse lighting applied.");
    }
}
