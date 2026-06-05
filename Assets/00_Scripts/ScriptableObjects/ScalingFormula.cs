using UnityEngine;

/// <summary>
/// 스테이지 스탯 배율 공식의 베이스 클래스입니다.
/// 이 클래스를 상속해 Evaluate()를 구현하면 새 공식 애셋을 만들 수 있습니다.
///
/// 새 공식 추가 방법:
///   1. ScalingFormula를 상속한 클래스를 작성합니다.
///   2. Evaluate()에 원하는 공식을 구현합니다.
///   3. Unity 에디터에서 [Create Asset] 메뉴로 애셋을 생성합니다.
///   4. StageStatScaler 인스펙터의 HP / 공격력 슬롯에 애셋을 연결합니다.
/// </summary>
public abstract class ScalingFormula : ScriptableObject
{
    /// <summary>
    /// 스탯 배율을 반환합니다.
    /// </summary>
    /// <param name="stage">현재 스테이지 (0부터 시작)</param>
    /// <param name="totalStages">전체 스테이지 수</param>
    /// <returns>1.0 = 원본 그대로, 2.0 = 2배</returns>
    public abstract float Evaluate(int stage, int totalStages);
}