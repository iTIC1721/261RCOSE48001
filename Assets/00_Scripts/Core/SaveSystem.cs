using System.IO;
using UnityEngine;

public enum SaveType
{
    Deck,
    Inventory
}

public static class SaveSystem
{
    public static string GetSavePath()
    {
        return Application.persistentDataPath;
    }

    #region Deck
    public static string GetDeckDirectory()
    {
        string folderPath = GetSavePath() + "/deck";
        Directory.CreateDirectory(folderPath);

        return folderPath;
    }

    static string GetDeckPath(string deckId)
    {
        return GetDeckDirectory() + $"/save_{deckId}.json";
    }

    public static void SaveDeck(DeckSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetDeckPath(data.deckId), json);

        Log.LogMessage($"Saved Deck: {GetDeckPath(data.deckId)}");
    }

    public static DeckSaveData LoadDeck(string deckId)
    {
        string path = GetDeckPath(deckId);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        return JsonUtility.FromJson<DeckSaveData>(json);
    }
    #endregion

    #region Inventory
    static string GetPlayerDataPath()
    {
        return GetSavePath() + $"/player.json";
    }

    public static void SavePlayerData(PlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPlayerDataPath(), json);

        Log.LogMessage($"Saved Inventory: {GetPlayerDataPath()}");
    }

    public static PlayerSaveData LoadPlayerData()
    {
        string path = GetPlayerDataPath();

        if (!File.Exists(path))
        {
            var data = new PlayerSaveData();
            SavePlayerData(data);
            return data;
        }

        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }
    #endregion
}
