using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IEntity
{
    public Transform Transform => this.transform;

    [Header("Control")]
    public bool enableMove = false;
    public bool enableAttack = false;

    [Header("Setting")]
    public float moveSpeed = 10f;

    [Space]
    public LayerMask detectMask;
    public float detectRange = 5;

    [Space]
    public float attackDelay = 1;
    public float attackPositionOffset = 0.2f;

    [Header("Ref")]
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private InputActionReference moveActionReference;

    private bool canControl = true;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;

    private int playerDir = 1;

    private float lastAttackTime = 0;

    private Monster target = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        CheckCanControl();

        if (joystick != null)
        {
            SetTarget();
            OnMove();
            Rotate();
            OnAttack();
        }
    }

    public void Initialize()
    {
        animator.SetBool("isDeath", false);
    }

    private void OnMove()
    {
        if (!joystick.IsMoving)
        {
            if (isMoving) MoveStop();
            
            return;
        }

        Vector2 joystickDir = joystick.Direction;
        moveInput = (joystickDir.sqrMagnitude > 0.01f) ? joystickDir : moveActionReference.action.ReadValue<Vector2>();

        if (enableMove && canControl) 
            Move();
    }

    private void Move()
    {
        //transform.Translate(moveInput.normalized * Time.deltaTime * 5f);
        //rb.MovePosition(rb.position + moveInput.normalized * Time.deltaTime * moveSpeed);
        rb.linearVelocity = moveInput.normalized * moveSpeed;

        if (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f)
        {
            isMoving = true;
            animator.SetBool("1_Move", true);
        }
    }

    private void MoveStop()
    {
        isMoving = false;
        animator.SetBool("1_Move", false); 
        rb.linearVelocity = Vector2.zero;
    }

    private void Rotate()
    {
        // 타겟 존재 시 플레이어 방향 타겟에게 고정
        if (target != null)
        {
            if (target.transform.position.x >= transform.position.x)
            {
                playerDir = -1;
            }
            else
            {
                playerDir = 1;
            }
        }
        else if (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f)
        {
            if (moveInput.x > 0)
            {
                playerDir = -1;
            }
            else
            {
                playerDir = 1;
            }
        }

        transform.localScale = new Vector3(playerDir, 1, 1);
    }

    private void SetTarget()
    {
        if (!enableMove || !canControl) return;

        Monster beforeTarget = target;
        target = FindNearestMonster();

        if (beforeTarget != target)
        {
            beforeTarget?.DisableTargetEffect();
            target?.EnableTargetEffect();
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

    private void OnAttack()
    {
        if (!canControl | !enableAttack) return;

        if (isMoving) return;

        if (target == null) return;

        if (Time.time - lastAttackTime >= attackDelay)
            Attack();
    }

    private void CheckCanControl()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("DAMAGED"))
        {
            float animTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (animTime == 0)
            {
                // 플레이 중이 아님
                canControl = true;
            }
            if (animTime > 0 && animTime < 1.0f)
            {
                // 애니메이션 플레이 중
                canControl = false;
            }
            else if (animTime >= 1.0f)
            {
                // 애니메이션 종료
                canControl = true;
            }
        }
        else
        {
            canControl = true;
        }
    }

    public void Attack()
    {
        animator.SetTrigger("2_Attack");
        lastAttackTime = Time.time;
    }

    public void GetDamaged(params DamageInfo[] damageInfos)
    {
        animator.SetTrigger("3_Damaged");
    }

    public void Die()
    {
        if (!animator.GetBool("isDeath"))
        {
            animator.SetBool("isDeath", true);
            animator.SetTrigger("4_Death");
        }
    }

    public void SpawnAttackObject()
    {
        if (target == null) return;

        Vector2 direction = target.transform.position - transform.position;

        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(GetAttackPosition(), value => {
            PlayerProjectile p = value.GetComponent<PlayerProjectile>();
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
