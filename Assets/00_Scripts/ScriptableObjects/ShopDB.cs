using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopItem
{
    public int id;
    public int characterId;
    public string name;
    public string desc;
    public int price;
}

[CreateAssetMenu(fileName = "ShopDB", menuName = "DB/ShopDB")]
public class ShopDB : ScriptableObject
{
    public List<ShopItem> items;
}
