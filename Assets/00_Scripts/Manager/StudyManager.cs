using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class StudyManager : MonoBehaviour
{
    public Deck deck;

    private SessionManager session;
    private Card currentCard;


    public DaySession currentDaySession;
    public StageDifficulty currentStageDifficulty;

    DateTime startDate;
    DateTime lastStudyDate;

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
            Log.LogMessage("이전 데이터를 정산했습니다");
            deck.EndOfDay();
            SaveSystem.SaveDeck(deck);

            todayCards = MainScheduler.GetTodayCards(deck);

            deck.todayCardIds.Clear();
            foreach (var c in todayCards)
                deck.todayCardIds.Add(c.id);

            deck.lastSessionDate = CustomTime.GetTimeNow();
        }
        else
        {
            todayCards = MainScheduler.GetCardsById(deck, deck.todayCardIds);
        }

        session = new SessionManager(todayCards);
    }

    public StageProgress GetStageProgress(StageDifficulty diff)
    {
        if (currentDaySession.stages[(int)diff] == null)
        {
            currentDaySession.stages[(int)diff] = new StageProgress
            {
                currentIndex = 0,
                results = new List<ReviewResult>(),
                isCompleted = false
            };
        }

        return currentDaySession.stages[(int)diff];
    }

    public Card GetNextWord()
    {
        if (!session.HasNext())
        {
            // TODO: 세션 종료
            return null;
        }

        currentCard = session.GetNextCard();

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

    public void ClearStageProgress(StageDifficulty diff)
    {
        var stage = GetStageProgress(diff);

        stage.results.Clear();
        stage.currentIndex = 0;

        //Save();
    }

    public int GetCurrentDay()
    {
        DateTime start = startDate.Date;
        DateTime today = CustomTime.GetTimeNow().Date;

        return (today - start).Days;
    }
}