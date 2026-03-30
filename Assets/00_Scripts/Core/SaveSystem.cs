using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

    public static void SaveDeck(Deck deck)
    {
        string json = JsonUtility.ToJson(deck, true);
        File.WriteAllText(GetDeckPath(deck.id), json, System.Text.Encoding.UTF8);

        Log.LogMessage($"Saved Deck: {GetDeckPath(deck.id)}");
    }

    public static Deck LoadDeck(string deckId)
    {
        string path = GetDeckPath(deckId);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        return JsonUtility.FromJson<Deck>(json);
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
        string encryptedJson = AESHelper.Encrypt(json, AESHelper.commonKey);
        File.WriteAllText(GetPlayerDataPath(), encryptedJson);

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

        string encryptedJson = File.ReadAllText(path, System.Text.Encoding.UTF8);
        string json = AESHelper.Decrypt(encryptedJson, AESHelper.commonKey);
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }
    #endregion
}

public static class AESHelper
{
    public const string commonKey = "MemorixMemorix00";

    public static string Encrypt(string text, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16]; // IV는 0으로 초기화하여 사용
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            byte[] encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static string Decrypt(string encryptedText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16]; // IV는 암호화할 때와 동일하게 설정
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
