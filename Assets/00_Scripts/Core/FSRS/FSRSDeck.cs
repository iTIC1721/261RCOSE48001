using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    New,
    Learning,
    Review
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

    public List<int> todayCardIds = new List<int>();

    // FSRS weight (덱마다 존재)
    public float[] w = new float[21] {
        0.40255f, 1.18385f, 3.173f, 5.69105f,
        7.1949f, 0.5345f, 1.4604f, 0.0046f,
        1.54575f, 0.1192f, 1.01925f, 1.9395f,
        0.11f, 0.29605f, 2.2698f, 0.2315f,
        2.9898f, 0.51655f, 0.6621f, 0.5f,
        0.1f
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