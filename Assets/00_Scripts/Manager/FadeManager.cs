using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage;

    private Coroutine fadeCor = null;

    private void Start()
    {
        fadeImage.gameObject.SetActive(false);
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void FadeIn(float time, Action callback = null)
    {
        if (fadeCor != null) StopCoroutine(fadeCor);

        fadeCor = StartCoroutine(FadeCoroutine(1, time, callback));
    }

    public void FadeOut(float time, Action callback = null)
    {
        if (fadeCor != null) StopCoroutine(fadeCor);

        fadeCor = StartCoroutine(FadeCoroutine(0, time, callback));
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float time, Action callback)
    {
        Color col = fadeImage.color;
        float startAlpha = fadeImage.color.a;

        Color startColor = new Color(col.r, col.g, col.b, startAlpha);
        Color endColor = new Color(col.r, col.g, col.b, targetAlpha);

        if (Mathf.Abs(targetAlpha - startAlpha) > 0.01f)
        {
            fadeImage.gameObject.SetActive(true);
        }

        float t = 0;
        while (t < time)
        {
            yield return null;
            t += Time.unscaledDeltaTime;

            fadeImage.color = Color.Lerp(startColor, endColor, t / time);
        }

        fadeImage.color = endColor;

        if (targetAlpha <= 0.01f)
        {
            fadeImage.gameObject.SetActive(false);
        }
        fadeCor = null;

        callback?.Invoke();
    }
}
