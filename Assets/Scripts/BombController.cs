using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    #region Variables
    [Header("Bomb")]
    public GameObject bombPrefab;
    public float bombFuseTime = 4f; // Total time before the bomb explodes
    public int bombAmount = 1;
    private int bombsRemaining;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;
    public int maxExplosionRadius = 8;

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab;
    public LayerMask itemLayerMask;
    public Destructible itemDestructiblePrefab;

    // Track exploded bombs to prevent multiple triggers
    private readonly HashSet<GameObject> explodedBombs = new();
    #endregion

    private void OnEnable()
    {
        bombsRemaining = bombAmount; // Initialize available bombs
    }

    #region Input
    public void OnPlaceBomb(InputAction.CallbackContext context)
    {
        // Check if the bomb placement action was triggered
        if (context.performed && bombsRemaining > 0)
        {
            StartCoroutine(PlaceBomb());
        }
    }
    #endregion

    #region Bomb Placement
    private IEnumerator PlaceBomb()
    {
        // Get the initial position to place the bomb
        Vector2 initialPosition = new(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));

        // Instantiate the bomb
        GameObject bomb = Instantiate(bombPrefab, initialPosition, Quaternion.identity);
        bombsRemaining--; // Decrement the bombs available

        yield return new WaitForSeconds(bombFuseTime);

        // Check if the bomb still exists before accessing its position
        if (bomb != null)
        {
            // Get the bomb's current position, then round it (in case it was moved)
            Vector2 bombCurrentPosition = new(Mathf.Round(bomb.transform.position.x), Mathf.Round(bomb.transform.position.y));

            // Trigger explosion at the bomb's rounded current position
            TriggerExplosion(bombCurrentPosition);

            Destroy(bomb); // Destroy the bomb object
            bombsRemaining++; // Restore the bomb count
        }
    }
    #endregion

    #region Explosion Handling
    private void TriggerExplosion(Vector2 position)
    {
        // Instantiate the explosion at the bomb position
        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(explosion.start);
        explosion.DestroyAfter(explosionDuration); // Handle explosion duration

        // Create explosions in all directions
        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);
    }

    private void Explode(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0) return; // Stop recursion if length is 0

        position += direction; // Move position in the specified direction

        // Check for objects on the "Bomb" layer
        Collider2D bombCollider = Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, LayerMask.GetMask("Bomb"));

        if (bombCollider != null && !explodedBombs.Contains(bombCollider.gameObject))
        {
            // Add the bomb to the exploded set to avoid repeating the explosion
            explodedBombs.Add(bombCollider.gameObject);

            // Trigger the bomb explosion with a delay to prevent recursion overload
            StartCoroutine(DelayedExplosion(bombCollider.gameObject));
        }

        // Check for destructible objects in the item layer
        Collider2D[] itemColliders = Physics2D.OverlapBoxAll(position, Vector2.one / 2f, 0f, itemLayerMask);
        foreach (var collider in itemColliders)
        {
            // Optionally instantiate a destructible prefab or play an effect here
            if (itemDestructiblePrefab != null)
            {
                Instantiate(itemDestructiblePrefab, collider.transform.position, Quaternion.identity);
            }

            Destroy(collider.gameObject); // Destroy the item gameobject
        }

        // Check for destructible objects
        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            ClearDestructible(position); // Clear destructible tiles
            return;
        }

        // Instantiate explosion effect
        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
        explosion.SetDirection(direction);
        explosion.DestroyAfter(explosionDuration);

        // Recursively call to create longer explosions
        Explode(position, direction, length - 1);
    }

    // Coroutine to handle delayed bomb explosions
    private IEnumerator DelayedExplosion(GameObject bomb)
    {
        yield return new WaitForSeconds(0.1f); // Small delay to avoid recursion overload
        Vector2 bombPosition = new(Mathf.Round(bomb.transform.position.x), Mathf.Round(bomb.transform.position.y));
        TriggerExplosion(bombPosition); // Trigger explosion at the bomb's position
        Destroy(bomb); // Destroy the bomb object
        bombsRemaining++;
    }

    private void ClearDestructible(Vector2 position)
    {
        // Clear destructible tiles
        Vector3Int cell = destructibleTiles.WorldToCell(position);
        TileBase tile = destructibleTiles.GetTile(cell);

        if (tile != null)
        {
            Instantiate(destructiblePrefab, position, Quaternion.identity); // Create destructible prefab
            destructibleTiles.SetTile(cell, null); // Remove the tile from the tilemap
        }
    }
    #endregion

    #region Bomb Management
    public void AddBomb()
    {
        bombAmount++; // Increase bomb count
        bombsRemaining++; // Restore bombs remaining
    }

    public void IncreaseBlastRadius()
    {
        if (explosionRadius <= maxExplosionRadius)
        {
            explosionRadius += 1; // Increase the blast radius
            Debug.Log("Blast radius increased to: " + explosionRadius);
        }
        else
        {
            Debug.Log("Blast radius is already at the maximum: " + maxExplosionRadius);
        }
    }

    public void MaximizeBlastRadius()
    {
        explosionRadius = maxExplosionRadius; // Set blast radius to the maximum
        Debug.Log("Blast radius maximized to: " + maxExplosionRadius);
    }
    #endregion

    #region Collision Handling
    private void OnTriggerExit2D(Collider2D other)
    {
        // Handle bomb interactions if necessary
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            other.isTrigger = false; // Reset trigger state if needed
        }
    }
    #endregion
}
