using UnityEngine;

public class BTCheckPlayerIsInAttackRange : BTNode
{
    private static int playerLayerMask = 1 << LayerMask.NameToLayer("Player");
    private Monster monster;
    private float range = 1.5f;
    private float angle = 120f;

    public BTCheckPlayerIsInAttackRange(Monster monster, float range, float angle)
    {
        this.monster = monster;
        this.range = range;
        this.angle = angle;
    }

    public override BTNodeState Evaluate()
    {
        Collider[] collider = Physics.OverlapSphere(monster.transform.position, range, playerLayerMask);

        if (collider.Length <= 0) return BTNodeState.Failure;
        
        Transform player = collider[0].transform;
        if (Vector3.Angle(monster.transform.forward, player.transform.position - monster.transform.position) < angle * 0.5f)
        {
            return BTNodeState.Success;
        }
        else
        {
            return BTNodeState.Failure;
        }
    }
}
