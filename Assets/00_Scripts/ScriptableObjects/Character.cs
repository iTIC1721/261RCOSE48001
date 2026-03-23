using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "PlayerData/Character")]
public class Character : ScriptableObject
{
    public int id;
    public new string name;
    public string desc;
    public int price;
    public GameObject character;
}
