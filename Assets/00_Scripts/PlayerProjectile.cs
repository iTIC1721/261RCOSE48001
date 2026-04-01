using UnityEngine;

public class PlayerProjectile : PlayerAttackObject
{
    public SpriteRenderer sprite;

    [HideInInspector] public Vector2 direction;
    [HideInInspector] public float speed = 10;

    private void Update()
    {
        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isInitialized) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Return(CallBack);
        }
    }

    public override void StartHitBox()
    {
        hitBox.StartCheckingCollision(KillCallBack);
    }

    public virtual void KillCallBack(Collider2D _)
    {
        Return(CallBack);
    }
}
