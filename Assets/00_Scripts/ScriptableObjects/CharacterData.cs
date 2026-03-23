using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Entity/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int id;
    public new string name;
    public string desc;
    public GameObject character;
}
