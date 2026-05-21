using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Entity/CharacterData")]
public class CharacterData : ScriptableObject
{
    public int id;

    [Header("캐릭터")]
    public new string name;
    public string desc;
    public GameObject character;

    [Header("투사체")]
    public string projectileName = "PlayerProjectile";
}
