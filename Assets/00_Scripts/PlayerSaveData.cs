using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int money;
    public int characterId;
    public uint purchaseList = 0b11;

    public List<string> upgradeKeys = new List<string>();
    public List<int> upgradeValues = new List<int>();
}
