using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;                // Move speed
    public float detectionRange = 10f;      // Player detection range
    public float obstacleCheckDistance = 1f;// Obstacle detection distance
    public LayerMask obstacleLayer;         // Obstacle layer
    private Rigidbody2D rb;
    private Transform targetPlayer;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // For sprite flip
    }

    void Update()
    {
        // Find nearest player (Gray first > then nearest Black/White)
        FindNearestPlayer();

        if (targetPlayer != null)
        {
            FollowPlayer();
        }
    }

    void FindNearestPlayer()
    {
        // 1. Try Gray player first
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerGray");

        // If Gray does not exist or is invalid ¡æ search Black + White
        if (players.Length == 0 || !HasValidPlayer(players))
        {
            players = GameObject.FindGameObjectsWithTag("PlayerBlack");
            GameObject[] whites = GameObject.FindGameObjectsWithTag("PlayerWhite");
            players = CombineArrays(players, whites);
        }

        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject player in players)
        {
            if (!IsValidPlayer(player)) continue; // skip invalid players

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player.transform;
            }
        }

        targetPlayer = nearest;
    }

    void FollowPlayer()
    {
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        // Obstacle check
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // If obstacle detected ¡æ avoid by moving left/right (random simple avoid)
            Vector2 perpDirection = Vector2.Perpendicular(direction);
            if (Random.value > 0.5f)
                direction = perpDirection;
            else
                direction = -perpDirection;
        }

        Vector2 newPos = (Vector2)transform.position + direction * speed * Time.deltaTime;
        rb.MovePosition(newPos);

        // Flip sprite based on player position
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (targetPlayer.position.x < transform.position.x);
        }
    }

    // Combine arrays (Black + White)
    GameObject[] CombineArrays(GameObject[] a1, GameObject[] a2)
    {
        GameObject[] result = new GameObject[a1.Length + a2.Length];
        a1.CopyTo(result, 0);
        a2.CopyTo(result, a1.Length);
        return result;
    }

    // Check if player is valid
    bool IsValidPlayer(GameObject player)
    {
        if (player == null) return false;
        if (!player.activeInHierarchy) return false; // Skip inactive objects
        return true;
    }

    // Check if array has at least one valid player
    bool HasValidPlayer(GameObject[] players)
    {
        foreach (GameObject p in players)
        {
            if (IsValidPlayer(p)) return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        // Debug Raycast line
        if (targetPlayer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (targetPlayer.position - transform.position).normalized * obstacleCheckDistance);
        }
    }
}
