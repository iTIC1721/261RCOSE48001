using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{
    public static Player Instance { get; private set; }

    [Header("Control")]
    public bool enableMove = false;
    public bool enableAttack = false;

    [Header("Setting")]
    public bool invulnerable = false;
    public float moveSpeed = 10f;

    [Space]
    public LayerMask detectMask;
    public float detectRange = 5;

    [Space]
    public float attackDelay = 1;
    public float attackPositionOffset = 0.2f;

    [Header("FX")]
    public string damageTMPName;

    [Header("Ref")]
    public Transform spriteRoot;
    public FloatingJoystick joystick;
    public InputActionReference moveActionReference;

    public bool CanControl { get; private set; } = true;

    private Animator animator;

    [HideInInspector] public float lastAttackTime = 0;

    [HideInInspector] public Monster target = null;

    public bool IsDied { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        CheckCanControl();
        SetTarget();
    }

    public override void Initialize()
    {
        hp = maxHp;
        IsDied = false;
        animator.SetBool("isDeath", false);
    }

    private void SetTarget()
    {
        if (!enableMove || !CanControl) return;

        Monster beforeTarget = target;
        target = FindNearestMonster();

        if (beforeTarget != target)
        {
            beforeTarget?.SetTargetEffectEnabled(false);
            target?.SetTargetEffectEnabled(true);
        }
    }

    private Monster FindNearestMonster()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, detectRange, detectMask);

        if (cols.Length == 0) return null;

        Monster nearest = null;
        float nearestDist = float.MaxValue;
        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].TryGetComponent<Monster>(out var monster)) continue;

            if (monster.IsDied) continue;   // 죽은 몬스터면 패스

            float dist = (transform.position - cols[i].transform.position).sqrMagnitude;
            if (dist < nearestDist)
            {
                // 벽에 가려지지 않았는지 체크
                RaycastHit2D hit = Physics2D.Raycast(GetAttackPosition(), cols[i].transform.position - transform.position, float.PositiveInfinity, LayerMask.GetMask("Wall", "Monster"));
                if (hit.collider == null || !hit.collider.TryGetComponent<Monster>(out _)) continue;

                nearestDist = dist;
                nearest = monster;
            }
        }

        return nearest;
    }

    private void CheckCanControl()
    {
        if (IsDied)
        {
            CanControl = false;
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("DAMAGED"))
        {
            float animTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (animTime == 0)
            {
                // 플레이 중이 아님
                CanControl = true;
            }
            if (animTime > 0 && animTime < 1.0f)
            {
                // 애니메이션 플레이 중
                CanControl = false;
            }
            else if (animTime >= 1.0f)
            {
                // 애니메이션 종료
                CanControl = true;
            }
        }
        else
        {
            CanControl = true;
        }
    }

    public override void Attack()
    {
        animator.SetTrigger("2_Attack");
        lastAttackTime = Time.time;
    }

    public override void GetDamaged(params DamageInfo[] damageInfos)
    {
        animator.SetTrigger("3_Damaged");

        foreach (DamageInfo damageInfo in damageInfos)
        {
            float damage = damageInfo.damage;

            if (!invulnerable)
            {
                hp -= damage;
                if (hp <= 0)
                {
                    Die();
                    break;
                }
            }

            // damageTMP 출력
            if (BaseCanvas.Instance != null && BaseCanvas.Instance.damageLayer != null)
            {
                if (damageTMPName.Length > 0)
                {
                    var damageTMP = MANAGER.Pool.PoolingObj(damageTMPName).Get((value) => {
                        value.GetComponent<DamageTMP>().Initialize(BaseCanvas.Instance.damageLayer, Transform, Vector3.zero, damage, Color.white);
                    });
                }
            }
            else
            {
                Log.LogWarning("BaseCanvas 또는 damageLayer가 없습니다.");
            }
        }

        OnDamaged?.Invoke(hp, maxHp);
    }

    public override void Die()
    {
        if (!animator.GetBool("isDeath"))
        {
            animator.SetBool("isDeath", true);
            animator.SetTrigger("4_Death");
        }

        // TODO: 플레이어 사망 시 이벤트
        IsDied = true;
    }

    public void SpawnAttackObject()
    {
        if (target == null) return;

        Vector2 direction = target.transform.position - transform.position;

        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(GetAttackPosition(), value => {
            AttackProjectile p = value.GetComponent<AttackProjectile>();
            p.Initialize(10, this);

            value.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
            p.direction = direction;
            p.speed = 10;
        });
    }

    private Vector3 GetAttackPosition()
    {
        return transform.position + Vector3.up * attackPositionOffset;
    }
}
