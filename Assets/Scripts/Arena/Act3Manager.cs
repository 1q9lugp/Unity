using UnityEngine;
using UnityEngine.Rendering;

public class Act3Manager : MonoBehaviour
{
    void Awake()
    {
        // Skybox
        var sky = new Material(Shader.Find("Skybox/Procedural"));
        if (sky != null)
        {
            sky.SetFloat("_SunSize",             0.02f);
            sky.SetFloat("_AtmosphereThickness", 0.25f);
            sky.SetColor("_SkyTint",    new Color(0.28f, 0.02f, 0.02f));
            sky.SetColor("_GroundColor",new Color(0.12f, 0.03f, 0.01f));
            sky.SetFloat("_Exposure",   0.6f);
            RenderSettings.skybox = sky;
            DynamicGI.UpdateEnvironment();
        }

        // Ambient lighting
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.25f, 0.05f, 0.02f);
        var sun = FindObjectOfType<Light>();
        if (sun != null)
        {
            sun.color     = new Color(1f, 0.35f, 0.1f);
            sun.intensity = 0.8f;
            sun.transform.eulerAngles = new Vector3(45f, 60f, 0f);
        }

        // Fog
        RenderSettings.fog              = true;
        RenderSettings.fogMode          = FogMode.Linear;
        RenderSettings.fogColor         = new Color(0.18f, 0.03f, 0.01f);
        RenderSettings.fogStartDistance = 60f;
        RenderSettings.fogEndDistance   = 200f;
    }
}
