using System.Threading;
using UnityEngine;

public class AttackSpinner : AttackObject
{
    [Header("Setting")]
    public float spinSpeed = 180f;
    public float radius = 1f;

    private Transform center;
    private float angle = 0;

    public override void Initialize(float damage, IAttackable parent)
    {
        base.Initialize(damage, parent);
        center = parent.Transform;
    }

    public override void StartHitBox()
    {
        hitBox.StartCheckingCollision(null);
    }

    private void FixedUpdate()
    {
        if (center == null) return;

        angle += spinSpeed * Time.fixedDeltaTime;
        if (angle > 360)
        {
            angle %= 360f;

            hitBox.StopCheckingCollision();
            StartHitBox();
        }

        Vector2 pos = (Vector2)center.position + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
        transform.position = pos;
    }
}
