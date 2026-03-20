using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string deckId;
    public string deckName;

    public List<WordState> words;

    public int dailyLimit;

    public int extraPullUsed;

    public string startDate;
    public string lastStudyDate;

    public DaySession currentSession;

    public override string ToString()
    {
        string result = 
            $"[{deckName}]\n" +
            $"Deck ID: {deckId}\n" +
            $"-------------------------------\n" +
            $"Word Count: {words.Count}\n" +
            $"Daily Limit: {dailyLimit}\n" +
            $"Last Study Date: {lastStudyDate}";
        return result;
    }
}
