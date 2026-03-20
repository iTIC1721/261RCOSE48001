using System;
using System.Collections.Generic;

[Serializable]
public class StageProgress
{
    public string stageName; // "Easy", "Normal", "Hard"
    public int currentIndex;

    public List<ReviewResult> results = new();
    public bool isCompleted = false;
}

[Serializable]
public class DaySession
{
    public int dayIndex;

    public List<WordState> newWords;
    public List<WordState> reviewWords;

    public List<WordState> totalWords;

    public Dictionary<string, StageProgress> stages = new();
}
