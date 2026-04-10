using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class SpaceBackgroundController : MonoBehaviour
    {
        [Header("Skybox Settings")]
        public Material skyboxMaterial;
        public float skyboxRotationSpeed = 4f;
        public float skyboxScrollSpeed = 1.5f;
        public bool useDepthParallax = true;
        public float depthScaleMultiplier = 1.0f;

        [Header("Background Music")]
        public AudioSource backgroundMusic;

        [Header("Global Control")]
        public float globalSpeedMultiplier = 1f;
        public bool animateOnStart = true;

        [Header("Title UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text devText;
        [SerializeField] private float letterDelay = 0.04f;
        [SerializeField] private float subtitleDelay = 0.5f;
        [SerializeField] private float glowPulseSpeed = 2f;
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color glowColor = new Color(1f, 0.95f, 0.7f, 1f);

        private bool isAnimating = false;
        private float currentRotation = 0f;
        private float scrollOffset = 0f;
        private Coroutine titleRoutine;
        private Coroutine glowRoutine;
        private Material runtimeSkybox;

        private void Awake()
        {
            if (skyboxMaterial != null)
                runtimeSkybox = new Material(skyboxMaterial);
        }

        private void Start()
        {
            isAnimating = animateOnStart;

            if (runtimeSkybox != null)
                RenderSettings.skybox = runtimeSkybox;

            if (backgroundMusic != null && !backgroundMusic.isPlaying)
            {
                backgroundMusic.loop = true;
                backgroundMusic.Play();
            }

            SetInitialTextState();

            if (titleRoutine != null)
                StopCoroutine(titleRoutine);

            titleRoutine = StartCoroutine(PlayTitleSequence());
        }

        private void SetInitialTextState()
        {
            if (titleText != null)
            {
                titleText.text = "Astra";
                titleText.maxVisibleCharacters = 0;
                titleText.outlineWidth = 0f;
            }

            if (subtitleText != null)
            {
                subtitleText.text = "Escape to Earth";
                subtitleText.maxVisibleCharacters = 0;
                subtitleText.outlineWidth = 0f;
            }

            if (devText != null)
            {
                devText.text = "Developed by Shiva Teja Panjala";
                devText.maxVisibleCharacters = 0;
                devText.outlineWidth = 0f;
            }
        }

        private void Update()
        {
            if (!isAnimating) return;

            if (runtimeSkybox != null)
            {
                currentRotation += skyboxRotationSpeed * globalSpeedMultiplier * Time.deltaTime;
                runtimeSkybox.SetFloat("_Rotation", currentRotation);

                scrollOffset += skyboxScrollSpeed * globalSpeedMultiplier * Time.deltaTime;
                runtimeSkybox.SetFloat("_Offset", scrollOffset);

                if (useDepthParallax)
                {
                    float depthFactor = globalSpeedMultiplier * depthScaleMultiplier;
                    runtimeSkybox.SetFloat("_Scale", 1f + depthFactor * 0.15f);
                }
            }
        }

        private IEnumerator PlayTitleSequence()
        {
            yield return AnimateText(titleText, "Astra", letterDelay);

            if (titleText != null)
            {
                titleText.outlineWidth = 0.2f;
                titleText.outlineColor = glowColor;
            }

            yield return new WaitForSecondsRealtime(subtitleDelay);

            yield return AnimateText(subtitleText, "Escape to Earth", letterDelay);

            if (subtitleText != null)
            {
                subtitleText.outlineWidth = 0.2f;
                subtitleText.outlineColor = glowColor;
            }

            yield return new WaitForSecondsRealtime(subtitleDelay * 0.5f);

            yield return AnimateText(devText, "Developed by Shiva Teja Panjala", letterDelay);

            if (glowRoutine != null)
                StopCoroutine(glowRoutine);

            glowRoutine = StartCoroutine(GlowPulseRoutine());
        }

        private IEnumerator AnimateText(TMP_Text text, string value, float delay)
        {
            if (text == null) yield break;

            text.text = value;
            text.color = titleColor;
            text.ForceMeshUpdate();

            int totalCharacters = text.textInfo.characterCount;
            text.maxVisibleCharacters = 0;

            for (int i = 0; i <= totalCharacters; i++)
            {
                text.maxVisibleCharacters = i;
                yield return new WaitForSecondsRealtime(delay);
            }
        }

        private IEnumerator GlowPulseRoutine()
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.unscaledTime * glowPulseSpeed) + 1f) * 0.5f;

                if (titleText != null)
                    titleText.outlineWidth = Mathf.Lerp(0.1f, 0.5f, t);

                if (subtitleText != null)
                    subtitleText.outlineWidth = Mathf.Lerp(0.1f, 0.5f, t);

                yield return null;
            }
        }

        public void SetSpeedMultiplier(float value)
        {
            globalSpeedMultiplier = value;
        }

        public void SetAnimation(bool state)
        {
            isAnimating = state;
        }

        public void StopMusic()
        {
            if (backgroundMusic != null && backgroundMusic.isPlaying)
                backgroundMusic.Stop();
        }

        public void StartMusic()
        {
            if (backgroundMusic != null && !backgroundMusic.isPlaying)
            {
                backgroundMusic.loop = true;
                backgroundMusic.Play();
            }
        }
    }
}