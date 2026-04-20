using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Act2Controller : MonoBehaviour
{
    public AudioClip music;

    void Start()
    {
        if (music != null)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.clip = music; a.loop = true; a.volume = 0.7f; a.Play();
        }
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene(4);
    }
}
