using System;

[Serializable]
public class FSRSData
{
    public float d;
    public float s;
    public float t;

    public int rating;
    public int y;

    public FSRSData(ReviewLog log)
    {
        d = log.lastDifficulty;
        s = log.lastStability;
        t = log.elapsedDays;

        rating = log.rating;
        y = log.recall;
    }
}
