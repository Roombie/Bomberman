using UnityEngine;

public class Bomb : MonoBehaviour
{
    public LayerMask stopBombLayer; // Assign this in the inspector to specify which layers can stop the bomb
    private Rigidbody2D rb;
    private bool isKicked = false; // To prevent multiple kicks
    private bool isPunched = false; // To prevent multiple punches

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Start as kinematic
    }

    #region Bomb Actions
    public void KickBomb(Vector2 direction, float force)
    {
        rb.bodyType = RigidbodyType2D.Dynamic; // Change to dynamic to allow movement
        rb.velocity = Vector2.zero; // Reset any previous velocity
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse); // Apply force in the specified direction
        isKicked = true; // Mark the bomb as kicked
    }

    public void PunchBomb(Vector2 direction, float force)
    {
        isPunched = true;
    }

    private void WrapBomb()
    {
        // Check if the bomb goes off-screen and wrap it around
        if (transform.position.x < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x)
        {
            transform.position = new Vector2(Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x, transform.position.y);
        }
        else if (transform.position.x > Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x)
        {
            transform.position = new Vector2(Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x, transform.position.y);
        }
        else if (transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y)
        {
            transform.position = new Vector2(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y);
        }
        else if (transform.position.y > Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y)
        {
            transform.position = new Vector2(transform.position.x, Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y);
        }
    }

    private void StopBomb()
    {
        // Stop the bomb's movement
        rb.velocity = Vector2.zero;                  // Stop any movement
        rb.bodyType = RigidbodyType2D.Kinematic;    // Set back to kinematic

        // Snap the position to the nearest integer coordinates
        transform.position = new Vector2(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y)
        );

        // Reset the kicked state
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
                Vector2 kickDirection = playerController.LastMoveInput.normalized; // Get the kick direction from the player
                KickBomb(kickDirection, playerController.kickForce); // Use the kick force from the player
            }
        }
    }
    #endregion
}