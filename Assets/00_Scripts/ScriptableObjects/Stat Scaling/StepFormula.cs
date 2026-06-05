using UnityEngine;

/// <summary>
/// 계단식 증가 공식 — N 스테이지마다 배율이 점프합니다.
/// 공식: 1 + floor(stage / stagesPerStep) * increasePerStep
///
/// 예) stagesPerStep = 3, increasePerStep = 0.5
///   stage 0~2 → 1.0배
///   stage 3~5 → 1.5배
///   stage 6~8 → 2.0배
/// </summary>
[CreateAssetMenu(fileName = "StepFormula", menuName = "Stat Scaling/Step")]
public class StepFormula : ScalingFormula
{
    [Tooltip("배율이 오르는 스테이지 주기")]
    [Min(1)]
    public int stagesPerStep = 3;

    [Tooltip("한 계단당 증가 배율\n0.5 = 계단마다 50%p 상승")]
    [Min(0f)]
    public float increasePerStep = 0.5f;

    public override float Evaluate(int stage, int totalStages)
    {
        return 1f + (stage / stagesPerStep) * increasePerStep;
    }
}