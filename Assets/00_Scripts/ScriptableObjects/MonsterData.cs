using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Entity/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int id;
    public float hp;
    public float damage;
    public GameObject monster;
}
