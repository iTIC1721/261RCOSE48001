using System;
using UnityEngine;

public static class FSRSScheduler
{
    public static float GetRetrievability(float s, float t)
    {
        return Mathf.Pow(1 + t / (9f * s), -1f);
    }

    public static float UpdateDifficulty(float d, int rating, float[] w)
    {
        float delta = 0;

        if (rating == 1) delta = w[6];
        else if (rating == 2) delta = w[7];
        else if (rating == 3) delta = -w[8];
        else if (rating == 4) delta = -w[9];

        return Mathf.Clamp(d + delta * (10 - d) / 9f, 1f, 10f);
    }

    public static float StabilityRecall(float d, float s, float r, int rating, float[] w)
    {
        float hardPenalty = (rating == 2) ? w[15] : 1f;
        float easyBonus = (rating == 4) ? w[16] : 1f;

        return s * (1 +
            Mathf.Exp(w[4]) *
            (11 - d) *
            Mathf.Pow(s, -w[5]) *
            (Mathf.Exp((1 - r) * w[6]) - 1) *
            hardPenalty *
            easyBonus
        );
    }

    public static float StabilityForget(float d, float s, float[] w)
    {
        return w[11] *
            Mathf.Pow(d, -w[12]) *
            (Mathf.Pow(s + 1, w[13]) - 1);
    }

    public static void Review(Card card, Deck deck, int rating)
    {
        float t = (float)(DateTime.Now - card.lastReview).TotalDays;
        float r = GetRetrievability(card.stability, t);

        // 로그 저장용 값
        float oldD = card.difficulty;
        float oldS = card.stability;

        // Difficulty 업데이트
        card.difficulty = UpdateDifficulty(card.difficulty, rating, deck.w);

        // Stability 업데이트
        if (rating == 1)
            card.stability = StabilityForget(card.difficulty, card.stability, deck.w);
        else
            card.stability = StabilityRecall(card.difficulty, card.stability, r, rating, deck.w);

        // 로그 저장
        card.logs.Add(new ReviewLog
        {
            reviewTime = DateTime.Now,
            elapsedDays = t,
            lastDifficulty = oldD,
            lastStability = oldS,
            rating = rating,
            recall = (rating == 1) ? 0 : 1
        });

        card.lastReview = DateTime.Now;
    }
}
