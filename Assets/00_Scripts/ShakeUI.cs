using System.Collections;
using UnityEngine;

public class ShakeUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 originalLocalPos;

    private Transform camTransform;
    private Vector3 camOriginalPos;

    private Coroutine shakeCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalLocalPos = rectTransform.localPosition;

        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
            camOriginalPos = camTransform.localPosition;
        }
    }

    public void Shake(float intensity, bool shakeCamera = true)
    {
        if (shakeCoroutine != null || intensity <= 0)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, shakeCamera));
    }

    private IEnumerator ShakeCoroutine(float intensity, bool shakeCamera)
    {
        float duration = Mathf.Clamp(intensity * 0.3f, 0.01f, 1.0f);
        float time = 0f;

        float frequency = 20f;

        while (time < duration)
        {
            float progress = time / duration;

            // 점점 줄어드는 감쇠
            float damping = 1f - progress;

            // Perlin Noise 기반 자연스러운 흔들림
            float x = (Mathf.PerlinNoise(Time.time * frequency, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, Time.time * frequency) - 0.5f) * 2f;

            Vector3 offset = new Vector3(x, y, 0f) * intensity * 10f * damping;

            rectTransform.localPosition = originalLocalPos + offset;
            if (shakeCamera && camTransform != null)
            {
                camTransform.localPosition = camOriginalPos + offset * 0.004f;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 원래 위치 복구
        rectTransform.localPosition = originalLocalPos;
        if (shakeCamera && camTransform != null)
        {
            camTransform.localPosition = camOriginalPos;
        }

        shakeCoroutine = null;
    }
}
