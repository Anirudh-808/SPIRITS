using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float stopDistance = 1.2f;
    public float detectionRange = 100f;
    public LayerMask obstacleLayer;
    
    // Improved tracking parameters
    public float lostPlayerMemoryTime = 5f; // Longer memory
    public float updateLastKnownPositionInterval = 0.1f; // Much more frequent updates (10x per second)
    public float minDistanceToUpdateLastKnown = 0.2f; // Smaller movement threshold for smoother tracking
    
    // New parameters for obstacle navigation
    public float obstacleAvoidanceRadius = 1.5f;
    public int obstacleCheckPoints = 8;
    public LayerMask avoidanceLayer;

    private Rigidbody2D rb;
    private List<Vector2> lastKnownPositions = new List<Vector2>(); // Store multiple positions for smooth path
    public int maxStoredPositions = 10; // How many positions to remember
    
    private bool canSeePlayer = false;
    private Transform playerTransform;
    
    private float lastUpdateTime;
    private Vector2 lastRecordedPlayerPosition;
    private float playerLostTime;
    
    // For debugging
    private Vector2 currentTargetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        FindPlayer();
        lastRecordedPlayerPosition = Vector2.zero;
        lastKnownPositions.Clear();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) 
            {
                rb.linearVelocity = Vector2.zero; 
                return; 
            }
        }

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Raycast logic - Check multiple points for better obstacle detection
        canSeePlayer = CanSeePlayer();
        
        if (canSeePlayer)
        {
            // Update player position history frequently
            UpdatePlayerPositionHistory(playerTransform.position);
            playerLostTime = Time.time; // Reset lost timer but keep memory alive
        }
        else
        {
            // Even if we can't see player, check if we have recent history
            if (lastKnownPositions.Count > 0)
            {
                // Keep memory alive for lostPlayerMemoryTime seconds
                if (Time.time - playerLostTime > lostPlayerMemoryTime)
                {
                    // Memory expired, clear history
                    lastKnownPositions.Clear();
                }
            }
        }

        MoveLogic(distanceToPlayer, directionToPlayer);
    }

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;
        
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Cast ray to player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        
        // Debug ray
        Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, 
                      hit.collider == null ? Color.green : Color.red);
        
        // Can see player if no obstacle hit
        return hit.collider == null;
    }

    void UpdatePlayerPositionHistory(Vector2 playerPos)
    {
        // Only add if position changed significantly
        if (lastRecordedPlayerPosition == Vector2.zero || 
            Vector2.Distance(lastRecordedPlayerPosition, playerPos) > minDistanceToUpdateLastKnown)
        {
            // Add to history list
            lastKnownPositions.Add(playerPos);
            lastRecordedPlayerPosition = playerPos;
            
            // Keep list at max size
            if (lastKnownPositions.Count > maxStoredPositions)
            {
                lastKnownPositions.RemoveAt(0);
            }
            
            Debug.Log($"Added position {lastKnownPositions.Count}: {playerPos}");
        }
    }

    void MoveLogic(float distance, Vector2 direction)
    {
        // Don't move towards origin
        if (playerTransform != null && playerTransform.position == Vector3.zero && distance > 20f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (canSeePlayer && distance > stopDistance)
        {
            // Chase player with obstacle avoidance
            Vector2 moveDirection = CalculateAvoidanceDirection(direction);
            rb.linearVelocity = moveDirection * moveSpeed;
            currentTargetPosition = playerTransform.position;
        }
        else if (!canSeePlayer && lastKnownPositions.Count > 0)
        {
            // Get the most recent position
            Vector2 targetPos = lastKnownPositions[lastKnownPositions.Count - 1];
            float distanceToTarget = Vector2.Distance(transform.position, targetPos);
            
            if (distanceToTarget > 0.5f)
            {
                // Move towards last known position with obstacle avoidance
                Vector2 directionToTarget = (targetPos - (Vector2)transform.position).normalized;
                Vector2 moveDirection = CalculateAvoidanceDirection(directionToTarget);
                rb.linearVelocity = moveDirection * (moveSpeed * 0.7f); // Slightly slower when searching
                currentTargetPosition = targetPos;
            }
            else
            {
                // Reached target, move to next oldest position if available
                if (lastKnownPositions.Count > 1)
                {
                    lastKnownPositions.RemoveAt(lastKnownPositions.Count - 1);
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    Vector2 CalculateAvoidanceDirection(Vector2 desiredDirection)
    {
        // Check for obstacles in the desired direction
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, obstacleAvoidanceRadius, 
                                                desiredDirection, 2f, avoidanceLayer);
        
        if (hit.collider != null)
        {
            // Obstacle detected, find best alternative direction
            return FindBestAvoidanceDirection(desiredDirection);
        }
        
        return desiredDirection;
    }

    Vector2 FindBestAvoidanceDirection(Vector2 desiredDirection)
    {
        float bestAngle = 0f;
        float maxClearance = 0f;
        
        // Check multiple directions around the enemy
        for (int i = 0; i < obstacleCheckPoints; i++)
        {
            float angle = (360f / obstacleCheckPoints) * i;
            Vector2 checkDir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            
            // Cast in this direction
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, obstacleAvoidanceRadius, 
                                                    checkDir, 3f, avoidanceLayer);
            
            if (hit.collider == null)
            {
                // Clear path in this direction
                float alignment = Vector2.Dot(checkDir, desiredDirection);
                if (alignment > maxClearance)
                {
                    maxClearance = alignment;
                    bestAngle = angle;
                }
            }
        }
        
        // Return best direction found, or perpendicular to obstacle if none found
        if (maxClearance > 0)
        {
            return Quaternion.Euler(0, 0, bestAngle) * Vector2.right;
        }
        
        // Default to perpendicular to obstacle
        Vector2 perpendicular = new Vector2(-desiredDirection.y, desiredDirection.x);
        return perpendicular.normalized;
    }

    public float returnDist() 
    { 
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return float.MaxValue;
        }
        
        return Vector2.Distance(transform.position, playerTransform.position); 
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            // Editor mode
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null && playerObj.transform.position != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, playerObj.transform.position);
            }
            return;
        }

        // Play mode - enhanced visualization
        if (playerTransform != null && playerTransform.position != Vector3.zero)
        {
            // Line to player
            Gizmos.color = canSeePlayer ? Color.red : new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position, playerTransform.position);
            
            // Detection range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw all stored positions as a path
            if (lastKnownPositions.Count > 0)
            {
                // Draw positions as trail
                for (int i = 0; i < lastKnownPositions.Count; i++)
                {
                    float alpha = (float)(i + 1) / lastKnownPositions.Count;
                    Gizmos.color = new Color(1f, 1f, 0f, alpha);
                    Gizmos.DrawSphere(lastKnownPositions[i], 0.2f);
                    
                    // Draw connecting lines
                    if (i > 0)
                    {
                        Gizmos.color = new Color(1f, 0.5f, 0f, alpha * 0.5f);
                        Gizmos.DrawLine(lastKnownPositions[i - 1], lastKnownPositions[i]);
                    }
                }
                
                // Draw line from enemy to next target
                Vector2 targetPos = lastKnownPositions[lastKnownPositions.Count - 1];
                float memoryPercent = 1f - ((Time.time - playerLostTime) / lostPlayerMemoryTime);
                Gizmos.color = new Color(1f, 0.5f, 0f, Mathf.Clamp01(memoryPercent));
                Gizmos.DrawLine(transform.position, targetPos);
                
                // Draw current target
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(currentTargetPosition, 0.25f);
            }
            
            // Draw obstacle avoidance radius
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);
        }
    }
}