using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    public FloatingJoystick joystick;
    public InputActionReference moveActionReference;

    private Player player;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput = Vector2.zero;
    private bool isMoving = false;

    private int playerDir = 1;

    private void Awake()
    {
        player = GetComponent<Player>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (joystick != null)
        {
            Move();
            Rotate();
            if (!isMoving) Attack();
        }
    }

    private void Move()
    {
        if (!joystick.IsMoving)
        {
            if (isMoving) MoveStop();

            return;
        }

        Vector2 joystickDir = joystick.Direction;
        moveInput = (joystickDir.sqrMagnitude > 0.01f) ? joystickDir : moveActionReference.action.ReadValue<Vector2>();

        if (player.enableMove && player.CanControl)
            MoveStart();
    }

    private void MoveStart()
    {
        rb.linearVelocity = moveInput.normalized * player.moveSpeed;

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
        if (player.target != null)
        {
            if (player.target.transform.position.x >= transform.position.x)
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

        player.spriteRoot.localScale = new Vector3(playerDir, 1, 1);
    }

    private void Attack()
    {
        if (!player.CanControl | !player.enableAttack) return;
        if (player.target == null) return;

        if (Time.time - player.lastAttackTime >= player.AttackDelay)
            player.Attack();
    }
}
