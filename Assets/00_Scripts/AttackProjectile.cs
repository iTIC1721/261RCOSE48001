using System.Collections.Generic;
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

    private List<ProjectileEffect> _onHitEffects = new();
    public void SetEffects(IReadOnlyList<ProjectileEffect> effects) 
        => _onHitEffects = new List<ProjectileEffect>(effects);

    public void Initialize(float damage, IAttackable parent, int ricochetCount = 0, int piercingCount = 0, int reflectCount = 0)
    {
        base.Initialize(damage, parent);

        this.ricochetCount = ricochetCount;
        this.piercingCount = piercingCount;
        this.reflectCount = reflectCount;

        ricochet = ricochetCount;
        piercing = piercingCount;
        reflect = reflectCount;
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)direction.normalized * speed * Time.fixedDeltaTime;
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

                Vector2 rayOrigin = (Vector2)transform.position + -direction.normalized * speed * Time.fixedDeltaTime;

                RaycastHit2D hit = Physics2D.Raycast(
                    rayOrigin,
                    direction,
                    speed * Time.fixedDeltaTime * 2f,
                    LayerMask.GetMask("Wall")
                );

                Vector2 closestPoint = collision.ClosestPoint(transform.position);
                Vector2 rawDiff = (Vector2)transform.position - closestPoint;

                Vector2 normal = hit.collider != null
                    ? hit.normal
                    : rawDiff.sqrMagnitude > 0.0001f
                        ? rawDiff.normalized
                        : -direction.normalized;

                Vector2 nextDirection = Vector2.Reflect(direction, normal);

                Vector2 oldDirection = new Vector2(direction.x, direction.y);
                ChangeDirection(nextDirection);
                Log.LogWarning($"[{gameObject.name} Reflect] {oldDirection} -> {direction}, closetPoint: {closestPoint}, normal: {normal}, nextDirection: {nextDirection}");
            }
        }
    }

    public override void StartHitBox()
    {
        hitBox.StartCheckingCollision(HitCallBack);
    }

    public virtual void HitCallBack(Collider2D coll)
    {
        // OnHit 이펙트 먼저 실행
        foreach (var effect in _onHitEffects)
            effect.Execute(this, coll, direction);


        // 투사체 다음 행동 관련
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
