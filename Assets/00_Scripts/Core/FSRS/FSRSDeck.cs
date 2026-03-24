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

    [SerializeField] private long lastSessionDateTicks = 0;
    public DateTime lastSessionDate
    {
        get => new DateTime(lastSessionDateTicks);
        set => lastSessionDateTicks = value.Ticks;
    }

    public List<int> todayCardIds = new List<int>();

    // FSRS weight (덱마다 존재)
    public float[] w = new float[17] {
        0.4f, 0.6f, 2.4f, 5.8f,
        4.93f, 0.94f, 0.86f, 0.01f,
        1.49f, 0.14f, 0.94f, 2.18f,
        0.05f, 0.34f, 1.26f, 0.29f, 2.61f
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

        // 기준: 500개 이상
        if (logCount < 500)
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
        DateTime start = lastSessionDate.Date;
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