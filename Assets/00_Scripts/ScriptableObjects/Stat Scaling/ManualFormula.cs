using UnityEngine;

/// <summary>
/// 스테이지별 배율을 직접 입력하는 공식입니다.
/// multipliers[0] = 스테이지 0의 배율, multipliers[1] = 스테이지 1의 배율, ...
///
/// 스테이지 수가 배열 범위를 초과하면 마지막 값을 그대로 유지합니다.
/// </summary>
[CreateAssetMenu(fileName = "ManualFormula", menuName = "Stat Scaling/Manual")]
public class ManualFormula : ScalingFormula
{
    [Tooltip("스테이지 순서대로 배율을 입력하세요.\n인덱스 0 = 스테이지 0, 인덱스 1 = 스테이지 1, ...")]
    public float[] multipliers = { 1f };

    public override float Evaluate(int stage, int totalStages)
    {
        if (multipliers == null || multipliers.Length == 0)
            return 1f;

        int index = Mathf.Clamp(stage, 0, multipliers.Length - 1);
        return multipliers[index];
    }
}