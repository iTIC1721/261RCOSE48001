using UnityEngine;

public class DefaultMonsterAnimationEvent : MonoBehaviour
{
    private DefaultMonsterBT monsterBT;

    private void Awake()
    {
        monsterBT = GetComponentInParent<DefaultMonsterBT>();
    }

    public void SpawnAttackObject()
    {
        monsterBT.SpawnAttackObject();
    }
}
