using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDB", menuName = "DB/MonsterDB")]
public class MonsterDB : ScriptableObject
{
    public List<MonsterData> monsters;

    public MonsterData GetMonsterData(int id)
    {
        return monsters.Find(c => c.id == id);
    }
}
