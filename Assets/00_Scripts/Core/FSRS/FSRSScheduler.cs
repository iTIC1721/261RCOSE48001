using System;
using UnityEngine;

public static class FSRSScheduler
{
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Learning / Relearning step °£°Ý (ºÐ ´ÜÀ§)
    // Anki ±âº»°ª°ú µ¿ÀÏÇÏ°Ô ¼³Á¤
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static readonly float[] LearningSteps = { 1f, 10f };   // ºÐ
    public static readonly float[] RelearningSteps = { 10f };       // ºÐ

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Retrievability (¸Á°¢ °î¼±)
    // R(t, S) = (1 + t / (9 * S))^(-1)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float GetRetrievability(float s, float t)
    {
        if (s <= 0f) return 0f;
        return Mathf.Pow(1f + t / (9f * s), -1f);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Èñ¸Á º¸Á¸À²(desiredRetention)·ÎºÎÅÍ ´ÙÀ½ °£°Ý °è»ê
    // I = S * ((1/DR)^(1/(-1)) - 1) * 9  ¡æ  DR=0.9ÀÌ¸é I=S
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float GetInterval(float s, float desiredRetention = 0.9f)
    {
        // R = (1 + t/(9S))^(-1) = DR  ¡æ  t = 9S * (DR^(-1) - 1)
        float interval = 9f * s * (Mathf.Pow(desiredRetention, -1f) - 1f);
        return Mathf.Max(1f, interval);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÃÊ±â Difficulty : D0(G) = w4 - exp(w5 * (G-1)) + 1
    // °ø½Ä FSRS-5 ¼ö½Ä
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float InitDifficulty(int rating, float[] w)
    {
        float d = w[4] - Mathf.Exp(w[5] * (rating - 1)) + 1f;
        return Mathf.Clamp(d, 1f, 10f);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÃÊ±â Stability : S0(G) = w[G-1]
    // Ã¹ ¸®ºä¿¡¼­´Â ´Ü¼øÈ÷ w[0]~w[3] »ç¿ë
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float InitStability(int rating, float[] w)
    {
        int idx = Mathf.Clamp(rating - 1, 0, 3);
        return Mathf.Max(0.1f, w[idx]);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Difficulty ¾÷µ¥ÀÌÆ®
    // delta_D(G): Again=w6, Hard=w7, Good=-w8, Easy=-w9
    // linear damping: delta * (10 - D) / 9
    // mean reversion: 0.1 * (w4 - D)  (w4°¡ "Good"ÀÇ ±âº» ³­ÀÌµµ)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float UpdateDifficulty(float d, int rating, float[] w)
    {
        float delta;
        if (rating == 1) delta = w[6];
        else if (rating == 2) delta = w[7];
        else if (rating == 3) delta = -w[8];
        else delta = -w[9];

        // linear damping
        float dPrime = d + delta * (10f - d) / 9f;

        // mean reversion (w4 = ±âº» ³­ÀÌµµ)
        float dDoublePrime = 0.1f * w[4] + 0.9f * dPrime;

        return Mathf.Clamp(dDoublePrime, 1f, 10f);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Stability (Recall) : ¼º°øÀûÀÎ º¹½À ÈÄ stability
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float StabilityRecall(float d, float s, float r, int rating, float[] w)
    {
        float hardPenalty = (rating == 2) ? w[15] : 1f;
        float easyBonus = (rating == 4) ? w[16] : 1f;

        float growth =
            Mathf.Exp(w[8]) *
            (11f - d) *
            Mathf.Pow(s, -w[9]) *
            (Mathf.Exp((1f - r) * w[10]) - 1f);

        return s * (1f + growth * hardPenalty * easyBonus);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Stability (Forget) : Again ÈÄÀÇ post-lapse stability
    // min(¡¦, S) ·Î lapse ÀÌÀü °ªÀ» ÃÊ°úÇÏÁö ¾Êµµ·Ï Á¦ÇÑ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float StabilityForget(float d, float s, float r, float[] w)
    {
        float sf = w[11] *
            Mathf.Pow(d, -w[12]) *
            (Mathf.Pow(s + 1f, w[13]) - 1f) *
            Mathf.Exp(w[14] * (1f - r));

        return Mathf.Min(sf, s);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Short-term Stability : °°Àº ³¯ Àçº¹½À (Learning / Relearning ´Ü°è)
    // S' = S * exp(w17 * (G - 3 + w18))
    // Good/Easy ´Â S' >= S º¸Àå, Hard/Again Àº °¨¼Ò °¡´É
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static float StabilityShortTerm(float s, int rating, float[] w)
    {
        float sPrime = s * Mathf.Exp(w[17] * (rating - 3f + w[18]));

        // Good(3), Easy(4) ´Â °¨¼Ò ºÒ°¡
        if (rating >= 3)
            sPrime = Mathf.Max(sPrime, s);

        return Mathf.Max(0.1f, sPrime);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Ä«µå ½Å±Ô ÃÊ±âÈ­ (Ã¹ ¹øÂ° ¸®ºä Àü È£Ãâ)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static void InitCard(Card card, Deck deck)
    {
        card.difficulty = InitDifficulty(3, deck.w); // Good ±âÁØ ±âº»°ª
        card.stability = deck.w[2];                 // w[2] = Good ±âÁØ ÃÊ±â stability
        card.stepIndex = 0;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // º¹½À Ã³¸® ÇÙ½É (FSRSScheduler.Review)
    // Review »óÅÂ Ä«µåÀÇ stability / difficulty ¾÷µ¥ÀÌÆ® + ·Î±× ÀúÀå
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public static void Review(Card card, Deck deck, int rating, bool isQuiz = false)
    {
        DateTime now = CustomTime.GetTimeNow();
        float t = (float)(now - card.lastReview).TotalDays;
        float r = GetRetrievability(card.stability, t);

        float oldD = card.difficulty;
        float oldS = card.stability;

        // Difficulty ¾÷µ¥ÀÌÆ®
        card.difficulty = UpdateDifficulty(card.difficulty, rating, deck.w);

        // Stability ¾÷µ¥ÀÌÆ®
        if (rating == 1)
            card.stability = StabilityForget(card.difficulty, card.stability, r, deck.w);
        else
            card.stability = StabilityRecall(card.difficulty, card.stability, r, rating, deck.w);

        // ·Î±× ÀúÀå
        card.logs.Add(new ReviewLog
        {
            reviewTime = now,
            elapsedDays = t,
            lastDifficulty = oldD,
            lastStability = oldS,
            rating = rating,
            recall = (rating == 1) ? 0 : 1,
        });

        card.lastReview = now;
    }
}
