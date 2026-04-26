using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlideshowController : MonoBehaviour
{
    [System.Serializable]
    public struct Slide
    {
        public Sprite image;
        [TextArea(3, 10)]
        public string text;
        public float holdDuration;
    }

    public List<Slide> slides;
    public Image slideImage;
    public Text slideText;

    [Header("Game Feel Settings")]
    public float typeSpeed = 0.05f;
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    public AudioSource sfxSource;
    public AudioClip typeSound;

    private bool _isSkipped = false;

    private void Start()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        SetupHighContrastText();
        StartCoroutine(PlaySlideshow());
    }

    private void SetupHighContrastText()
    {
        if (slideText == null) return;

        slideText.color = Color.white;
        slideText.fontStyle = FontStyle.Bold;

        // 1. Add a thick Outline (Border)
        var ol = slideText.gameObject.GetComponent<Outline>() ?? slideText.gameObject.AddComponent<Outline>();
        ol.effectColor = new Color(0f, 0f, 0f, 1f); // Pure black
        ol.effectDistance = new Vector2(2f, -2f); // Thicker border

        // 2. Add a Shadow (Depth)
        // Adding a shadow on top of an outline creates a "lifted" look that handles white backgrounds perfectly
        var sh = slideText.gameObject.GetComponent<Shadow>() ?? slideText.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.8f);
        sh.effectDistance = new Vector2(3f, -3f);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            _isSkipped = true;
    }

    private IEnumerator PlaySlideshow()
    {
        foreach (var slide in slides)
        {
            _isSkipped = false;

            if (slideImage != null)
            {
                slideImage.sprite = slide.image;
                // Ensure image alpha is reset before fading in
                Color c = slideImage.color;
                c.a = 0f;
                slideImage.color = c;
                StartCoroutine(FadeImage(1f, 0.5f));
            }

            yield return StartCoroutine(TypeText(slide.text));

            float timer = 0;
            while (timer < slide.holdDuration && !_isSkipped)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            yield return StartCoroutine(FadeAll(0f, 0.5f));
        }

        SceneManager.LoadScene("Act1_Space");
    }

    private IEnumerator TypeText(string fullText)
    {
        slideText.text = "";
        Color c = slideText.color;
        c.a = 1f;
        slideText.color = c;

        foreach (char letter in fullText.ToCharArray())
        {
            if (_isSkipped)
            {
                slideText.text = fullText;
                _isSkipped = false;
                yield break;
            }

            slideText.text += letter;

            if (sfxSource != null && typeSound != null)
                sfxSource.PlayOneShot(typeSound);

            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator FadeImage(float targetAlpha, float duration)
    {
        float startAlpha = slideImage.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = slideImage.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            slideImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeAll(float targetAlpha, float duration)
    {
        float startAlphaTxt = slideText.color.a;
        float startAlphaImg = slideImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Color ct = slideText.color;
            ct.a = Mathf.Lerp(startAlphaTxt, targetAlpha, t);
            slideText.color = ct;

            Color ci = slideImage.color;
            ci.a = Mathf.Lerp(startAlphaImg, targetAlpha, t);
            slideImage.color = ci;

            yield return null;
        }
    }
}