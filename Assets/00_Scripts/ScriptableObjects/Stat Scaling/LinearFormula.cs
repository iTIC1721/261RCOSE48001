using UnityEngine;

/// <summary>
/// 선형 증가 공식 — 스테이지마다 일정량씩 배율이 오릅니다.
/// 공식: 1 + stage * increasePerStage
///
/// 예) increasePerStage = 0.1
///   stage 0 → 1.0배
///   stage 5 → 1.5배
///   stage 9 → 1.9배
/// </summary>
[CreateAssetMenu(fileName = "LinearFormula", menuName = "Stat Scaling/Linear")]
public class LinearFormula : ScalingFormula
{
    [Tooltip("스테이지당 증가 배율\n0.1 = 매 스테이지 10%p 상승")]
    [Min(0f)]
    public float increasePerStage = 0.1f;

    public override float Evaluate(int stage, int totalStages)
    {
        return 1f + stage * increasePerStage;
    }
}