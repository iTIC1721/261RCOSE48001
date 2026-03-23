using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDB", menuName = "DB/CharacterDB")]
public class CharacterDB : ScriptableObject
{
    public List<CharacterData> characters;

    public CharacterData GetCharacterData(int id)
    {
        return characters.Find(c => c.id == id);
    }
}
