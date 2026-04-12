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
    public float typeSpeed = 0.05f; // Speed of typewriter effect
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    public AudioSource sfxSource;   // Optional: Add a small "blip" sound per character
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

        StartCoroutine(PlaySlideshow());
    }

    private void Update()
    {
        // Allow player to skip the typewriter or the hold duration
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            _isSkipped = true;
        }
    }

    private IEnumerator PlaySlideshow()
    {
        foreach (var slide in slides)
        {
            _isSkipped = false;
            
            // Setup slide
            if (slideImage != null) {
                slideImage.sprite = slide.image;
                // Fade image in quickly or keep it static
                StartCoroutine(FadeImage(1f, 0.5f)); 
            }

            // Start Typewriter
            yield return StartCoroutine(TypeText(slide.text));

            // Wait for duration OR player click
            float timer = 0;
            while (timer < slide.holdDuration && !_isSkipped)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Fade out text and image before next slide
            yield return StartCoroutine(FadeAll(0f, 0.5f));
        }

        SceneManager.LoadScene(1);
    }

    private IEnumerator TypeText(string fullText)
    {
        slideText.text = "";
        // Ensure text is visible (Alpha 1)
        Color c = slideText.color;
        c.a = 1f;
        slideText.color = c;

        foreach (char letter in fullText.ToCharArray())
        {
            if (_isSkipped) 
            {
                slideText.text = fullText; // Finish text immediately
                _isSkipped = false; // Reset skip for the "Hold" phase
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