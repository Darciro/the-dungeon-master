using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MovementController))]
public class PlayerManager : CharacterManager
{
    [Header("Path Recalculation")]
    [SerializeField] private float recalcInterval = 0.1f;
    [SerializeField] private float worldThreshold = 0.2f;

    private Tilemap floorTilemap;
    private MovementController movementController;

    private float nextRecalcTime = 0f;
    private Vector3 lastTargetWorld = Vector3.positiveInfinity;

    private void Awake()
    {
        base.Awake();
        movementController = GetComponent<MovementController>();
    }

    void Start()
    {
        floorTilemap = FindFirstObjectByType<DungeonGenerator>().FloorTilemap;
        UIManager.Instance.UpdatePlayerVitals(Stats.CurrentHP, Stats.MaxHP, Stats.CurrentAP, Stats.MaxAP, Stats.Hunger, Stats.Thirst);
    }

    void Update()
    {
        // Handle attack on initial click
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseW.z = 0f;
            Collider2D hit = Physics2D.OverlapCircle(mouseW, 1.0f, targetMask);
            Debug.Log($"Click-hit returned: {hit?.name ?? "none"}");

            if (hit != null)
            {
                // Attack logic
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                CharacterManager enemyMgr = hit.GetComponent<CharacterManager>();
                Debug.Log($"enemyMgr? {enemyMgr}");

                if (enemyMgr != null)
                {
                    Debug.Log("→ About to Attack()");

                    // Stop current movement
                    movementController.StopMovement();

                    if (dist <= attackRadius)
                    {
                        Debug.Log("Attacking immediately");
                        Attack(enemyMgr);
                    }
                    else
                    {
                        Debug.Log("Moving into range, then attack");
                        StartCoroutine(MoveAndAttack(hit.transform));
                    }
                }
                return; // Skip movement handling when clicking on enemy
            }
        }

        // Movement handling when not attacking
        if (Input.GetMouseButton(0))
        {
            HandleClickAndHold();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Reset recalc timer so next click fires immediately
            nextRecalcTime = 0f;
        }
    }

    private void HandleClickAndHold()
    {
        // 1) only recalc at most once per interval
        if (Time.time < nextRecalcTime) return;
        nextRecalcTime = Time.time + recalcInterval;

        // 2) get exact world-point under mouse
        Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseW.z = 0f;

        // 3) bail if we haven’t moved enough in world units
        if (Vector3.Distance(mouseW, lastTargetWorld) < worldThreshold)
            return;
        lastTargetWorld = mouseW;

        // 4) bail if it’s off the floor
        var cell = floorTilemap.WorldToCell(mouseW);
        if (!floorTilemap.HasTile(cell)) return;

        // 5) pivot instantly toward the new click spot
        movementController.StopMovement();
        movementController.MoveToWorld(mouseW);
    }

    private IEnumerator MoveAndAttack(Transform enemy)
    {
        // Calculate attack approach point
        Vector2 dir = ((Vector2)enemy.position - (Vector2)transform.position).normalized;
        Vector2 attackPoint = (Vector2)enemy.position - dir * attackRadius;
        movementController.MoveToWorld(attackPoint);

        // Wait until reaching attack point
        while (movementController.IsMoving)
            yield return null;

        // Perform attack
        CharacterManager enemyMgr = enemy.GetComponent<CharacterManager>();
        if (enemyMgr != null)
            Attack(enemyMgr);
    }
}
