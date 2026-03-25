using System;
using System.Collections.Generic;

[Serializable]
public class StageProgress
{
    public int currentIndex;

    public List<ReviewResult> results = new();
    public bool isCompleted = false;

    public void Clear()
    {
        currentIndex = 0;
        results.Clear();
        isCompleted = false;
    }
}

[Serializable]
public class DaySession
{
    public int dayIndex;

    public List<WordState> newWords;
    public List<WordState> reviewWords;

    public List<WordState> totalWords;

    public StageProgress[] stages = new StageProgress[Enum.GetValues(typeof(StageDifficulty)).Length];

    public bool IsNull()
    {
        if (totalWords.Count == 0) return true;
        return false;
    }
}
