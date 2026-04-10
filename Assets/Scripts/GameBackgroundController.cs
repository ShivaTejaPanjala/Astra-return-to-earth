using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private TMP_Text devTitleText;
    [SerializeField] private TMP_Text devDetailsText;
    [SerializeField] private float letterDelay = 0.04f;
    [SerializeField] private float lineDelay = 0.5f;
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color subtitleColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color devColor = new Color(0.8f, 0.9f, 1f, 1f);

    [SerializeField] private Outline titleOutline;
    [SerializeField] private Outline devOutline;
    [SerializeField] private float glowPulseSpeed = 2f;

    private bool isAnimating;
    private float currentRotation;
    private float scrollOffset;
    private Coroutine titleRoutine;
    private Material runtimeSkybox;

    private void Awake()
    {
        if (skyboxMaterial != null)
            runtimeSkybox = new Material(skyboxMaterial);

        if (titleText != null) titleText.text = "";
        if (subtitleText != null) subtitleText.text = "";
        if (devTitleText != null) devTitleText.text = "";
        if (devDetailsText != null) devDetailsText.text = "";
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

        if (titleRoutine != null)
            StopCoroutine(titleRoutine);

        titleRoutine = StartCoroutine(PlayTitleSequence());
    }

    private void Update()
    {
        if (!isAnimating)
            return;

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

        if (titleOutline != null && titleText != null && titleText.gameObject.activeInHierarchy)
        {
            float t = (Mathf.Sin(Time.unscaledTime * glowPulseSpeed) + 1f) * 0.5f;
            titleOutline.effectDistance = Vector2.Lerp(new Vector2(1f, 1f), new Vector2(4f, 4f), t);
        }

        if (devOutline != null && devTitleText != null && devTitleText.gameObject.activeInHierarchy)
        {
            float t = (Mathf.Sin(Time.unscaledTime * (glowPulseSpeed * 0.9f)) + 1f) * 0.5f;
            devOutline.effectDistance = Vector2.Lerp(new Vector2(0.5f, 0.5f), new Vector2(2f, 2f), t);
        }
    }

    private IEnumerator PlayTitleSequence()
    {
        yield return TypeText(titleText, "Astra - Escape to Earth", titleColor, letterDelay);
        yield return new WaitForSecondsRealtime(lineDelay);
        yield return TypeText(subtitleText, "Developed by Shiva Teja Panjala", subtitleColor, letterDelay);
        yield return new WaitForSecondsRealtime(lineDelay);
        yield return TypeText(devTitleText, "Developer", devColor, letterDelay);
        yield return new WaitForSecondsRealtime(lineDelay);
        yield return TypeText(devDetailsText, "Shiva Teja Panjala", new Color(0.8f, 0.85f, 1f, 1f), letterDelay);
    }

    private IEnumerator TypeText(TMP_Text txt, string value, Color color, float delay)
    {
        if (txt == null)
            yield break;

        txt.text = value;
        txt.color = color;
        txt.ForceMeshUpdate();
        txt.maxVisibleCharacters = 0;

        int totalCharacters = txt.textInfo.characterCount;
        for (int i = 0; i <= totalCharacters; i++)
        {
            txt.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(delay);
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