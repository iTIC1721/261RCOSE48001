using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public enum SaveType
{
    Deck,
    Inventory
}

public static class SaveSystem
{
    public static bool encrypt = false;

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
        return GetSavePath() + $"/player";
    }

    public static void SavePlayerData(PlayerSaveData data)
    {
        if (encrypt == true)
        {
            string json = JsonUtility.ToJson(data, false);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);

            byte[] encryptedBytes = AESHelper.Encrypt(plainBytes);
            File.WriteAllBytes(GetPlayerDataPath(), encryptedBytes);
        }
        else
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetPlayerDataPath(), json, System.Text.Encoding.UTF8);
        }

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

        if (encrypt == true)
        {
            byte[] encryptedBytes = File.ReadAllBytes(path);
            byte[] decryptedBytes = AESHelper.Decrypt(encryptedBytes);

            string json = Encoding.UTF8.GetString(decryptedBytes);
            return JsonUtility.FromJson<PlayerSaveData>(json);
        }
        else
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerSaveData>(json);
        }
    }
    #endregion
}

public static class AESHelper
{
    private const string commonKey = "MemorixMemorix00";

    private static readonly byte[] keyBytes;
    private static readonly byte[] iv = new byte[16]; // 현재는 고정 (추후 개선 가능)

    static AESHelper()
    {
        keyBytes = Encoding.UTF8.GetBytes(commonKey);
    }

    public static byte[] Encrypt(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;

            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }
    }

    public static byte[] Decrypt(byte[] encryptedData)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;

            using (var ms = new MemoryStream(encryptedData))
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var result = new MemoryStream())
            {
                cs.CopyTo(result);
                return result.ToArray();
            }
        }
    }
}
