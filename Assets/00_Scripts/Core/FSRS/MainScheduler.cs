using System;
using System.Collections.Generic;
using UnityEngine;

public static class MainScheduler
{
    public static int newLimit = 20;

    // 희망 보존율 (0.7~0.97 권장, Anki 기본값 0.9)
    public static float desiredRetention = 0.9f;

    public static List<Card> GetTodayCards(Deck deck)
    {
        DateTime now = CustomTime.GetTimeNow();

        List<Card> reviewResult = new List<Card>();
        List<Card> learningResult = new List<Card>();

        foreach (var card in deck.cards)
        {
            if (card.due > now) continue;

            switch (card.state)
            {
                case CardState.Review:
                    reviewResult.Add(card);
                    break;
                case CardState.Learning:
                case CardState.Relearning:
                    learningResult.Add(card);
                    break;
            }
        }

        // New 카드 (limit 적용, 셔플)
        int newCount = 0;
        List<Card> newResult = new List<Card>();
        List<Card> newCardPool = new List<Card>(deck.cards);
        ShuffleHelper.Shuffle(newCardPool);

        foreach (var card in newCardPool)
        {
            if (card.state == CardState.New)
            {
                newResult.Add(card);
                if (++newCount >= newLimit) break;
            }
        }

        // Learning > New > Review 순서로 제공 (Anki 기본 순서)
        List<Card> result = new List<Card>();
        result.AddRange(learningResult);
        result.AddRange(newResult);
        result.AddRange(reviewResult);

        return result;
    }

    public static List<Card> GetCardsById(Deck deck, List<int> ids)
    {
        List<Card> result = new List<Card>();

        foreach (var id in ids)
        {
            var card = deck.cards.Find(c => c.id == id);
            if (card != null)
                result.Add(card);
        }

        return result;
    }

    // ─────────────────────────────────────────────
    // 카드 평가 처리
    // ─────────────────────────────────────────────
    public static void RateCard(Card card, Deck deck, int rating)
    {
        DateTime now = CustomTime.GetTimeNow();

        switch (card.state)
        {
            // ── New ──────────────────────────────
            case CardState.New:
                ProcessNew(card, deck, rating, now);
                break;

            // ── Learning ─────────────────────────
            case CardState.Learning:
                ProcessLearning(card, deck, rating, now);
                break;

            // ── Review ───────────────────────────
            case CardState.Review:
                ProcessReview(card, deck, rating, now);
                break;

            // ── Relearning ───────────────────────
            case CardState.Relearning:
                ProcessRelearning(card, deck, rating, now);
                break;
        }

        Log.LogMessage($"[{card.state}] stability={card.stability:F3}  difficulty={card.difficulty:F3}  due={card.due}");
    }

    // ─────────────────────────────────────────────
    // New 카드 첫 평가
    // ─────────────────────────────────────────────
    static void ProcessNew(Card card, Deck deck, int rating, DateTime now)
    {
        float[] w = deck.w;

        // ① 공식 FSRS-5 초기 D0, S0 설정
        card.difficulty = FSRSScheduler.InitDifficulty(rating, w);
        card.stability = FSRSScheduler.InitStability(rating, w);
        card.stepIndex = 0;
        card.lastReview = now;

        // 로그 저장 (elapsed=0, 이전 D/S는 초기값)
        card.logs.Add(new ReviewLog
        {
            reviewTime = now,
            elapsedDays = 0f,
            lastDifficulty = card.difficulty,
            lastStability = card.stability,
            rating = rating,
            recall = (rating == 1) ? 0 : 1,
        });

        if (rating == 1) // Again → Learning 첫 step
        {
            card.state = CardState.Learning;
            card.stepIndex = 0;
            card.due = now.AddMinutes(FSRSScheduler.LearningSteps[0]);
        }
        else
        {
            // Good / Easy → 곧바로 Review로 졸업
            // Hard도 짧은 간격으로 Learning 진행
            if (rating == 2)
            {
                card.state = CardState.Learning;
                card.stepIndex = 0;
                card.due = now.AddMinutes(FSRSScheduler.LearningSteps[0]);
            }
            else
            {
                GraduateToReview(card, deck, now);
            }
        }
    }

