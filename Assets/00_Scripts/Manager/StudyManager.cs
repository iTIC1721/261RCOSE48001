using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StudyManager : MonoBehaviour
{
    public Deck deck;

    private SessionManager session;
    private Card currentCard;

    public StageDifficulty currentStageDifficulty;

    private int cardViewCount = 0;

    public void Save()
    {
        SaveSystem.SaveDeck(deck);
    }

    public void Load(string deckId)
    {
        deck = SaveSystem.LoadDeck(deckId);

        if (deck == null)
            deck = new Deck();
    }


    public void StartToday()
    {
        List<Card> todayCards;
        // 날짜가 지났다면 이전 데이터는 정산
        if (deck.lastSessionDate.Date != CustomTime.GetTimeNow().Date)
        {
            deck.EndOfDay();
            SaveSystem.SaveDeck(deck);

            todayCards = MainScheduler.GetTodayCards(deck);
            // TODO: 랜덤 배치

            deck.todayCardIds.Clear();
            foreach (var c in todayCards)
                deck.todayCardIds.Add(c.id);

            deck.lastSessionDate = CustomTime.GetTimeNow();
            Log.LogMessage("이전 데이터를 정산했습니다");
        }
        else
        {
            todayCards = MainScheduler.GetCardsById(deck, deck.todayCardIds);
        }

        session = new SessionManager(todayCards);
    }

    public Card GetNextWord()
    {
        if (!session.HasNext())
        {
            // 세션 종료
            SaveSystem.SaveDeck(deck);
            return null;
        }

        currentCard = session.GetNextCard();
        
        cardViewCount++;
        if (cardViewCount >= 5)
        {
            SaveSystem.SaveDeck(deck);
            cardViewCount = 0;
        }

        return currentCard;
    }

    public string[] GetRandomMeanings(int count, string exceptBack)
    {
        string[] result = new string[count];
        RandomQueue<Card> rq = new RandomQueue<Card>(deck.cards.Where(w => w.back != exceptBack));
        for (int i = 0; i < count; i++)
        {
            Card word = rq.Dequeue();
            result[i] = word.back;
        }

        return result;
    }

    public void SubmitAnswer(int rating)
    {
        MainScheduler.RateCard(currentCard, deck, rating);

        // Again이면 다시 넣기
        if (rating == 1)
        {
            session.Requeue(currentCard);
        }
    }
}