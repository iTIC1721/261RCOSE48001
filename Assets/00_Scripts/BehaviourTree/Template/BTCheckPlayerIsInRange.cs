using UnityEngine;

public class BTCheckPlayerIsInRange : BTNode
{
    private static int playerLayerMask = 1 << LayerMask.NameToLayer("Player");
    private Monster monster;
    private float range = 5f;

    public BTCheckPlayerIsInRange(Monster monster, float range)
    {
        this.monster = monster;
        this.range = range;
    }

    public override BTNodeState Evaluate()
    {
        Collider2D collider = Physics2D.OverlapCircle(monster.transform.position, range, playerLayerMask);

        if (collider is null) return BTNodeState.Failure;
        else return BTNodeState.Success;
    }
}
