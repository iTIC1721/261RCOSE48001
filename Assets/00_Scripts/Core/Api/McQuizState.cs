using UnityEngine;

/// <summary>
/// 4지선다 퀴즈 패널(mcPanel)에 붙이는 헬퍼 컴포넌트.
/// 현재 문제의 정답 버튼 인덱스를 저장합니다.
/// </summary>
public class McQuizState : MonoBehaviour
{
    public int CorrectIndex { get; private set; }

    public void SetCorrectIndex(int idx)
    {
        CorrectIndex = idx;
    }
}