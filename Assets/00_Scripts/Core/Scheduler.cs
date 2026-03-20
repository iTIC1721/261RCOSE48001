using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scheduler
{
    public List<WordState> totalWords;
    public RandomQueue<WordState> newQueue;
    public int extraPullUsed = 0;
    public int maxExtraPull = 3;

    public Scheduler(List<WordState> words)
    {
        totalWords = new List<WordState>(words);
        newQueue = new RandomQueue<WordState>(words.Where(w => !w.isLearned).ToList());
    }

    public (int newCount, int reviewCount) DecideDailyLoad(int dailyLimit, int reviewDue, float LSS)
    {
        int reviewCount = Mathf.Min(reviewDue, dailyLimit);

        int remaining = dailyLimit - reviewCount;

        float ratio =
            (LSS > 0.8f) ? 0.6f :
            (LSS < 0.6f) ? 0.3f : 0.5f;

        int newCount = Mathf.RoundToInt(remaining * ratio);

        return (newCount, reviewCount);
    }

    public List<WordState> GetNewWords(int count)
    {
        List<WordState> result = new();

        for (int i = 0; i < count && newQueue.Count > 0; i++)
        {
            result.Add(newQueue.Dequeue());
        }

        return result;
    }

    public List<WordState> GetReviewWords(List<WordState> words, int today, int limit)
    {
        return words
            .Where(w => w.nextReviewDay <= today && w.totalReviews > 0)
            .OrderByDescending(w => GetPriority(w, today))
            .Take(limit)
            .ToList();
    }

    float GetPriority(WordState w, int today)
    {
        float overdue = today - w.nextReviewDay;
        return Mathf.Max(0, overdue) * 3f + w.difficulty;
    }
}