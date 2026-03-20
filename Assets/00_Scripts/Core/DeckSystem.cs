using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class DeckInfo
{
    public string deckId;
    public string deckName;
    public int totalWords;
    public int learnedWords;
    public float progress; // 0 ~ 1
}

public static class DeckSystem
{
    public static List<DeckInfo> GetAllDecks()
    {
        List<DeckInfo> decks = new();

        string path = SaveSystem.GetSavePath();

        var files = Directory.GetFiles(path, "save_*.json");

        foreach (var file in files)
        {
            string json = File.ReadAllText(file);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            DeckInfo info = GetDeckInfo(data);

            decks.Add(info);
        }

        return decks.OrderBy(d => d.deckName).ToList();
    }

    public static DeckInfo GetDeckInfo(SaveData data)
    {
        DeckInfo info = new DeckInfo();

        info.deckId = data.deckId;
        info.deckName = data.deckName;

        info.totalWords = data.words.Count;
        info.learnedWords = data.words.Count(w => w.isLearned);

        info.progress = CalculateProgress(data.words);

        return info;
    }

    static float CalculateProgress(List<WordState> words)
    {
        if (words == null || words.Count == 0)
            return 0f;

        int learned = words.Count(w => w.isLearned);

        return learned / (float)words.Count;
    }


    public static string CreateDeckFromCSV(string csvPath, string deckName, int dailyLimit = 30)
    {
        string deckId = Guid.NewGuid().ToString();

        var words = CSVLoader.Load(csvPath);

        SaveData data = new SaveData();

        data.deckId = deckId;
        data.deckName = deckName;

        data.words = words;

        data.dailyLimit = dailyLimit;
        data.extraPullUsed = 0;

        data.startDate = CustomTime.GetTimeNow().Date.ToString();
        data.lastStudyDate = CustomTime.GetTimeNow().ToString();

        data.currentSession = null;

        SaveSystem.Save(data);

        return deckId;
    }

    public static bool DeleteDeck(string deckId)
    {
        string path = Application.persistentDataPath + $"/save_{deckId}.json";

        if (!File.Exists(path))
            return false;

        File.Delete(path);

        return true;
    }

    public static void RenameDeck(string deckId, string newName)
    {
        var data = SaveSystem.Load(deckId);

        if (data == null) return;

        data.deckName = newName;

        SaveSystem.Save(data);
    }
}