using UnityEngine;

public class AttackProjectile : AttackObject
{
    public SpriteRenderer sprite;

    [Header("Setting")]
    public int ricochetCount = 0;
    public int piercingCount = 0;
    public int reflectCount = 0;

    private int ricochet = 0;
    private int piercing = 0;
    private int reflect = 0;

    [HideInInspector] public Vector2 direction;
    [HideInInspector] public float speed = 10;

    public void Initialize(float damage, IAttackable parent, int ricochetCount = 0, int piercingCount = 0, int reflectCount = 0)
    {
        base.Initialize(damage, parent);

        if (ricochetCount > 0 || piercingCount > 0 || reflectCount > 0)
        {
            this.ricochetCount = ricochetCount;
            this.piercingCount = piercingCount;
            this.reflectCount = reflectCount;
        }

        InitializeSetting();
    }

    private void InitializeSetting()
    {
        ricochet = ricochetCount;
        piercing = piercingCount;
        reflect = reflectCount;
    }

    private void Update()
    {
        transform.position += (Vector3)direction.normalized * speed * Time.deltaTime;
    }

    private void ChangeDirection(Vector2 newDirection)
    {
        direction = newDirection;
        transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isInitialized) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if (reflect <= 0)
            {
                Return(CallBack);
            }
            else
            {
                reflect--;
                hitBox.Damage = hitBox.Damage * 0.5f;

                Vector2 closestPoint = collision.ClosestPoint(transform.position);
                Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;
                Vector2 nextDirection = Vector2.Reflect(direction, normal);

                ChangeDirection(nextDirection);
            }
        }
    }

    public override void StartHitBox()
    {
        hitBox.StartCheckingCollision(HitCallBack);
    }

    public virtual void HitCallBack(Collider2D coll)
    {
        if (ricochet > 0)
        {
            ricochet--;

            if (coll.TryGetComponent<Entity>(out var hit))
            {
                Transform nearest = GetNearestEntityFromHitEntity(hit);
                if (nearest == null)
                {
                    Return(CallBack);
                    return;
                }

                ChangeDirection((nearest.position - transform.position).normalized);
                hitBox.Damage = hitBox.Damage * 0.7f;
            }
            else
            {
                Return(CallBack);
                return;
            }
        }
        else if (ricochetCount <= 0 && piercing > 0)
        {
            piercing--;
            hitBox.Damage = hitBox.Damage * 0.67f;
        }
        else
        {
            Return(CallBack);
            return;
        }
    }

    private Transform GetNearestEntityFromHitEntity(Entity hit)
    {
        float detectRange = 10;

        Transform nearest = null;
        float minDistSqr = float.MaxValue;

        var colls = Physics2D.OverlapCircleAll(transform.position, detectRange, hitBox.mask);
        foreach (var coll in colls)
        {
            if (coll.GetComponent<Entity>() == hit) continue;

            float distSqr = (coll.transform.position - transform.position).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                nearest = coll.transform;
                minDistSqr = distSqr;
            }
        }

        return nearest;
    }
}
