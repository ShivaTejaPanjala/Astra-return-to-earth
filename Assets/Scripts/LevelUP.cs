using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Plane.UI
{
    public class LevelUP : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelMessageText;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip levelUpSfx;
        [SerializeField] private string menuSceneName = "Menu";
        [SerializeField] private Material levelSkyboxMaterial;

        [SerializeField] private float showDuration = 5f;
        [SerializeField] private float blinkInterval = 0.35f;
        [SerializeField] private int[] levelTargets = new int[] { 200, 500, 1000 };
        [SerializeField] private float speedMultiplier = 2f;

        [HideInInspector] public bool IsLevelUpActive;
        [HideInInspector] public int CurrentLevelIndex = 0;
        [HideInInspector] public int CurrentTarget = 200;

        private Coroutine routine;
        private Coroutine blinkRoutine;

        private void OnEnable()
        {
            PlayLevelUpSequence();
        }

        private void OnDisable()
        {
            if (audioSource != null)
                audioSource.Stop();

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }

            IsLevelUpActive = false;
        }

        public bool IsLastLevel
        {
            get { return CurrentLevelIndex >= levelTargets.Length - 1; }
        }

        public void TriggerLevelUp()
        {
            if (CurrentLevelIndex < levelTargets.Length - 1)
                CurrentLevelIndex++;

            CurrentTarget = levelTargets[CurrentLevelIndex];

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            else
                PlayLevelUpSequence();
        }

        private void PlayLevelUpSequence()
        {
            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(LevelUpRoutine());
        }

        private IEnumerator LevelUpRoutine()
        {
            IsLevelUpActive = true;

            if (UIControl.Current != null)
                UIControl.Current.ShowLevelUp();

            if (levelSkyboxMaterial != null)
                RenderSettings.skybox = levelSkyboxMaterial;

            if (levelMessageText != null)
            {
                levelMessageText.text = "Congrats!";
                levelMessageText.color = Color.white;
                levelMessageText.enabled = true;
            }

            if (targetText != null)
                targetText.text = "Target: " + CurrentTarget;

            if (audioSource != null && levelUpSfx != null)
            {
                audioSource.loop = false;
                audioSource.clip = levelUpSfx;
                audioSource.Play();
            }

            if (GameControl.m_Current != null)
                GameControl.m_Current.IncreaseSpeed(speedMultiplier);

            if (blinkRoutine != null)
                StopCoroutine(blinkRoutine);

            blinkRoutine = StartCoroutine(BlinkTextRoutine());

            yield return new WaitForSecondsRealtime(showDuration);

            if (levelMessageText != null)
            {
                levelMessageText.text = "You have level up!";
                levelMessageText.color = Color.white;
                levelMessageText.enabled = true;
            }

            yield return new WaitForSecondsRealtime(0.8f);

            if (UIControl.Current != null)
                UIControl.Current.HideLevelUp();

            IsLevelUpActive = false;
            routine = null;
        }

        private IEnumerator BlinkTextRoutine()
        {
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < showDuration)
            {
                if (levelMessageText != null)
                    levelMessageText.enabled = visible;

                visible = !visible;
                yield return new WaitForSecondsRealtime(blinkInterval);
                elapsed += blinkInterval;
            }

            if (levelMessageText != null)
                levelMessageText.enabled = true;
        }

        private void PlayButtonClick()
        {
            if (audioSource == null)
                return;

            if (levelUpSfx != null)
                audioSource.PlayOneShot(levelUpSfx);
        }

        public void BtnRestart()
        {
            PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void BtnExit()
        {
            PlayButtonClick();
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }
    }
}