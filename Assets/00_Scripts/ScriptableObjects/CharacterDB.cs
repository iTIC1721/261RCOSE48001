using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDB", menuName = "DB/CharacterDB")]
public class CharacterDB : ScriptableObject
{
    public List<Character> characters;

    public Character GetCharacterData(int id)
    {
        return characters.Find(c => c.id == id);
    }
}
