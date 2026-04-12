using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LivesManager : MonoBehaviour
{
    [Header("Lives")]
    public int lives = 3;
    public Image heart1, heart2, heart3;
    [Header("UI")]
    public GameObject gameOverPanel;
    public System.Action onAllLivesLost;
    Image[] _hearts;

    static void FindHearts(ref Image h1, ref Image h2, ref Image h3)
    {
        var ld = GameObject.Find("LivesDisplay");
        if (ld == null) return;
        if (h1 == null) { var t = ld.transform.Find("Heart1"); if (t) h1 = t.GetComponent<Image>(); }
        if (h2 == null) { var t = ld.transform.Find("Heart2"); if (t) h2 = t.GetComponent<Image>(); }
        if (h3 == null) { var t = ld.transform.Find("Heart3"); if (t) h3 = t.GetComponent<Image>(); }
    }

    void Awake() { FindHearts(ref heart1, ref heart2, ref heart3); _hearts = new Image[]{heart1,heart2,heart3}; }

    void Start()
    {
        FindHearts(ref heart1, ref heart2, ref heart3);
        _hearts = new Image[]{heart1,heart2,heart3};
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
        ResetLives();
        if (gameOverPanel == null) return;
        var btn = gameOverPanel.GetComponentInChildren<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { ResetLives(); SceneManager.LoadScene(1); });
    }

    public void LoseLife()
    {
        if (lives <= 0) return;
        lives--;
        for (int i = _hearts.Length - 1; i >= 0; i--)
            if (_hearts[i] != null && _hearts[i].gameObject.activeSelf) { _hearts[i].gameObject.SetActive(false); break; }
        if (lives <= 0) OnGameOver();
    }

    public void OnGameOver()
    {
        if (onAllLivesLost != null) onAllLivesLost.Invoke();
        else if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RefreshHearts()
    {
        if (_hearts == null) _hearts = new Image[]{heart1,heart2,heart3};
        for (int i = 0; i < _hearts.Length; i++)
            if (_hearts[i] != null) _hearts[i].gameObject.SetActive(i < lives);
    }

    public void ResetLives()
    {
        lives = 3;
        if (_hearts == null) _hearts = new Image[]{heart1,heart2,heart3};
        foreach (var h in _hearts) if (h != null) h.gameObject.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}
