using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Plane.UI
{
    public class LoseUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip buttonClickSfx;
        [SerializeField] private string menuSceneName = "Menu";

        private void OnEnable()
        {
            UpdateTexts();

            if (audioSource != null)
            {
                audioSource.loop = true;
                audioSource.Play();
            }

            if (messageText != null)
                messageText.text = "You Lose!";
        }

        private void OnDisable()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        private void UpdateTexts()
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
        }

        private void PlayClick()
        {
            if (buttonClickSfx == null)
                return;

            if (audioSource != null)
                audioSource.PlayOneShot(buttonClickSfx);
        }

        public void BtnRestart()
        {
            PlayClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void BtnExit()
        {
            PlayClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }
    }
}