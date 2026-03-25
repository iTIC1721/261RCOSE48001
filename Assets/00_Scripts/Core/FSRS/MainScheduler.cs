using System;
using System.Collections.Generic;
using UnityEngine;

public static class MainScheduler
{
    public static int newLimit = 20;

    public static List<Card> GetTodayCards(Deck deck)
    {
        DateTime now = CustomTime.GetTimeNow();

        // Review 카드 (due 지난 것)
        List<Card> reviewResult = new List<Card>();
        foreach (var card in deck.cards)
        {
            if (card.state == CardState.Review && card.due <= now)
            {
                reviewResult.Add(card);
            }
        }

        // New 카드 (limit 적용)
        int newCount = 0;

        List<Card> newResult = new List<Card>();
        List<Card> newCardPool = new List<Card>(deck.cards);
        ShuffleHelper.Shuffle(newCardPool);
        foreach (var card in newCardPool)
        {
            if (card.state == CardState.New)
            {
                newResult.Add(card);
                newCount++;

                if (newCount >= newLimit)
                    break;
            }
        }

        List<Card> result = new List<Card>();
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

    public static void RateCard(Card card, Deck deck, int rating)
    {
        // FSRS 적용 + 로그 저장
        if (card.state == CardState.Review)
        {
            FSRSScheduler.Review(card, deck, rating);
        }
        else
        {
            card.difficulty = FSRSScheduler.UpdateDifficulty(card.difficulty, rating, deck.w);
            card.lastReview = CustomTime.GetTimeNow();
            switch (rating)
            {
                case 2:
                    card.stability = deck.w[1]; break;
                case 3:
                    card.stability = deck.w[2]; break;
                case 4:
                    card.stability = deck.w[3]; break;
                default:
                    card.stability = deck.w[0]; break;
            }
        }

        // 다음 복습 시간 설정
        if (rating == 1) // Again
        {
            // 짧게 다시
            card.due = CustomTime.GetTimeNow().AddMinutes(1);
            card.state = CardState.Learning;
        }
        else
        {
            // FSRS 기반 interval
            Log.LogMessage($"stability: {card.stability}");
            card.due = CustomTime.GetTimeNow().AddDays(card.stability);
            card.state = CardState.Review;
        }
    }
}
