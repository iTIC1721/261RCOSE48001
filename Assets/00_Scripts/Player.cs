using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Player : MonoBehaviour, IEntity
{
    public Transform Transform => this.transform;

    public bool enableMove = false;
    public bool enableAttack = false;

    private bool canControl = true;

    private Animator animator;

    private PlayerInput playerInput;
    private InputActionMap playerActionMap;
    private InputAction moveAction;
    private InputAction attackAction;

    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerInput = GetComponent<PlayerInput>();

        playerActionMap = playerInput.actions.FindActionMap("PlayerActions");
        moveAction = playerActionMap.FindAction("Move");
        attackAction = playerActionMap.FindAction("Attack");

        moveAction.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        moveAction.canceled += ctx => OnMove(ctx.ReadValue<Vector2>());

        attackAction.performed += ctx => OnAttack();
    }

    private void Update()
    {
        CheckCanControl();

        if (enableMove && canControl) Move();
    }

    public void Intialize()
    {
        animator.SetBool("isDeath", false);
    }

    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    private void Move()
    {
        transform.Translate(moveInput.normalized * Time.deltaTime * 5f);

        if (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f)
        {
            isMoving = true;
            animator.SetBool("1_Move", true);

            if (moveInput.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            isMoving = false;
            animator.SetBool("1_Move", false);
        }
    }

    private void OnAttack()
    {
        if (!enableAttack || !canControl) return;
        
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
}
