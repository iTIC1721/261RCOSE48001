using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int money;
    public int characterId;
    public uint purchaseList = 0b11;
}
