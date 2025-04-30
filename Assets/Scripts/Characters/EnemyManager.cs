using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : CharacterManager
{
    public enum AIState { Patrol, Pursue, Attack }

    [Header("Attack")]
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Patrol")]
    [Tooltip("Radius around spawn point for random patrol destinations.")]
    public float patrolRadius = 5f;
    [Tooltip("Seconds to wait at each patrol point.")]
    public float patrolWaitTime = 2f;

    private Vector2 spawnPosition;
    private AIState currentState;
    private Transform target;
    private MovementController movementController;

    private void Awake()
    {
        base.Awake();
        movementController = GetComponent<MovementController>();
    }

    void Start()
    {
        if (movementController == null)
            Debug.LogError("EnemyManager requires a MovementController component.", this);

        spawnPosition = transform.position;
        currentState = AIState.Patrol;
        StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                LookForPlayer();
                break;
            case AIState.Pursue:
                PursueTarget();
                break;
            case AIState.Attack:
                AttackTarget();
                break;
        }
    }

    void LookForPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, viewRadius, targetMask);
        if (hit)
        {
            Vector2 dir = (hit.transform.position - transform.position).normalized;
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (!Physics2D.Raycast(transform.position, dir, dist, obstructionMask))
            {
                // Player detected: switch to pursue and enter combat mode
                target = hit.transform;
                StopAllCoroutines();
                currentState = AIState.Pursue;
                GameManager.Instance.EnterCombat();
            }
        }
    }

    void PursueTarget()
    {
        if (target == null)
        {
            ReturnToPatrol();
            return;
        }

        Vector2 dirToTarget = (target.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, target.position);

        // Check line of sight
        if (Physics2D.Raycast(transform.position, dirToTarget, dist, obstructionMask))
        {
            ReturnToPatrol();
            return;
        }

        // If within attack range, switch to attack state
        if (dist <= attackRadius)
        {
            movementController.StopMovement();
            currentState = AIState.Attack;
            return;
        }

        // Otherwise, keep pursuing
        movementController.MoveToWorld(target.position);
    }

    void AttackTarget()
    {
        if (target == null)
        {
            ReturnToPatrol();
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > attackRadius)
        {
            currentState = AIState.Pursue;
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            CharacterManager charMgr = GetComponent<CharacterManager>();
            CharacterManager targetMgr = target.GetComponent<CharacterManager>();
            if (charMgr != null && targetMgr != null)
            {
                charMgr.Attack(targetMgr);
            }
        }
    }

    void ReturnToPatrol()
    {
        // Lost sight: return to patrol and exit combat mode
        currentState = AIState.Patrol;
        target = null;
        GameManager.Instance.EnterExploration();
        StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        while (currentState == AIState.Patrol)
        {
            // Pick a random destination within patrol radius
            Vector2 randomPoint = spawnPosition + Random.insideUnitCircle * patrolRadius;

            // Initiate movement
            movementController.MoveToWorld(randomPoint);

            // If movement didn't start (no path), pick a new point next frame
            if (!movementController.IsMoving)
            {
                yield return null;
                continue;
            }

            // Wait until movement completes
            while (movementController.IsMoving)
                yield return null;

            // Wait at destination
            yield return new WaitForSeconds(patrolWaitTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? (Vector3)spawnPosition : transform.position;
        Gizmos.DrawWireSphere(center, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
