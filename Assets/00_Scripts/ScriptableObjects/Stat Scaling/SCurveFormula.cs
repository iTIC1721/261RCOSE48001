using UnityEngine;

/// <summary>
/// S커브 증가 공식 — 초반 완만 → 중반 급증 → 후반 완만하게 수렴합니다.
/// Smoothstep 보간을 사용합니다.
/// 공식: Lerp(minMultiplier, maxMultiplier, smoothstep(t))
///       t = stage / totalStages
///
/// 예) minMultiplier = 1.0, maxMultiplier = 4.0
///   stage 0         → 1.0배
///   stage 절반       → 2.5배 (중간값)
///   stage totalStages → 4.0배
/// </summary>
[CreateAssetMenu(fileName = "SCurveFormula", menuName = "Stat Scaling/S-Curve")]
public class SCurveFormula : ScalingFormula
{
    [Tooltip("첫 스테이지(0)의 배율")]
    [Min(0f)]
    public float minMultiplier = 1f;

    [Tooltip("마지막 스테이지의 배율")]
    [Min(0f)]
    public float maxMultiplier = 4f;

    public override float Evaluate(int stage, int totalStages)
    {
        if (totalStages <= 0) return minMultiplier;

        float t = Mathf.Clamp01((float)stage / totalStages);
        float smoothT = t * t * (3f - 2f * t);     // smoothstep
        return Mathf.Lerp(minMultiplier, maxMultiplier, smoothT);
    }
}