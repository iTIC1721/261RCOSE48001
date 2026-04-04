using System.Collections.Generic;
using UnityEngine;

public class DefaultMonsterBT : MonsterBT
{
    protected override BTNode SetupBehaviorTree()
    {
        BTNode root = new BTSelectorNode(new List<BTNode>
        {
            new BTConditionDecorator(new BTSequenceNode(new List<BTNode>
            {
                new BTCheckAnimationEnd(monster, "DAMAGED"),
                new BTCheckPlayerIsInRange(monster, 10),
                new BTMoveToPlayer(Player.Instance.transform, monster)
            }), () => !monster.IsDied),
            new BTIdle(monster)
        });

        return root;
    }
}
