using System;
using UnityEngine;

public static class RewardSystem
{
    public const int TotalReward = 100;

    private static string BestKey(string userId)
        => $"rewardBest_{userId}_{DateTime.Today:yyyy-MM-dd}";

    private static string EarnedKey(string userId)
        => $"rewardEarned_{userId}_{DateTime.Today:yyyy-MM-dd}";

    public static int GetBestProgressCount(string userId)
        => PlayerPrefs.GetInt(BestKey(userId), 0);

    /// <summary>
    /// 오늘 누적 지급된 보상 총액.
    /// </summary>
    public static int GetEarnedTotal(string userId)
        => PlayerPrefs.GetInt(EarnedKey(userId), 0);

    /// <summary>
    /// 세션 종료 시 정산. 최고 기록 갱신 시에만 차액 지급.
    /// </summary>
    public static int CalculateReward(int progressCount, int totalCount, string userId)
    {
        if (totalCount <= 0 || progressCount <= 0) return 0;

        int prev = GetBestProgressCount(userId);
        if (progressCount <= prev) return 0;

        int prevEarned = GetEarnedTotal(userId);
        int newEarned = Mathf.FloorToInt(TotalReward * (float)progressCount / totalCount);
        int delta = newEarned - prevEarned;
        if (delta <= 0) return 0;

        PlayerPrefs.SetInt(BestKey(userId), progressCount);
        PlayerPrefs.SetInt(EarnedKey(userId), newEarned);
        PlayerPrefs.Save();

        CleanOldKeys(userId);
        return delta;
    }

    // Obsolete
    public static int CalculateQuizReward(StageDifficulty diff)
    {
        switch (diff)
        {
            case StageDifficulty.Easy:
                return 100;
            case StageDifficulty.Hard:
                return 100;
        }

        return 0;
    }

    public static int CalculateLearnReward()
    {
        return 0;
    }

    public static void CleanOldKeys(string userId)
    {
        for (int i = 1; i <= 30; i++)
        {
            string date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
            PlayerPrefs.DeleteKey($"rewardBest_{userId}_{date}");
            PlayerPrefs.DeleteKey($"rewardEarned_{userId}_{date}");
        }
    }
}