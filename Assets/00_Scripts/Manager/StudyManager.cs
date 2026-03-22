using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StudyManager : MonoBehaviour
{
    public string deckId;

    public List<WordState> words;

    public int dailyLimit = 30;

    public Scheduler scheduler;

    public DaySession currentDaySession;
    public StageDifficulty currentStageDifficulty;

    DateTime startDate;
    DateTime lastStudyDate;

    public void Save()
    {
        var data = SaveSystem.Load(deckId);
        
        data.words = words;

        data.dailyLimit = dailyLimit;
        data.extraPullUsed = scheduler.extraPullUsed;

        data.lastStudyDate = CustomTime.GetTimeNow().ToString();

        data.currentSession = currentDaySession;

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

            currentDaySession = data.currentSession;
        }
        else
        {
            words = CSVLoader.Load(deckId + ".csv");

            scheduler = new Scheduler(words);

            startDate = CustomTime.GetTimeNow();
            lastStudyDate = CustomTime.GetTimeNow();

            currentDaySession = null;
        }
    }


    public void StartToday()
    {
        // 오늘 세션이 이미 진행 중이면 그대로 사용
        if (!currentDaySession.IsNull() && currentDaySession.dayIndex == GetCurrentDay())
        {
            // 미완료된 스테이지가 있다면 초기화
            for (int d = Enum.GetValues(typeof(StageDifficulty)).Length - 1; d >= 0; d--)
            {
                StageDifficulty diff = (StageDifficulty)d;
                if (!MANAGER.StudyManager.GetStageProgress(diff).isCompleted)
                {
                    ClearStageProgress(diff);
                }
            }

            return;
        }

        // 날짜가 지났다면 이전 데이터는 정산
        if(!currentDaySession.IsNull() && currentDaySession.dayIndex != GetCurrentDay())
        {
            Log.LogMessage("이전 데이터를 정산했습니다");
            EndDay();
        }

        // 이전 기록에서 정산한 기록을 바탕으로 오늘의 학습량 및 학습 데이터를 가져옴
        List<ReviewResult> recentResults = (!currentDaySession.IsNull()) ? GetSettlementResults() : new List<ReviewResult>();
        var (newWords, reviewWords) = GetTodaySchedule(recentResults);

        // 오늘의 DaySession을 새로 생성
        currentDaySession = new DaySession
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

        var (newCount, reviewCount) = scheduler.DecideDailyLoad(dailyLimit, reviewCandidates.Count, LSS);
        Log.LogMessage($"New: {newCount}, Review: {reviewCount}");

        var reviewWords = scheduler.GetReviewWords(words, GetCurrentDay(), reviewCount);
        var newWords = scheduler.GetNewWords(newCount);

        return (newWords, reviewWords);
    }

    private int GetMissedDays()
    {
        return (CustomTime.GetTimeNow().Date - lastStudyDate.Date).Days;
    }

    private List<WordState> GetCombinedList(List<WordState> newWords, List<WordState> reviewWords)
    {
        var list = new List<WordState>();

        list.AddRange(reviewWords);
        list.AddRange(newWords);

        ShuffleHelper.Shuffle(list);

        return list;
    }

    public int PredictLeftDays()
    {
        int remaining = scheduler.newQueue.Count;

        if (currentDaySession != null)
        {
            int totalToday = currentDaySession.newWords.Count;

            remaining -= totalToday;
        }

        remaining = Mathf.Max(remaining, 0);

        return Mathf.CeilToInt(remaining / (float)dailyLimit);
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

    public WordState GetNextWord()
    {
        var stage = GetStageProgress(currentStageDifficulty);

        if (stage.currentIndex >= currentDaySession.totalWords.Count)
        {
            Log.LogMessage($"{stage.currentIndex}, {currentDaySession.totalWords.Count}");
            return null;
        }

        return currentDaySession.totalWords[stage.currentIndex];
    }

    public string[] GetRandomMeanings(int count, string exceptMeaning)
    {
        string[] result = new string[count];
        RandomQueue<WordState> rq = new RandomQueue<WordState>(words.Where(w => w.meaning != exceptMeaning));
        for (int i = 0; i < count; i++)
        {
            WordState word = rq.Dequeue();
            result[i] = word.meaning;
        }

        return result;
    }

    public void SubmitAnswer(ReviewResult result)
    {
        var stage = GetStageProgress(currentStageDifficulty);

        stage.results.Add(result);
        stage.currentIndex++;

        Save();

        if (stage.currentIndex >= currentDaySession.totalWords.Count)
        {
            int reward = CompleteStage();
            Log.LogMessage($"Reward: {reward}");
        }
    }

    public void ClearStageProgress(StageDifficulty diff)
    {
        var stage = GetStageProgress(diff);

        stage.results.Clear();
        stage.currentIndex = 0;

        Save();
    }

    public int CompleteStage()
    {
        var stage = GetStageProgress(currentStageDifficulty);

        // 이미 클리어한 스테이지일 경우
        if (stage.isCompleted)
            return 0;

        // 스테이지가 아직 끝나지 않았을 경우
        if (stage.currentIndex < currentDaySession.totalWords.Count)
            return 0;

        int reward = 0;
        for (int i = (int)currentStageDifficulty; i >= 0; i--)
        {
            var s = GetStageProgress((StageDifficulty)i);
            s.isCompleted = true;
            reward += RewardSystem.Calculate(stage.results);
        }

        Save();
        return reward;
    }

    private void EndDay()
    {
        // 공부한 결과가 있는 단어만 정산
        var allResults = GetSettlementResults();
        Log.LogMessage($"정산할 데이터 개수: {allResults.Count}");

        foreach (var r in allResults)
        {
            AdaptiveEngine.UpdateWord(r.word, r, GetCurrentDay());

            var target = words.FirstOrDefault(w => w.word == r.word.word);
            if (target != null)
            {
                target.avgResponseTime = r.word.avgResponseTime;
                target.strength = r.word.strength;
                target.correctCount = r.word.correctCount;
                target.difficulty = r.word.difficulty;
                target.totalReviews = r.word.totalReviews;
                target.lastReviewedDay = r.word.lastReviewedDay;
                target.isLearned = r.word.isLearned;
                target.nextReviewDay = r.word.nextReviewDay;
            }
        }

        Save();
        scheduler = new Scheduler(words);
    }

    private List<ReviewResult> GetSettlementResults()
    {
        var allResults = currentDaySession.stages
            .SelectMany(s => s.results)
            .GroupBy(r => r.word)
            .Select(g => g.Last())
            .ToList();

        return allResults;
    }

    public int GetCurrentDay()
    {
        DateTime start = startDate.Date;
        DateTime today = CustomTime.GetTimeNow().Date;

        return (today - start).Days;
    }
}