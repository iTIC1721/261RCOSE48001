using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.Overlays;
using UnityEngine;

public class StudyManager : MonoBehaviour
{
    public string deckId;

    public List<WordState> words;

    public int dailyLimit = 30;

    public Scheduler scheduler;

    public DaySession currentSession;
    public string currentStage = "Easy";

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

            remaining -= totalToday;
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

        // 날짜가 지났다면 이전 데이터는 정산
        EndDay();

        List<ReviewResult> recentResults =
            currentSession != null
            ? currentSession.stages
                .SelectMany(s => s.Value.results)
                .GroupBy(r => r.word)
                .Select(g => g.Last())
                .ToList()
            : new List<ReviewResult>();
        var (newWords, reviewWords) = GetTodaySchedule(recentResults);

        currentSession = new DaySession
        {
            dayIndex = GetCurrentDay(),
            newWords = newWords,
            reviewWords = reviewWords,
            totalWords = GetCombinedList(newWords, reviewWords),
            stages = new Dictionary<string, StageProgress>()
        };
    }

    List<WordState> GetCombinedList(List<WordState> newWords, List<WordState> reviewWords)
    {
        var list = new List<WordState>();

        list.AddRange(reviewWords);
        list.AddRange(newWords);

        return list;
    }

    StageProgress GetStage()
    {
        if (!currentSession.stages.ContainsKey(currentStage))
        {
            currentSession.stages[currentStage] = new StageProgress
            {
                stageName = currentStage,
                currentIndex = 0,
                results = new List<ReviewResult>()
            };
        }

        return currentSession.stages[currentStage];
    }

    public WordState GetNextWord()
    {
        var stage = GetStage();

        if (stage.currentIndex >= currentSession.totalWords.Count)
            return null;

        return currentSession.totalWords[stage.currentIndex];
    }

    public void SubmitAnswer(ReviewResult result)
    {
        var stage = GetStage();

        stage.results.Add(result);
        stage.currentIndex++;

        SaveDeck();

        if (stage.currentIndex >= currentSession.totalWords.Count)
        {
            CompleteStage();
        }
    }

    public int CompleteStage()
    {
        var stage = GetStage();

        // 이미 클리어한 경우
        if (stage.isCompleted)
            return 0;

        if (stage.currentIndex < currentSession.totalWords.Count)
            return 0;

        stage.isCompleted = true;

        int reward = RewardSystem.Calculate(stage.results);

        SaveDeck();

        return reward;
    }

    public void EndDay()
    {
        var allResults = currentSession.stages
        .SelectMany(s => s.Value.results)
        .GroupBy(r => r.word)
        .Select(g => g.Last())
        .ToList();

        foreach (var r in allResults)
        {
            AdaptiveEngine.UpdateWord(r.word, r, GetCurrentDay());
        }

        SaveDeck();
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