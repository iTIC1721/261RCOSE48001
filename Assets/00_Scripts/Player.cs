using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IEntity
{
    public Transform Transform => this.transform;

    [Header("Control")]
    public bool enableMove = false;
    public bool enableAttack = false;

    [Header("Setting")]
    public LayerMask detectMask;
    public float detectRange = 5;

    [Space]
    public float attackDelay = 1;

    [Header("Ref")]
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private InputActionReference moveActionReference;

    private bool canControl = true;

    private Animator animator;

    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;

    private float lastAttackTime = 0;

    private Monster target = null;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        CheckCanControl();

        SetTarget();
        Rotate();
        OnMove();
        OnAttack();
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
        transform.Translate(moveInput.normalized * Time.deltaTime * 5f);

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
    }

    private void Rotate()
    {
        int playerDir = 1;

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

            float dist = Vector2.Distance(transform.position, cols[i].transform.position);
            if (dist < nearestDist)
            {
                // 벽에 가려지지 않았는지 체크
                RaycastHit2D hit = Physics2D.Raycast(transform.position, cols[i].transform.position - transform.position, float.PositiveInfinity, LayerMask.GetMask("Wall", "Monster"));
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

        if (Time.time - lastAttackTime >= attackDelay)
            Attack();
    }

    private void CheckCanControl()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("ATTACK") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("DAMAGED"))
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

        FireProjectile();
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

    public void FireProjectile()
    {
        Vector2 direction = Vector2.right;
        if (target != null)
        {
            direction = target.transform.position - transform.position;
        }
        else if (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f)
        {
            direction = moveInput;
        }
        else
        {
            if (transform.localScale.x < 0) direction = -Vector2.right;
            else direction = Vector2.right;
        }

        MANAGER.Pool.PoolingObj("PlayerProjectile").Get(value => {
            value.GetComponent<PlayerProjectile>().Initialize(direction, 10, 10, this);
        });
    }
}
