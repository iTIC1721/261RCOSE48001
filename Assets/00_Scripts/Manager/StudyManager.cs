using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Overlays;
using UnityEngine;

public class StudyManager : MonoBehaviour
{
    public string deckId;

    public List<WordState> words;

    public int dailyLimit = 30;

    public Scheduler scheduler;

    public DaySession currentSession;

    DateTime startDate;
    DateTime lastStudyDate;

    public void SaveDeck()
    {
        SaveData data = new SaveData();

        data.deckId = deckId;
        data.words = words;
        data.dailyLimit = dailyLimit;
        data.extraPullUsed = scheduler.extraPullUsed;

        data.lastStudyDate = DateTime.Now.ToString();

        data.currentSession = currentSession;

        SaveSystem.Save(data);
    }

    public void LoadDeck(string deckId)
    {
        var data = SaveSystem.Load(deckId);

        if (data != null)
        {
            words = data.words;
            dailyLimit = data.dailyLimit;

            scheduler = new Scheduler(words);

            startDate = DateTime.Parse(data.startDate);
            lastStudyDate = DateTime.Parse(data.lastStudyDate);

            currentSession = data.currentSession;
        }
        else
        {
            words = CSVLoader.Load(deckId + ".csv");

            scheduler = new Scheduler(words);

            startDate = DateTime.Now;
            lastStudyDate = DateTime.Now;

            currentSession = null;
        }
    }

    int GetMissedDays()
    {
        return (DateTime.Now.Date - lastStudyDate.Date).Days;
    }

    public int PredictLeftDays()
    {
        int remaining = scheduler.newQueue.Count;

        if (currentSession != null)
        {
            int totalToday = currentSession.newWords.Count;

            // 아직 안 푼 신규만 제외
            int remainingToday = totalToday - currentSession.currentIndex;

            remaining -= remainingToday;
        }

        remaining = Mathf.Max(remaining, 0);

        return Mathf.CeilToInt(remaining / (float)dailyLimit);
    }

    public (List<WordState> newWords, List<WordState> reviewWords) GetTodaySchedule(List<ReviewResult> recentResults)
    {
        int missed = GetMissedDays();

        if (missed > 0)
        {
            lastStudyDate = DateTime.Now;
        }

        float LSS = AdaptiveEngine.CalculateLSS(recentResults, missed);

        var reviewCandidates = words
            .Where(w => w.nextReviewDay <= GetCurrentDay())
            .ToList();

        var (newCount, reviewCount) =
            scheduler.DecideDailyLoad(dailyLimit, reviewCandidates.Count, LSS);

        var reviewWords = scheduler.GetReviewWords(words, GetCurrentDay(), reviewCount);
        var newWords = scheduler.GetNewWords(newCount);

        return (newWords, reviewWords);
    }

    public void StartToday()
    {
        // 이미 진행 중이면 그대로 사용
        if (currentSession != null && currentSession.dayIndex == GetCurrentDay())
            return;

        List<ReviewResult> recentResults = currentSession.results;
        var (newWords, reviewWords) = GetTodaySchedule(recentResults);

        currentSession = new DaySession
        {
            dayIndex = GetCurrentDay(),
            newWords = newWords,
            reviewWords = reviewWords,
            totalWords = GetCombinedList(newWords, reviewWords),
            currentIndex = 0
        };
    }

    List<WordState> GetCombinedList(List<WordState> newWords, List<WordState> reviewWords)
    {
        var list = new List<WordState>();

        list.AddRange(reviewWords);
        list.AddRange(newWords);

        return list;
    }

    public WordState GetNextWord()
    {
        if (currentSession.currentIndex >= currentSession.totalWords.Count)
            return null;

        return currentSession.totalWords[currentSession.currentIndex];
    }

    public void SubmitAnswer(ReviewResult result)
    {
        currentSession.results.Add(result);

        currentSession.currentIndex++;

        if (currentSession.currentIndex >= currentSession.totalWords.Count)
        {
            EndSession(currentSession.results);
            currentSession = null;
        }

        SaveDeck();
    }

    public int EndSession(List<ReviewResult> results)
    {
        foreach (var r in results)
        {
            AdaptiveEngine.UpdateWord(r.word, r, GetCurrentDay());
        }

        return RewardSystem.Calculate(results);
    }

    public int GetCurrentDay()
    {
        DateTime start = startDate.Date;
        DateTime today = DateTime.Now.Date;

        return (today - start).Days;
    }

    public void PullExtra(int count)
    {
        int remaining = scheduler.newQueue.Count;
        scheduler.ApplyExtraPull(count, ref remaining);
    }
}