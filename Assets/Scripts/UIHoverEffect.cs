using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Graphic backgroundGraphic;
    [SerializeField] private TMP_Text textGraphic;
    [SerializeField] private float hoverAlpha = 0.08f;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color hoverTextColor = Color.black;
    [SerializeField] private float fadeDuration = 0.15f;

    private Color baseBackgroundColor;
    private Color baseTextColor;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (backgroundGraphic == null)
            backgroundGraphic = GetComponent<Graphic>();

        if (textGraphic == null)
            textGraphic = GetComponentInChildren<TMP_Text>(true);

        if (backgroundGraphic != null)
            baseBackgroundColor = backgroundGraphic.color;

        if (textGraphic != null)
            baseTextColor = textGraphic.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartFade(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(false);
    }

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
        targetBg.a = hover ? hoverAlpha : baseBackgroundColor.a;

        Color startText = textGraphic != null ? textGraphic.color : Color.white;
        Color targetText = hover ? hoverTextColor : baseTextColor;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;

            if (backgroundGraphic != null)
                backgroundGraphic.color = Color.Lerp(startBg, targetBg, t);

            if (textGraphic != null)
                textGraphic.color = Color.Lerp(startText, targetText, t);

            yield return null;
        }

        if (backgroundGraphic != null)
            backgroundGraphic.color = targetBg;

        if (textGraphic != null)
            textGraphic.color = targetText;

        fadeRoutine = null;
    }
}