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

    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private InputActionReference moveActionReference;

    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        CheckCanControl();

        OnMove();
        // TODO: OnAttack 구현하기
    }

    public void Intialize()
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

            if (moveInput.x > 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void MoveStop()
    {
        isMoving = false;
        animator.SetBool("1_Move", false);
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
