using System;
using System.Collections.Generic;

[Serializable]
public class DaySession
{
    public int dayIndex;

    public List<WordState> newWords;
    public List<WordState> reviewWords;

    public List<WordState> totalWords;

    public int currentIndex;

    public List<ReviewResult> results = new();
}