    // ─────────────────────────────────────────────
    // Learning 단계 처리 (short-term stability 업데이트)
    // ─────────────────────────────────────────────
    static void ProcessLearning(Card card, Deck deck, int rating, DateTime now)
    {
        float[] w = deck.w;
        float elapsed = (float)(now - card.lastReview).TotalDays;

        float oldD = card.difficulty;
        float oldS = card.stability;

        // Difficulty 업데이트 (Learning 중에도 적용)
        card.difficulty = FSRSScheduler.UpdateDifficulty(card.difficulty, rating, w);

        // Short-term Stability 업데이트
        card.stability = FSRSScheduler.StabilityShortTerm(card.stability, rating, w);

        // 로그 저장
        card.logs.Add(new ReviewLog
        {
            reviewTime = now,
            elapsedDays = elapsed,
            lastDifficulty = oldD,
            lastStability = oldS,
            rating = rating,
            recall = (rating == 1) ? 0 : 1,
        });

        card.lastReview = now;

        float[] steps = FSRSScheduler.LearningSteps;

        if (rating == 1) // Again → step 초기화
        {
            card.stepIndex = 0;
            card.due = now.AddMinutes(steps[0]);
        }
        else if (rating == 2) // Hard → 현재 step 유지
        {
            card.due = now.AddMinutes(steps[card.stepIndex]);
        }
        else // Good / Easy
        {
            card.stepIndex++;
            if (card.stepIndex >= steps.Length)
            {
                // 모든 step 통과 → Review 졸업
                GraduateToReview(card, deck, now);
            }
            else
            {
                card.due = now.AddMinutes(steps[card.stepIndex]);
            }
        }
    }

    // ─────────────────────────────────────────────
    // Review 단계 처리 (장기 stability 업데이트)
    // ─────────────────────────────────────────────
    static void ProcessReview(Card card, Deck deck, int rating, DateTime now)
    {
        // FSRS 복습 처리 (stability / difficulty 갱신 + 로그)
        FSRSScheduler.Review(card, deck, rating);

        if (rating == 1) // Again → Relearning
        {
            card.state = CardState.Relearning;
            card.stepIndex = 0;
            card.due = now.AddMinutes(FSRSScheduler.RelearningSteps[0]);
        }
        else
        {
            // stability 기반으로 다음 복습 간격 계산
            card.state = CardState.Review;
            float intervalDays = FSRSScheduler.GetInterval(card.stability, desiredRetention);
            card.due = now.AddDays(intervalDays);
        }
    }

    // ─────────────────────────────────────────────
    // Relearning 단계 처리
    // ─────────────────────────────────────────────
    static void ProcessRelearning(Card card, Deck deck, int rating, DateTime now)
    {
        float[] w = deck.w;
        float elapsed = (float)(now - card.lastReview).TotalDays;

        float oldD = card.difficulty;
        float oldS = card.stability;

        card.difficulty = FSRSScheduler.UpdateDifficulty(card.difficulty, rating, w);
        card.stability = FSRSScheduler.StabilityShortTerm(card.stability, rating, w);

        card.logs.Add(new ReviewLog
        {
            reviewTime = now,
            elapsedDays = elapsed,
            lastDifficulty = oldD,
            lastStability = oldS,
            rating = rating,
            recall = (rating == 1) ? 0 : 1,
        });

        card.lastReview = now;

        float[] steps = FSRSScheduler.RelearningSteps;

        if (rating == 1) // Again → step 초기화
        {
            card.stepIndex = 0;
            card.due = now.AddMinutes(steps[0]);
        }
        else // Hard / Good / Easy
        {
            card.stepIndex++;
            if (card.stepIndex >= steps.Length)
            {
                // Relearning 통과 → Review로 복귀
                card.state = CardState.Review;
                float intervalDays = FSRSScheduler.GetInterval(card.stability, desiredRetention);
                card.due = now.AddDays(Mathf.Max(1f, intervalDays));
            }
            else
            {
                card.due = now.AddMinutes(steps[card.stepIndex]);
            }
        }
    }

    // ─────────────────────────────────────────────
    // Learning 완료 → Review 상태로 전환
    // ─────────────────────────────────────────────
    static void GraduateToReview(Card card, Deck deck, DateTime now)
    {
        card.state = CardState.Review;
        card.stepIndex = 0;

        float intervalDays = FSRSScheduler.GetInterval(card.stability, desiredRetention);
        card.due = now.AddDays(Mathf.Max(1f, intervalDays));
    }
}
