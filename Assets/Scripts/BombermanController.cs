using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombermanController : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] private float speed = 5f;
    public float kickForce = 10f;
    public float punchRange = 0.5f;

    private Vector2 moveInput;
    private Vector2 lastMoveInput;
    public Vector2 LastMoveInput => lastMoveInput;

    [Header("Ability Flags")]
    private bool hasKick = false;
    private bool hasBoxingGlove = false;
    private bool hasPowerGlove = false;

    public bool HasKick => hasKick;
    public bool HasBoxingGlove => hasBoxingGlove;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private CircleCollider2D circleCollider;
    private BombController bombController;

    [Header("State Flags")]
    private bool isAlive = true;
    private bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();
        bombController = GetComponent<BombController>();
    }

    void Update()
    {
        if (GameManager.instance.currentState == GameState.Playing && isAlive && canMove)
        {
            UpdateAnimations();
            Movement();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAlive)
        {
            if (other.gameObject.CompareTag("Win"))
            {
                WinState();
            }
            
            if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
            {
                Death();
            }
        }
    }

    #region Item Reference
    public void IncreaseSpeed()
    {
        speed += 1f;
        Debug.Log("Speed increased! New speed: " + speed);
    }
    #endregion

    #region Animations
    private void UpdateAnimations()
    {
        animator.SetFloat("speed", moveInput.magnitude);

        // If there is no movement, keep the last direction
        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("horizontal", moveInput.x);
            animator.SetFloat("vertical", moveInput.y);
        }
        else
        {
            animator.SetFloat("horizontal", lastMoveInput.x);
            animator.SetFloat("vertical", lastMoveInput.y);
        }
    }
    #endregion

    #region Movement
    private void Movement()
    {
        Vector2 normalizedInput = moveInput.normalized;
        rb.velocity = normalizedInput * speed;

        float horizontalAxis = normalizedInput.x;
        float verticalAxis = normalizedInput.y;

        // Direction control
        if (Mathf.Abs(horizontalAxis) > Mathf.Abs(verticalAxis))
        {
            float direction = horizontalAxis > 0 ? 1 : -1;
            rb.MovePosition(new Vector2(transform.position.x + direction * Time.deltaTime * speed, transform.position.y));
        }
        else if (Mathf.Abs(horizontalAxis) < Mathf.Abs(verticalAxis))
        {
            float direction = verticalAxis > 0 ? 1 : -1;
            rb.MovePosition(new Vector2(transform.position.x, transform.position.y + direction * Time.deltaTime * speed));
        }

        // Store last movement direction
        if (moveInput != Vector2.zero)
        {
            lastMoveInput = normalizedInput;
        }
    }
    public void EnableMovement()
    {
        rb.isKinematic = false;
        bombController.enabled = true;
        canMove = true;
    }

    public void DisableMovement()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        bombController.enabled = false;
        canMove = false;
    }
    #endregion

    #region Abilities
    public void EnableKick()
    {
        hasKick = true;
        Debug.Log($"Can you kick? {hasKick}");
    }

    public void EnableBoxingGlove()
    {
        hasBoxingGlove = true;
        Debug.Log($"Can you use boxing glove? {hasBoxingGlove}");
    }

    public void EnablePowerGlove()
    {
        hasPowerGlove = true;
        Debug.Log($"Can you use power glove? {hasPowerGlove}");
    }
    #endregion

    #region Win
    public void WinState()
    {
        DisableMovement();
        circleCollider.enabled = false;
        animator.SetTrigger("win");
        GameManager.instance.PlayerWon();
        Debug.Log("You Win!");
    }
    #endregion

    #region Death
    public void RespawnPlayer()
    {
        isAlive = true;
        circleCollider.enabled = true;
        GameManager.instance.currentState = GameState.Playing;
        EnableMovement();
    }

    private void Death()
    {
        DisableMovement();
        isAlive = false;
        circleCollider.enabled = false;
        StartCoroutine(HandleDeathAnimation());
    }

    private IEnumerator HandleDeathAnimation()
    {
        animator.SetTrigger("death");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("bomberman_death") &&
                                          animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        OnDeathEnd();
    }

    private void OnDeathEnd()
    {
        gameObject.SetActive(false);
        GameManager.instance.PlayerDied();
    }
    #endregion

    #region Input
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.instance.TogglePauseGame();
        }
    }
    #endregion
}
