using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public void AddMoney(int money)
    {
        InventorySaveData data = SaveSystem.LoadInventory();
        if (data == null)
            data = new InventorySaveData();

        data.money += money;
        SaveSystem.SaveInventory(data);
    }

    public bool SpendMoney(int money)
    {
        InventorySaveData data = SaveSystem.LoadInventory();
        if (data == null)
            data = new InventorySaveData();

        if (data.money >= money)
        {
            data.money -= money;
            SaveSystem.SaveInventory(data);
            return true;
        }

        return false;
    }
}
