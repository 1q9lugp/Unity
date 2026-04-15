using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public AudioClip menuMusic;
    void Start()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        if (menuMusic != null) { src.clip = menuMusic; src.Play(); }
    }
    public void StartGame() { SceneManager.LoadScene(1); }
}
