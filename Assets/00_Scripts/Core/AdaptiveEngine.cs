using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AdaptiveEngine
{
    public static float CalculateLSS(List<ReviewResult> recentResults, int missedDays)
    {
        if (recentResults == null || recentResults.Count == 0)
            return Mathf.Clamp01(0.6f - missedDays * 0.05f);

        float acc = recentResults.Count(r => r.correct) / (float)recentResults.Count;
        float avgTime = recentResults.Average(r => r.responseTime);

        float speed = Mathf.Clamp01(2.5f / avgTime);

        float baseScore = 0.7f * acc + 0.3f * speed;
        float panelty = missedDays * 0.05f;
        
        float finalScore = baseScore - panelty;

        return Mathf.Clamp01(finalScore);
    }

    public static void UpdateWord(WordState w, ReviewResult r, int today)
    {
        w.avgResponseTime = Mathf.Lerp(w.avgResponseTime, r.responseTime, 0.3f);

        if (r.correct)
        {
            w.strength *= 1.2f;
            w.correctCount++;
        }
        else
        {
            w.strength *= 0.5f;
            w.difficulty += 0.1f;
        }

        w.totalReviews++;
        w.lastReviewedDay = today;
        w.isLearned = true;

        float interval = -w.strength * Mathf.Log(0.75f);
        w.nextReviewDay = today + Mathf.Max(1, Mathf.RoundToInt(interval));
    }
}
