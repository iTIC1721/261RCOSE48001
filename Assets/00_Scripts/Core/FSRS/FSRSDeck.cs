using System;
using System.Collections.Generic;

public enum CardState
{
    New,
    Learning,
    Review
}

[Serializable]
public class Deck
{
    public string name;

    public List<Card> cards = new List<Card>();

    // FSRS weight (덱마다 존재)
    public float[] w = new float[17];

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

        // TODO: 덱 저장
        //DeckStorage.Save(deck);

        Log.LogMessage("End of day training complete.");
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
    public float stability = 0f;

    public int stepIndex = 0;
    public DateTime due;

    public DateTime lastReview;

    public List<ReviewLog> logs = new List<ReviewLog>();
}

[Serializable]
public class ReviewLog
{
    public DateTime reviewTime;

    public float elapsedDays;

    public float lastDifficulty;
    public float lastStability;

    public int rating; // 1~4
    public int recall; // 0 or 1
}