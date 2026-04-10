using UnityEngine;
using TMPro;
using System.Collections;

namespace Plane.UI
{
    public class WinUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private ParticleSystem celebrateEffect;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickSfx;
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private float restartDelay = 0.2f;

        private void OnEnable()
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            UpdateUI();

            if (celebrateEffect != null)
                celebrateEffect.Play();
        }

        private void OnDisable()
        {
            if (celebrateEffect != null)
                celebrateEffect.Stop();
        }

        private void UpdateUI()
        {
            int score = 0;
            if (GameUI.Current != null)
                score = GameUI.Current.CurrentScore;
            else if (GameControl.m_Current != null)
                score = Mathf.RoundToInt(GameControl.m_Current.State_Timer);

            if (scoreText != null)
                scoreText.SetText("SCORE: {0}", score);

            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.SetString("HighScorePlayer", playerName);
                PlayerPrefs.Save();
            }

            if (highScoreText != null)
                highScoreText.text = string.Format("{0} | BEST: {1}", PlayerPrefs.GetString("HighScorePlayer", playerName), highScore);

            if (messageText != null)
                messageText.text = "YOU WIN! CONGRATULATIONS";
        }

        public void BtnRestart()
        {
            RestartGame();
        }

        public void BtnExit()
        {
            QuitToMenu();
        }

        private void PlayClick()
        {
            if (buttonClickSfx == null)
                return;

            if (audioSource != null)
                audioSource.PlayOneShot(buttonClickSfx);
        }

        public void RestartGame()
        {
            PlayClick();
            Time.timeScale = 1f;
            PlayerPrefs.SetInt("PlayerScore", 0);
            PlayerPrefs.Save();
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            yield return new WaitForSeconds(restartDelay);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void QuitToMenu()
        {
            PlayClick();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
        }
    }
}