using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public void AddMoney(int money)
    {
        PlayerSaveData data = SaveSystem.LoadPlayerData();
        if (data == null)
            data = new PlayerSaveData();

        data.money += money;
        SaveSystem.SavePlayerData(data);
    }

    public bool SpendMoney(int money)
    {
        PlayerSaveData data = SaveSystem.LoadPlayerData();
        if (data == null)
            data = new PlayerSaveData();

        if (data.money >= money)
        {
            data.money -= money;
            SaveSystem.SavePlayerData(data);
            return true;
        }

        return false;
    }
}
