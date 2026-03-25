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
    public static List<Deck> GetAllDecks()
    {
        List<Deck> decks = new();

        string path = SaveSystem.GetDeckDirectory();
        var files = Directory.GetFiles(path, "save_*.json");

        foreach (var file in files)
        {
            string json = File.ReadAllText(file);
            Deck deck = JsonUtility.FromJson<Deck>(json);

            decks.Add(deck);
        }

        return decks.OrderBy(d => d.name).ToList();
    }

    public static string CreateDeckFromCSV(string csvPath, string deckName, int dailyLimit = 30)
    {
        string deckId = Guid.NewGuid().ToString();

        var cards = CSVLoader.Load(csvPath);

        Deck deck = new Deck();

        deck.id = deckId;
        deck.name = deckName;
        deck.startDate = CustomTime.GetTimeNow();

        deck.cards = cards;

        SaveSystem.SaveDeck(deck);

        return deckId;
    }

    public static bool DeleteDeck(string deckId)
    {
        string path = SaveSystem.GetDeckDirectory() + $"/save_{deckId}.json";

        if (!File.Exists(path))
            return false;

        File.Delete(path);

        return true;
    }

    public static void RenameDeck(string deckId, string newName)
    {
        var data = SaveSystem.LoadDeck(deckId);

        if (data == null) return;

        data.name = newName;

        SaveSystem.SaveDeck(data);
    }
}