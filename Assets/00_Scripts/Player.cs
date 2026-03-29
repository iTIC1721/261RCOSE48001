using UnityEngine;

public class Player : MonoBehaviour, IEntity
{
    public bool enableMove = false;
    public bool enableAttack = false;

    private bool canControl = true;

    private Animator animator;

    private bool isMoving = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        CheckCanControl();

        if (enableMove && canControl) OnMove();
        if (enableAttack && canControl) OnAttack();
    }

    private void OnMove()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector3 moveVector = new Vector3(moveX, moveY, 0);

        transform.Translate(moveVector.normalized * Time.deltaTime * 5f);

        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveY) > 0.01f)
        {
            isMoving = true;
            animator.SetBool("1_Move", true);

            if (moveX > 0)
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
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Attack(0);
        }
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
    }

    public void Attack(float damage)
    {
        animator.SetTrigger("2_Attack");
    }

    public void GetDamaged(float damage)
    {
        animator.SetTrigger("3_Damaged");
    }

    public void Die()
    {
        animator.SetTrigger("4_Death");
    }
}
