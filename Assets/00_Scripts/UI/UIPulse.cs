using System.Collections;
using UnityEngine;

public class UIPulse : MonoBehaviour
{
    [Header("크기 설정")]
    public float minScale = 0.8f;   // 최소 크기 비율
    public float maxScale = 1.2f;   // 최대 크기 비율

    [Header("속도 설정")]
    public float speed = 1.5f;      // 펄싱 속도

    private RectTransform targetTransform;
    private Vector3 originalScale;

    private void Awake()
    {
        targetTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        originalScale = targetTransform.localScale;
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            // 커지는 단계
            yield return ScaleTo(maxScale, speed);
            // 작아지는 단계
            yield return ScaleTo(minScale, speed);
        }
    }

    private IEnumerator ScaleTo(float targetScale, float duration)
    {
        Vector3 startScale = targetTransform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = MyMath.EaseInOut(elapsed / duration);
            targetTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        targetTransform.localScale = endScale;
    }

    // 외부에서 펄싱 중지/재개 가능
    public void StopPulse()
    {
        StopAllCoroutines();
        targetTransform.localScale = originalScale;
    }

    public void StartPulse()
    {
        StopAllCoroutines();
        StartCoroutine(PulseRoutine());
    }
}
