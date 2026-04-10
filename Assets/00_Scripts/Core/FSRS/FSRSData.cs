using System;

[Serializable]
public class FSRSData
{
    public float d; // difficulty (이전 리뷰 직후 상태)
    public float s; // stability
    public float t; // 이전 리뷰까지 경과 시간

    public int rating; // Again=1, Hard=2, Good=3, Easy=4

    public float t_next; // 현재 리뷰 → 다음 리뷰까지 경과 시간
    public int y; // 다음 리뷰에서 recall 성공 여부 (0 or 1)

    public bool isQuiz;

    public FSRSData(ReviewLog prev, ReviewLog next)
    {
        // 이전 리뷰에서 기록된 카드 상태
        d = prev.lastDifficulty;
        s = prev.lastStability;
        t = prev.elapsedDays;

        // 현재 리뷰의 평가
        rating = prev.rating;

        // 다음 리뷰까지의 간격 및 결과
        t_next = next.elapsedDays;
        y = next.recall;
    }
}
