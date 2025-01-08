using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AbilityType
{
    Kick,
    BoxingGlove,
    PowerGlove
}

public class BombermanController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private int playerID;
    public int PlayerID => playerID;

    [Header("Movement Variables")]
    [SerializeField] private float speed = 5f;
    public float kickForce = 10f;
    public float punchRange = 0.5f;

    private Vector2 moveInput;
    private Vector2 lastMoveInput;
    public Vector2 LastMoveInput => lastMoveInput;

    [Header("Ability Flags")]
    private HashSet<AbilityType> abilities = new HashSet<AbilityType>();
    public bool HasAbility(AbilityType ability) => abilities.Contains(ability);

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private CircleCollider2D circleCollider;
    private BombController bombController;

    [Header("State Flags")]
    private bool isAlive = true;
    private bool canMove = true;

    private int explosionLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>() ?? throw new MissingComponentException("Missing Rigidbody2D component.");
        animator = GetComponent<Animator>() ?? throw new MissingComponentException("Missing Animator component.");
        circleCollider = GetComponent<CircleCollider2D>() ?? throw new MissingComponentException("Missing CircleCollider2D component.");
        bombController = GetComponent<BombController>();
        explosionLayer = LayerMask.NameToLayer("Explosion");
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
                transform.position = other.transform.position + new Vector3(0, 0.5f, 0);
                WinState();
            }

            if (other.gameObject.layer == explosionLayer)
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

    public void EnableAbility(AbilityType ability)
    {
        abilities.Add(ability);
        Debug.Log($"Enabled {ability}");
    }
    #endregion

    #region Animations
    private void UpdateAnimations()
    {
        animator.SetFloat("speed", moveInput.magnitude);

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
        rb.velocity = moveInput.normalized * speed;

        if (moveInput != Vector2.zero)
        {
            lastMoveInput = moveInput.normalized;
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

    #region Multiplayer
    public void SetPlayerID(int id)
    {
        playerID = id;
    }
    #endregion

    #region Win
    public void WinState()
    {
        DisableMovement();
        circleCollider.enabled = false;
        animator.SetTrigger("win");
        GameManager.instance.PlayerWon(gameObject);
        Debug.Log("You Win!");
    }
    #endregion

    #region Death
    public void RespawnPlayer()
    {
        Debug.Log($"{name} is respawning.");
        isAlive = true;
        circleCollider.enabled = true;
        GameManager.instance.currentState = GameState.Playing;
        EnableMovement();
    }

    private void Death()
    {
        Debug.Log($"{name} has died.");
        DisableMovement();
        isAlive = false;
        circleCollider.enabled = false;
        StartCoroutine(HandleDeathAnimation());
    }

    private IEnumerator HandleDeathAnimation()
    {
        animator.SetTrigger("death");
        yield return new WaitUntil(() => IsAnimationComplete("bomberman_death"));
        OnDeathEnd();
    }

    private bool IsAnimationComplete(string animationName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1f;
    }

    private void OnDeathEnd()
    {
        gameObject.SetActive(false);
        GameManager.instance.PlayerDied(gameObject);
    }
    #endregion

    #region Input
    public void OnMove(InputAction.CallbackContext context)
    {
        if (GameManager.instance.CurrentPlayerID == playerID)
        {
            moveInput = context.ReadValue<Vector2>();
        }
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