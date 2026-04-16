using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    // Called by the Start Game button onClick in the Inspector
    public void StartGame()
    {
        SceneManager.LoadScene("Act0_Briefing");;
    }
}
