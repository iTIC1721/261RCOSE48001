using System.Collections.Generic;

public static class RewardSystem
{
    public static int CalculateQuizReward(StageDifficulty diff)
    {
        switch (diff)
        {
            case StageDifficulty.Easy:
                return 30;
            case StageDifficulty.Hard:
                return 50;
        }

        return 0;
    }

    public static int CalculateLearnReward()
    {
        return 20;
    }
}