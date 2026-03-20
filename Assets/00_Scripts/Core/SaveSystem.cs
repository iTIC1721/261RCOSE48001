using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static string GetSavePath()
    {
        return Application.persistentDataPath;
    }

    static string GetPath(string deckId)
    {
        return GetSavePath() + $"/save_{deckId}.json";
    }

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(data.deckId), json);

        Debug.Log($"Saved Deck: {GetPath(data.deckId)}");
    }

    public static SaveData Load(string deckId)
    {
        string path = GetPath(deckId);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
