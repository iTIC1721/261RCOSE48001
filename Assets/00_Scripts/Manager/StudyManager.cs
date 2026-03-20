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
    public StageDifficulty currentStageDifficulty;

    DateTime startDate;
    DateTime lastStudyDate;

    public void Save()
    {
        var data = SaveSystem.Load(deckId);
        
        data.words = words;

        data.dailyLimit = dailyLimit;
        data.extraPullUsed = scheduler.extraPullUsed;

        data.lastStudyDate = DateTime.Now.ToString();

        data.currentSession = currentSession;

        SaveSystem.Save(data);
    }

    public void Load(string deckId)
    {
        var data = SaveSystem.Load(deckId);

        if (data != null)
        {
            this.deckId = data.deckId;

            words = data.words;
            dailyLimit = data.dailyLimit;

            scheduler = new Scheduler(words);

            startDate = DateTime.Parse(data.startDate);
            lastStudyDate = DateTime.Parse(data.lastStudyDate);

            currentSession = data.currentSession;
            Log.LogMessage($"Load: {data}");
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


    public void StartToday()
    {
        // 오늘 세션이 이미 진행 중이면 그대로 사용
        if (!currentSession.IsNull() && currentSession.dayIndex == GetCurrentDay())
        {
            return;
        }

        // 날짜가 지났다면 이전 데이터는 정산
        if(!currentSession.IsNull() && currentSession.dayIndex != GetCurrentDay())
        {
            EndDay();
        }

        // 이전 기록에서 정산한 기록을 바탕으로 오늘의 학습량 및 학습 데이터를 가져옴
        List<ReviewResult> recentResults = (!currentSession.IsNull()) ? GetSettlementResults() : new List<ReviewResult>();
        var (newWords, reviewWords) = GetTodaySchedule(recentResults);

        // 오늘의 DaySession을 새로 생성
        currentSession = new DaySession
        {
            dayIndex = GetCurrentDay(),
            newWords = newWords,
            reviewWords = reviewWords,
            totalWords = GetCombinedList(newWords, reviewWords),
            stages = new StageProgress[Enum.GetValues(typeof(StageDifficulty)).Length]
        };
    }

    private (List<WordState> newWords, List<WordState> reviewWords) GetTodaySchedule(List<ReviewResult> recentResults)
    {
        int missed = GetMissedDays();

        if (missed > 0)
        {
            lastStudyDate = DateTime.Now;
        }

        float LSS = AdaptiveEngine.CalculateLSS(recentResults, missed);
        Log.LogMessage($"LSS: {LSS}");

        var reviewCandidates = words
            .Where(w => w.isLearned && w.nextReviewDay <= GetCurrentDay())
            .ToList();

        string tmp = "Reviews: ";
        foreach (var rc in reviewCandidates)
        {
            tmp += rc.word + " ";
        }
        Log.LogMessage(tmp);

        var (newCount, reviewCount) =
            scheduler.DecideDailyLoad(dailyLimit, reviewCandidates.Count, LSS);
        Log.LogMessage($"New: {newCount}, Review: {reviewCount}");

        var reviewWords = scheduler.GetReviewWords(words, GetCurrentDay(), reviewCount);
        var newWords = scheduler.GetNewWords(newCount);

        return (newWords, reviewWords);
    }

    private int GetMissedDays()
    {
        return (DateTime.Now.Date - lastStudyDate.Date).Days;
    }

    private List<WordState> GetCombinedList(List<WordState> newWords, List<WordState> reviewWords)
    {
        var list = new List<WordState>();

        list.AddRange(reviewWords);
        list.AddRange(newWords);

        return list;
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


    public StageProgress GetStageProgress(StageDifficulty diff)
    {
        if (currentSession.stages[(int)diff] == null)
        {
            currentSession.stages[(int)diff] = new StageProgress
            {
                currentIndex = 0,
                results = new List<ReviewResult>(),
                isCompleted = false
            };
        }

        return currentSession.stages[(int)diff];
    }

    public WordState GetNextWord()
    {
        var stage = GetStageProgress(currentStageDifficulty);

        if (stage.currentIndex >= currentSession.totalWords.Count)
        {
            Log.LogMessage($"{stage.currentIndex}, {currentSession.totalWords.Count}");
            return null;
        }

        return currentSession.totalWords[stage.currentIndex];
    }

    public void SubmitAnswer(ReviewResult result)
    {
        var stage = GetStageProgress(currentStageDifficulty);

        stage.results.Add(result);
        stage.currentIndex++;

        Save();

        if (stage.currentIndex >= currentSession.totalWords.Count)
        {
            int reward = CompleteStage();
            Log.LogMessage(reward);
        }
    }

    public int CompleteStage()
    {
        var stage = GetStageProgress(currentStageDifficulty);

        // 이미 클리어한 스테이지일 경우
        if (stage.isCompleted)
            return 0;

        // 스테이지가 아직 끝나지 않았을 경우
        if (stage.currentIndex < currentSession.totalWords.Count)
            return 0;

        stage.isCompleted = true;

        Save();

        int reward = RewardSystem.Calculate(stage.results);
        return reward;
    }

    private void EndDay()
    {
        // 공부한 결과가 있는 단어만 정산
        var allResults = GetSettlementResults();

        foreach (var r in allResults)
        {
            AdaptiveEngine.UpdateWord(r.word, r, GetCurrentDay());
        }

        Save();
    }

    private List<ReviewResult> GetSettlementResults()
    {
        var allResults = currentSession.stages
            .SelectMany(s => s.results)
            .GroupBy(r => r.word)
            .Select(g => g.Last())
            .ToList();

        return allResults;
    }

    public int GetCurrentDay()
    {
        DateTime start = startDate.Date;
        DateTime today = DateTime.Now.Date;

        return (today - start).Days;
    }
}