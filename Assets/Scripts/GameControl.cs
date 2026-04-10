using System.Collections;
using UnityEngine;

namespace Plane.UI
{
    public class GameControl : MonoBehaviour
    {
        public static GameControl m_Current;

        public const int State_Start = 0;
        public const int State_Chase = 1;
        public const int State_Shoot = 2;
        public const int State_Win = 3;
        public const int State_Lose = 4;

        [SerializeField] private GameObject speedParticle;
        [SerializeField] private float gameSpeed = 18f;
        [SerializeField] private float baseGameSpeed = 18f;
        [SerializeField] private bool enableDebugLogs = true;

        [SerializeField] private float[] levelTargets = new float[] { 20f, 40f, 60f };

        [HideInInspector] public int m_GameState = State_Start;
        [HideInInspector] public float State_Timer = 0f;

        public float m_GameSpeed => gameSpeed;
        public int CurrentStageIndex => currentTargetIndex;

        private bool gameEnded;
        private int currentTargetIndex = 0;
        public bool LevelUpTriggered;

        private Coroutine speedBoostRoutine;
        private float speedBoostOriginal;

        private Coroutine shootRoutine;

        private void Awake()
        {
            m_Current = this;
            Application.runInBackground = true;
        }

        private void Start()
        {
            ResetGameState();
        }

        private void Update()
        {
            if (gameEnded)
                return;

            State_Timer += Time.deltaTime;

            if (m_GameState == State_Start && State_Timer > 1f)
            {
                m_GameState = State_Chase;
                State_Timer = 0f;

                if (enableDebugLogs)
                    Debug.Log($"Chase started - stage {currentTargetIndex + 1}");
            }

            if (m_GameState != State_Chase)
                return;

            if (!LevelUpTriggered && currentTargetIndex < levelTargets.Length)
            {
                if (State_Timer >= levelTargets[currentTargetIndex])
                {
                    LevelUpTriggered = true;

                    if (UIControl.Current != null)
                        UIControl.Current.ShowLevelUp();
                }
            }
        }

        public void HandleGameOver()
        {
            if (gameEnded)
                return;

            gameEnded = true;
            m_GameState = State_Lose;
            gameSpeed = 0f;

            if (speedParticle != null)
                speedParticle.SetActive(false);

            if (UIControl.Current != null)
                UIControl.Current.ShowResult(false);
        }

        public void HandleWin()
        {
            if (gameEnded)
                return;

            gameEnded = true;
            m_GameState = State_Win;
            gameSpeed = 0f;

            if (speedParticle != null)
                speedParticle.SetActive(false);

            if (UIControl.Current != null)
                UIControl.Current.ShowResult(true);
        }

        public void RestartRun()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        public void StartSpeedBoost(float multiplier, float duration)
        {
            if (speedBoostRoutine != null)
                StopCoroutine(speedBoostRoutine);

            speedBoostOriginal = baseGameSpeed;
            gameSpeed = baseGameSpeed * multiplier;

            speedBoostRoutine = StartCoroutine(SpeedBoostRoutine(duration));
        }

        private IEnumerator SpeedBoostRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            gameSpeed = baseGameSpeed;
            speedBoostRoutine = null;
        }

        public void ActivateShootMode(float duration)
        {
            if (shootRoutine != null)
                StopCoroutine(shootRoutine);

            shootRoutine = StartCoroutine(ShootRoutine(duration));
        }

        private IEnumerator ShootRoutine(float duration)
        {
            int previousState = m_GameState;
            m_GameState = State_Shoot;

            yield return new WaitForSeconds(duration);

            m_GameState = previousState == State_Chase ? State_Chase : State_Chase;
            shootRoutine = null;
        }

        public void IncreaseSpeed(float multiplier)
        {
            gameSpeed *= multiplier;
        }

        public void ResetSpeed()
        {
            gameSpeed = baseGameSpeed;
        }

        public float GetCurrentTarget()
        {
            if (levelTargets == null || levelTargets.Length == 0)
                return 0f;

            int index = Mathf.Clamp(currentTargetIndex, 0, levelTargets.Length - 1);
            return levelTargets[index];
        }

        public void CompleteLevelUp()
        {
            LevelUpTriggered = false;
            currentTargetIndex++;

            if (currentTargetIndex >= levelTargets.Length)
            {
                HandleWin();
                return;
            }

            State_Timer = 0f;
            IncreaseSpeed(1.2f);

            if (UIControl.Current != null)
                UIControl.Current.HideLevelUp();
        }

        private void ResetGameState()
        {
            if (speedBoostRoutine != null)
            {
                StopCoroutine(speedBoostRoutine);
                speedBoostRoutine = null;
            }

            if (shootRoutine != null)
            {
                StopCoroutine(shootRoutine);
                shootRoutine = null;
            }

            gameEnded = false;
            m_GameState = State_Start;
            State_Timer = 0f;
            gameSpeed = baseGameSpeed;
            currentTargetIndex = 0;
            LevelUpTriggered = false;

            if (speedParticle != null)
                speedParticle.SetActive(true);
        }
    }
}