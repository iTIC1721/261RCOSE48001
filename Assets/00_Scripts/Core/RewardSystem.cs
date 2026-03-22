using System.Collections.Generic;

public static class RewardSystem
{
    public static int CalculateReward(StageDifficulty diff)
    {
        switch (diff)
        {
            case StageDifficulty.Easy:
                return 20;
            case StageDifficulty.Normal:
                return 50;
            case StageDifficulty.Hard:
                return 100;
        }

        return 0;
    }
}