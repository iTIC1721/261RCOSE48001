using System;
using System.Collections.Generic;

[Serializable]
public class PlayerSaveData
{
    public int money;
    public int characterId;
    public int purchaseList = 0b11;
}
