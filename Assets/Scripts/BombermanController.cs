using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombermanController : MonoBehaviour
{
     [Header("Movement Variables")]
    [SerializeField] private float speed = 5f;        // Player movement speed
    public float kickForce = 10f;                     // Force applied when kicking
    public float punchRange = 0.5f;                   // Range for punching

    private Vector2 moveInput;                        // Current movement input
    private Vector2 lastMoveInput;                    // Last non-zero movement input
    public Vector2 LastMoveInput => lastMoveInput;    // Property to get last move input

    [Header("Ability Flags")]
    private bool hasKick = false;
    private bool hasBoxingGlove = false;
    private bool hasPowerGlove = false;

    public bool HasKick => hasKick;                   // Check if player can kick
    public bool HasBoxingGlove => hasBoxingGlove;     // Check if player can use boxing glove

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private CircleCollider2D circleCollider;
    private BombController bombController;            // Bomb controller for bomb mechanics

    [Header("State Flags")]
    private bool isAlive = true;                      // Check if player is alive
    private bool canMove = true;                      // Check if player can move

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
        if (isAlive && other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            Death();   
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
        rb.velocity = moveInput * speed;

        // Store the last movement direction if the input is non-zero
        if (moveInput != Vector2.zero)
        {
            lastMoveInput = moveInput;
        }
    }
    public void EnableMovement()
    {
        rb.isKinematic = false;                // Re-enable physics interactions
        bombController.enabled = true;         // Re-enable bomb placement
        canMove = true;                        // Allow movement again
    }

    public void DisableMovement()
    {
        rb.velocity = Vector2.zero;            // Stop movement
        rb.isKinematic = true;                 // Disable physics interactions
        bombController.enabled = false;        // Disable bomb placement
        canMove = false;                       // Prevent movement (can be reused for frozen or stunned states)
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

    #region Death
    public void RespawnPlayer()
    {
        isAlive = true;         // Mark the player as alive
        circleCollider.enabled = true;
        GameManager.instance.currentState = GameState.Playing;
        EnableMovement();
    }

    private void Death()
    {
        DisableMovement();       // Call DisableMovement when the player dies
        isAlive = false;         // Mark the player as dead
        circleCollider.enabled = false;
        // Start the coroutine to wait for the animation to finish
        StartCoroutine(HandleDeathAnimation());
    }

    private IEnumerator HandleDeathAnimation()
    {
        animator.SetTrigger("death");
        // Wait until the death animation is playing
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("bomberman_death") &&
                                          animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        OnDeathEnd();  // Call OnDeathEnd after the animation
    }

    private void OnDeathEnd()
    {
        gameObject.SetActive(false);  // Deactivate player after death animation ends
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
