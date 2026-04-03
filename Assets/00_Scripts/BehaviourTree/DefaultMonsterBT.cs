using System.Collections.Generic;
using UnityEngine;

public class DefaultMonsterBT : MonsterBT
{
    public Player player;

    protected override BTNode SetupBehaviorTree()
    {
        BTNode root = new BTSelectorNode(new List<BTNode>
        {
            new BTMoveToPlayer(player.transform, monster)
        });

        return root;
    }
}
