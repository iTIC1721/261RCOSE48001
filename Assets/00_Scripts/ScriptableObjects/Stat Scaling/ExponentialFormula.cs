using UnityEngine;

/// <summary>
/// 지수(복리) 증가 공식 — 스테이지마다 일정 비율씩 배율이 곱해집니다.
/// 공식: baseMultiplier ^ stage
///
/// 예) baseMultiplier = 1.15
///   stage 0 → 1.00배
///   stage 5 → 2.01배
///   stage 9 → 3.52배
/// </summary>
[CreateAssetMenu(fileName = "ExponentialFormula", menuName = "Stat Scaling/Exponential")]
public class ExponentialFormula : ScalingFormula
{
    [Tooltip("스테이지당 곱해지는 배율\n1.15 = 매 스테이지 15% 복리 증가")]
    [Min(1f)]
    public float baseMultiplier = 1.15f;

    public override float Evaluate(int stage, int totalStages)
    {
        return Mathf.Pow(baseMultiplier, stage);
    }
}