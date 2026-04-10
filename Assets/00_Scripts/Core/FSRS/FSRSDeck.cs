using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    New,
    Learning,
    Review,
    Relearning   // Review 카드가 Again을 받으면 진입하는 상태
}

[Serializable]
public class Deck
{
    public string id;
    public string name;

    public List<Card> cards = new List<Card>();

    [SerializeField] private long startDateTicks = 0;
    public DateTime startDate
    {
        get => new DateTime(startDateTicks);
        set => startDateTicks = value.Ticks;
    }

    [SerializeField] private long lastSessionDateTicks = 0;
    public DateTime lastSessionDate
    {
        get => new DateTime(lastSessionDateTicks);
        set => lastSessionDateTicks = value.Ticks;
    }

    [SerializeField] private long lastLearnDateTicks = 0;
    public DateTime lastLearnDate
    {
        get => new DateTime(lastLearnDateTicks);
        set => lastLearnDateTicks = value.Ticks;
    }

    public bool[] quizCompleted = new bool[Enum.GetValues(typeof(StageDifficulty)).Length];

    public List<int> todayCardIds = new List<int>();

    // FSRS-5 weight (21개)
    // w[0]~w[3]  : 첫 리뷰 rating 1~4에 대한 초기 Stability
    // w[4]~w[5]  : 초기 Difficulty (D0) 계산용
    // w[6]~w[9]  : Difficulty 업데이트 delta (rating 1~4)
    // w[10]~w[16]: Stability 업데이트 (recall / forget)
    // w[17]~w[19]: Short-term Stability 업데이트 (같은 날 재복습)
    // w[20]      : (FSRS-6에서 망각 곡선 형태 조정용, FSRS-5에서는 사용하지 않음)
    public float[] w = new float[21] {
        0.4072f, 1.1829f, 3.1262f, 15.4722f,   // w[0]~w[3]
        7.2102f, 0.5316f, 1.0651f, 0.0589f,     // w[4]~w[7]
        1.5330f, 0.1544f, 1.0071f, 1.9395f,     // w[8]~w[11]
        0.1100f, 0.2900f, 2.2700f, 0.2320f,     // w[12]~w[15]
        2.9898f, 0.5100f, 0.8000f, 0.0f,        // w[16]~w[19]
        0.0f                                      // w[20] (미사용)
    };

    public int GetLogCount()
    {
        int count = 0;
        foreach (var card in cards)
        {
            count += card.logs.Count;
        }

        return count;
    }

    public void Train()
    {
        int logCount = GetLogCount();

        // 기준: 100개 이상
        if (logCount < 100)
        {
            Log.LogMessage("Not enough logs to train.");
            return;
        }

        FSRSOptimizer optimizer = new FSRSOptimizer();

        optimizer.Train(this);
        Log.LogMessage("FSRS weights updated!");
    }

    public void EndOfDay()
    {
        // 덱 학습 - 파라미터 조정
        Train();

        // 초기화
        for (int i = 0; i < quizCompleted.Length; i++)
            quizCompleted[i] = false;

        // 덱 저장
        SaveSystem.SaveDeck(this);

        Log.LogMessage("End of day training complete.");
    }

    public int GetCurrentDay()
    {
        DateTime start = startDate.Date;
        DateTime today = CustomTime.GetTimeNow().Date;

        return (today - start).Days;
    }
}

[Serializable]
public class Card
{
    public int id;

    public string front;
    public string back;

    public CardState state = CardState.New;

    public float difficulty = 5f;
    public float stability = 1f;

    // Learning / Relearning 단계에서의 step 인덱스
    public int stepIndex = 0;

    [SerializeField] private long dueTicks;
    public DateTime due
    {
        get => new DateTime(dueTicks);
        set => dueTicks = value.Ticks;
    }

    [SerializeField] private long lastReviewTicks;
    public DateTime lastReview
    {
        get => new DateTime(lastReviewTicks);
        set => lastReviewTicks = value.Ticks;
    }

    public List<ReviewLog> logs = new List<ReviewLog>();
}

[Serializable]
public class ReviewLog
{
    [SerializeField] private long reviewTimeTicks;
    public DateTime reviewTime
    {
        get => new DateTime(reviewTimeTicks);
        set => reviewTimeTicks = value.Ticks;
    }

    public float elapsedDays;

    public float lastDifficulty;
    public float lastStability;

    public int rating; // 1~4
    public int recall; // 0 or 1
}