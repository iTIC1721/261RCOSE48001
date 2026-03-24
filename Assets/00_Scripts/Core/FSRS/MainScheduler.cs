using System;
using System.Collections.Generic;
using UnityEngine;

public class MainScheduler
{
    public int newLimit = 20;

    public List<Card> GetTodayCards(Deck deck)
    {
        List<Card> result = new List<Card>();
        DateTime now = DateTime.Now;

        // Review 카드 (due 지난 것)
        foreach (var card in deck.cards)
        {
            if (card.state == CardState.Review && card.due <= now)
            {
                result.Add(card);
            }
        }

        // New 카드 (limit 적용)
        int newCount = 0;

        foreach (var card in deck.cards)
        {
            if (card.state == CardState.New)
            {
                result.Add(card);
                newCount++;

                if (newCount >= newLimit)
                    break;
            }
        }

        return result;
    }

    public static void RateCard(Card card, Deck deck, int rating)
    {
        // FSRS 적용 + 로그 저장
        FSRSScheduler.Review(card, deck, rating);

        // 다음 복습 시간 설정
        if (rating == 1) // Again
        {
            // 짧게 다시
            card.due = DateTime.Now.AddMinutes(1);
            card.state = CardState.Learning;
        }
        else
        {
            // FSRS 기반 interval
            card.due = DateTime.Now.AddDays(card.stability);
            card.state = CardState.Review;
        }
    }

    
}
