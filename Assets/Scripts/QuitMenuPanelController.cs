using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitMenuPanelController : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip clickSound;
    public AudioClip enterSound;

    [Header("Scene Names")]
    public string gameSceneName = "Game";
    public string menuSceneName = "Menu";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            RestartGame();
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    public void RestartGame()
    {
        PlaySFX(enterSound);
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitToMainMenu()
    {
        PlaySFX(clickSound);
        SceneManager.LoadScene(menuSceneName);
    }
}