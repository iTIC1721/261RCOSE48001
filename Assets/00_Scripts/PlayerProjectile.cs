using UnityEngine;

public class PlayerProjectile : PoolObject
{
    [Min(0)] public float lifeTime = 10;

    [SerializeField] HitBox2D hitBox;
    [SerializeField] SpriteRenderer sprite;

    private Vector2 direction;
    private float speed = 10;

    public void Initialize(Vector2 direction, float speed, float damage, IAttackable parent)
    {
        transform.position = parent.Transform.position;
        transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
        this.direction = direction;
        this.speed = speed;

        hitBox.damage = damage;
        hitBox.parent.Value = parent;
        hitBox.StartCheckingCollision();

        Return(lifeTime, CallBack);
    }

    private void Update()
    {
        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
    }

    private void KillCallBack(Collider2D _)
    {
        Return(CallBack);
    }

    private void CallBack(GameObject _)
    {
        hitBox.StopCheckingCollision();
    }
}
