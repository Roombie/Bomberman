using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public LayerMask stopBombLayer; // Layers that can stop the bomb
    private Rigidbody2D rb;
    private bool isKicked = false; // Prevents multiple kicks
    private bool isPunched = false; // Prevents multiple punches

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Start as kinematic
    }

    private void Update()
    {
        WrapBomb(); // Check wrapping every frame
    }

    #region Bomb Actions
    public void KickBomb(Vector2 direction, float force)
    {
        if (isKicked) return; // Prevent multiple kicks

        rb.bodyType = RigidbodyType2D.Dynamic; // Enable movement
        rb.velocity = Vector2.zero; // Reset previous velocity
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse); // Apply kick force
        isKicked = true; // Mark the bomb as kicked
    }

    public void PunchBomb(Vector2 direction, float force)
    {
        if (isPunched) return; // Prevent multiple punches

        rb.bodyType = RigidbodyType2D.Dynamic; // Enable movement
        rb.velocity = Vector2.zero; // Reset previous velocity
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse); // Apply punch force
        isPunched = true; // Mark the bomb as punched
    }

    private void WrapBomb()
    {
        // Check if the bomb goes off-screen and wrap it around
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        Vector3 newPos = transform.position;

        if (viewportPos.x < 0) newPos.x = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        else if (viewportPos.x > 1) newPos.x = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

        if (viewportPos.y < 0) newPos.y = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        else if (viewportPos.y > 1) newPos.y = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;

        transform.position = newPos;
    }

    private void StopBomb()
    {
        // Stop the bomb's movement
        rb.velocity = Vector2.zero;                  // Stop any movement
        rb.bodyType = RigidbodyType2D.Kinematic;    // Set back to kinematic

        // Snap position to nearest integer
        transform.position = new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y)
        );

        // Reset states
        isKicked = false;
        isPunched = false;
    }
    #endregion

    #region Collision Handling
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the bomb collides with any of the specified layers
        if (((1 << collision.gameObject.layer) & stopBombLayer) != 0)
        {
            StopBomb();
        }

        // Check if the bomb collides with the player
        if (collision.gameObject.CompareTag("Player") && !isKicked)
        {
            var playerController = collision.gameObject.GetComponent<BombermanController>();
            if (playerController != null && playerController.HasKick)
            {
                Vector2 kickDirection = playerController.LastMoveInput.normalized; // Get kick direction
                KickBomb(kickDirection, playerController.kickForce); // Apply player's kick force
            }
        }
    }
    #endregion
}
