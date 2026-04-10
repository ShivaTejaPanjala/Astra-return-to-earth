using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Plane.UI
{
    [System.Serializable]
    public class PlayerScoreData
    {
        public string playerName;
        public int highScore;
    }

    public class GameUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private string playerNameKey = "PlayerName";

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickSfx;
        [SerializeField] private AudioClip backgroundSfx;
        [SerializeField] private AudioClip moveSfx;

        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;

        [SerializeField] private float scorePerSecond = 1f;
        [SerializeField] private bool enableDebugLogs = true;

        [SerializeField] private float normalAlpha = 0f;
        [SerializeField] private float hoverAlpha = 0.2f;
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color hoverTextColor = Color.black;
        [SerializeField] private float fadeDuration = 0.15f;

        public static GameUI Current;

        private Graphic backgroundGraphic;
        private TMP_Text buttonText;
        private Color baseBackgroundColor;
        private Coroutine fadeRoutine;
        private float travelTime;
        private int currentScore;
        private int currentHighScore;
        private int bonusScore;
        private int previousScore;
        private int previousHighScore;

        public int CurrentScore => currentScore;
        public int CurrentHighScore => currentHighScore;
        public int BonusScore => bonusScore;

        private void Awake()
        {
            Current = this;
            backgroundGraphic = GetComponent<Graphic>();
            buttonText = GetComponentInChildren<TMP_Text>(true);

            if (backgroundGraphic != null)
            {
                baseBackgroundColor = backgroundGraphic.color;
                baseBackgroundColor.a = normalAlpha;
                backgroundGraphic.color = baseBackgroundColor;
            }

            if (buttonText != null)
                buttonText.color = normalTextColor;
        }

        private void OnEnable()
        {
            travelTime = 0f;
            bonusScore = 0;
            currentScore = 0;
            currentHighScore = LoadHighScore();
            previousScore = -1;
            previousHighScore = -1;
            UpdateScoreUI();
            PlayBackgroundMusic();
        }

        private void OnDisable()
        {
            StopBackgroundMusic();
        }

        private void Update()
        {
            if (GameControl.m_Current == null)
                return;

            if (GameControl.m_Current.LevelUpTriggered)
                return;

            if (GameControl.m_Current.m_GameState == GameControl.State_Chase)
            {
                travelTime += Time.deltaTime;
                currentScore = Mathf.FloorToInt(travelTime * scorePerSecond) + bonusScore;

                if (currentScore > currentHighScore)
                {
                    currentHighScore = currentScore;
                    SaveHighScore(currentHighScore);
                }
            }

            if (currentScore != previousScore || currentHighScore != previousHighScore)
            {
                if (enableDebugLogs && currentScore != previousScore)
                {
                    var stageIndex = GameControl.m_Current != null ? GameControl.m_Current.CurrentStageIndex + 1 : 0;
                    Debug.Log($"[GameUI] Stage={stageIndex} Score={currentScore} Timer={travelTime:F2} Bonus={bonusScore} High={currentHighScore}");
                }

                UpdateScoreUI();
                previousScore = currentScore;
                previousHighScore = currentHighScore;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => StartFade(true);
        public void OnPointerExit(PointerEventData eventData) => StartFade(false);

        private void StartFade(bool hover)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeRoutine(hover));
        }

        private IEnumerator FadeRoutine(bool hover)
        {
            Color startBg = backgroundGraphic != null ? backgroundGraphic.color : Color.white;
            Color targetBg = baseBackgroundColor;
            targetBg.a = hover ? hoverAlpha : normalAlpha;

            Color startText = buttonText != null ? buttonText.color : Color.white;
            Color targetText = hover ? hoverTextColor : normalTextColor;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeDuration;

                if (backgroundGraphic != null)
                    backgroundGraphic.color = Color.Lerp(startBg, targetBg, t);

                if (buttonText != null)
                    buttonText.color = Color.Lerp(startText, targetText, t);

                yield return null;
            }

            if (backgroundGraphic != null)
                backgroundGraphic.color = targetBg;

            if (buttonText != null)
                buttonText.color = targetText;

            fadeRoutine = null;
        }

        private void PlayClickSfx()
        {
            if (clickSfx == null)
                return;

            if (audioSource != null)
                audioSource.PlayOneShot(clickSfx);
            else if (Camera.main != null)
                AudioSource.PlayClipAtPoint(clickSfx, Camera.main.transform.position);
        }

        private void PlayBackgroundMusic()
        {
            if (audioSource == null || backgroundSfx == null)
                return;

            audioSource.loop = true;
            audioSource.clip = backgroundSfx;
            audioSource.Play();
        }

        private void StopBackgroundMusic()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
                scoreText.SetText("SCORE: {0}", currentScore);

            string playerName = LoadPlayerName();

            if (highScoreText != null)
                highScoreText.text = string.Format("{0} | BEST: {1}", playerName.ToUpper(), currentHighScore);
        }

        public void ResetScore()
        {
            travelTime = 0f;
            bonusScore = 0;
            currentScore = 0;
            currentHighScore = 0;

            SaveHighScore(0);
            UpdateScoreUI();
        }

        public void AddBonusScore(int bonus)
        {
            bonusScore += bonus;
            currentScore += bonus;
            UpdateScoreUI();
        }

        public void BtnRestart()
        {
            PlayClickSfx();
            ResetScore();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void BtnExit()
        {
            PlayClickSfx();
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        public void PlayMoveSfx()
        {
            if (moveSfx == null)
                return;

            if (audioSource != null)
                audioSource.PlayOneShot(moveSfx);
        }

        private string GetScoreDataPath()
        {
            string playerName = PlayerPrefs.GetString(playerNameKey, "Player");
            return Path.Combine(Application.persistentDataPath, $"playerScore_{playerName}.json");
        }

        private void SaveHighScore(int score)
        {
            string playerName = PlayerPrefs.GetString(playerNameKey, "Player");
            PlayerScoreData data = new PlayerScoreData
            {
                playerName = playerName,
                highScore = score
            };

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(GetScoreDataPath(), json);

            if (enableDebugLogs)
                Debug.Log($"[GameUI] Saved high score: {score} for {playerName}");
        }

        private int LoadHighScore()
        {
            string path = GetScoreDataPath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                PlayerScoreData data = JsonUtility.FromJson<PlayerScoreData>(json);
                if (enableDebugLogs)
                    Debug.Log($"[GameUI] Loaded high score: {data.highScore} for {data.playerName}");
                return data.highScore;
            }
            return 0; 
        }

        private string LoadPlayerName()
        {
            return PlayerPrefs.GetString(playerNameKey, "Player");
        }
    }
}