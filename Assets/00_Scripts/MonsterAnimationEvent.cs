using UnityEngine;

public class MonsterAnimationEvent : MonoBehaviour
{
    private Monster monster;

    private void Awake()
    {
        monster = GetComponentInParent<Monster>();
    }

    public void SpawnAttackObject()
    {
        monster.SpawnAttackObject();
    }
}
