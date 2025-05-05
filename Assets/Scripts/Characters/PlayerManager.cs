using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[RequireComponent(typeof(MovementController))]
public class PlayerManager : CharacterManager
{
    private enum MoveState { Idle, Previewing, Moving }
    private MoveState moveState = MoveState.Idle;

    [SerializeField] private MovementRangeCalculator rangeCalculator;
    [SerializeField] private MovementPathRenderer pathRenderer;
    private Tilemap floorTilemap;
    private MovementController movementController;
    private float recalcInterval = 0.1f;
    private float nextRecalcTime = 0f;
    private float worldThreshold = 0.2f;
    private Vector3 lastTargetWorld = Vector3.positiveInfinity;

    private void Awake()
    {
        base.Awake();
        movementController = GetComponent<MovementController>();
        rangeCalculator = GetComponent<MovementRangeCalculator>();
        pathRenderer = GetComponent<MovementPathRenderer>();
        floorTilemap = FindObjectOfType<DungeonGenerator>().FloorTilemap;

        // re-enable preview whenever the character finishes moving
        movementController.OnMovementComplete += () =>
        {
            if (Stats.CurrentAP > 0)
                BeginPreview();
            else
                EndMovementPhase();
        };
    }

    void Start()
    {
        UIManager.Instance.UpdatePlayerVitals(Stats.CurrentHP, Stats.MaxHP, Stats.CurrentAP, Stats.MaxAP, Stats.Hunger, Stats.Thirst);
    }

    public override void StartTurn()
    {
        RestoreActionPoints();
        BeginPreview();
    }

    private void BeginPreview()
    {
        Debug.Log("[DM] BeginPreview");
        // recalc & show reach
        float maxDist = Stats.CurrentAP * 0.5f;
        rangeCalculator.Calculate(transform.position, maxDist);
        rangeCalculator.ShowHighlights();

        moveState = MoveState.Previewing;
    }

    private void EndMovementPhase()
    {
        Debug.Log("[DM] EndMovementPhase");
        moveState = MoveState.Idle;
        rangeCalculator.ClearHighlights();
        pathRenderer.ClearPath();

        // now you can enter attack/skill phase, or call TurnsManager.Instance.EndTurn()
    }

    private void Update()
    {
        if (moveState == MoveState.Previewing)
        {
            DoMovementPreview();
            return;
        }

        // Handle attack on initial click
        /* if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseW.z = 0f;
            Collider2D hit = Physics2D.OverlapCircle(mouseW, 1.0f, targetMask);

            if (hit != null)
            {
                // Attack logic
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                CharacterManager enemyMgr = hit.GetComponent<CharacterManager>();

                if (enemyMgr != null)
                {
                    // Stop current movement
                    movementController.StopMovement();

                    if (dist <= attackRadius)
                    {
                        Attack(enemyMgr);
                    }
                    else
                    {
                        StartCoroutine(MoveAndAttack(hit.transform));
                    }
                }
                return; // Skip movement handling when clicking on enemy
            }
        }*/

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


    private void DoMovementPreview()
    {
        // 1) Raycast mouse → world → cell
        Vector3 mouseW = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseW.z = 0f;
        Vector3Int targetCell = floorTilemap.WorldToCell(mouseW);

        // 2) Bail out if not on floor or out of AP-range
        if (!floorTilemap.HasTile(targetCell) ||
            !rangeCalculator.Reachable.ContainsKey(targetCell))
        {
            pathRenderer.ClearPath();
            return;
        }

        // 3) Compute world-space start & end, both at tile centers
        Vector3Int startCell = floorTilemap.WorldToCell(transform.position);
        Vector3 startW = floorTilemap.GetCellCenterWorld(startCell) + Vector3.up * 0.1f;
        Vector3 targetW = floorTilemap.GetCellCenterWorld(targetCell) + Vector3.up * 0.1f;

        // 4) Try a straight line (3D physics) first
        bool direct = !Physics.Linecast(startW, targetW, obstructionMask);

        Vector3[] worldPoints;
        float previewDist;

        if (direct)
        {
            // Straight shot
            worldPoints = new[] { startW, targetW };
            previewDist = Vector3.Distance(startW, targetW);
        }
        else
        {
            // Fallback: A* on grid
            List<Vector3Int> cellPath = PathFinder.Instance.FindTilePath(startCell, targetCell);

            worldPoints = new Vector3[cellPath.Count];
            previewDist = 0f;

            for (int i = 0; i < cellPath.Count; i++)
            {
                Vector3 wp = floorTilemap.GetCellCenterWorld(cellPath[i]) + Vector3.up * 0.1f;
                worldPoints[i] = wp;
                if (i > 0)
                    previewDist += Vector3.Distance(worldPoints[i - 1], wp);
            }
        }

        // 5) Draw dashed line
        pathRenderer.DrawPath(worldPoints);

        // 6) On click: spend AP & move
        int apCost = Mathf.CeilToInt(previewDist / 0.5f);
        if (Input.GetMouseButtonDown(0))
        {
            Stats.CurrentAP -= apCost;
            UIManager.Instance.UpdatePlayerVitals(
                Stats.CurrentHP, Stats.MaxHP,
                Stats.CurrentAP, Stats.MaxAP,
                Stats.Hunger, Stats.Thirst
            );

            movementController.MoveToWorld(worldPoints[worldPoints.Length - 1]);

            // Tear down preview
            rangeCalculator.ClearHighlights();
            pathRenderer.ClearPath();
            moveState = MoveState.Moving;
        }
    }


}
