using System.Collections.Generic;

public static class RewardSystem
{
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
}