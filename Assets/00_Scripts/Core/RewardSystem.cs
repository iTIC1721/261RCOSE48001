using System.Collections.Generic;

public static class RewardSystem
{
    public static int Calculate(List<ReviewResult> results)
    {
        int reward = 0;

        foreach (var r in results)
        {
            int baseVal = 10;

            if (r.correct) baseVal += 5;
            if (r.responseTime < 1.5f) baseVal += 3;

            reward += baseVal;
        }

        return reward;
    }
}