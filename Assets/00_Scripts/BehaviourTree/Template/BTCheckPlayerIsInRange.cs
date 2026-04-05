using UnityEngine;

public class BTCheckPlayerIsInRange : BTNode
{
    private Monster monster;
    private float range = 5f;
    private bool raycast = false;

    public BTCheckPlayerIsInRange(Monster monster, float range, bool raycast = false)
    {
        this.monster = monster;
        this.range = range;
        this.raycast = raycast;
    }

    public override BTNodeState Evaluate()
    {
        Collider2D collider = Physics2D.OverlapCircle(monster.transform.position, range, LayerMask.GetMask("Player"));
        if (collider is null) return BTNodeState.Failure;

        if (raycast)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                origin: monster.transform.position,
                direction: collider.transform.position - monster.transform.position,
                distance: range,
                layerMask: LayerMask.GetMask("Wall", "Player"));

            if (hit.collider != collider) return BTNodeState.Failure;
        }

        return BTNodeState.Success;
    }
}
