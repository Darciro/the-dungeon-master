using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class PlayerManager : CharacterManager
{
    [Header("Isometric Movement")]
    [SerializeField] private Tilemap floorTilemap;
    [Tooltip("World‑units per Action Point during combat.")]
    [SerializeField] private float unitsPerAP = 0.1f;

    private MovementController movement;
    private bool inCombatTurn = false;
    private Vector3 combatStartPos;
    private int pathCost = 0;

    private void Start()
    {
        if (floorTilemap == null)
        {
            var dungeonGenerator = Object.FindFirstObjectByType<DungeonGenerator>();
            floorTilemap = dungeonGenerator?.FloorTilemap;
        }

        movement = GetComponent<MovementController>();
        // movement?.Configure(floorTilemap);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleClickDown();
        if (Input.GetMouseButton(0)) HandleHoldMove();
        /* if (Input.GetMouseButtonUp(0) && GameManager.Instance.InCombat && inCombatTurn && TurnManager.Instance.CurrentActor == this)
        {
            movement.StopMovement();
        } */

        /* if (GameManager.Instance.InCombat &&
            TurnManager.Instance.CurrentActor == this &&
            Input.GetKeyDown(KeyCode.Space))
        {
            EndPlayerTurn();
        }

        CheckView(); */
    }

    public override void StartTurn()
    {
        Debug.Log("Player turn started!");
        RestoreActionPoints();
    }

    private void HandleClickDown()
    {
        if (movement.IsMoving) return;

        if (GameManager.Instance.Mode == GameMode.Combat && Stats.CurrentAP <= 0)
        {
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3Int playerCell = floorTilemap.WorldToCell(transform.position);

        // Attack logic (unchanged)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider != null && hit.collider.TryGetComponent<CharacterManager>(out var target) && target != this)
        {
            Vector3Int targetCell = floorTilemap.WorldToCell(target.transform.position);
            if (Vector3Int.Distance(playerCell, targetCell) == 1)
            {
                Attack(target);
                EndPlayerTurn();
                return;
            }
            foreach (var d in new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right })
            {
                Vector3Int adj = targetCell + d;
                if (!floorTilemap.HasTile(adj) || GameManager.Instance.IsTileOccupied(adj)) continue;
                var path = Pathfinding.FindPath(playerCell, adj, floorTilemap);
                if (path == null) continue;

                pathCost = path.Count - 1;
                movement.FollowPath(path);
                return;
            }
            //UIManager.Instance.AddLog("Can't reach enemy to attack.");
            return;
        }

        // Free‐move follow hold
        HandleHoldMove();
    }

    private void HandleHoldMove()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3Int clickedCell = floorTilemap.WorldToCell(mouseWorld);
        if (!floorTilemap.HasTile(clickedCell)) return;

        if (GameManager.Instance.Mode != GameMode.Combat)
        {
            movement.MoveToPosition(mouseWorld);
            return;
        }

        /* if (inCombatTurn && TurnManager.Instance.CurrentActor == this)
        {
            Vector3Int startCell = floorTilemap.WorldToCell(transform.position);
            var path = Pathfinding.FindPath(startCell, clickedCell, floorTilemap);
            if (path == null) return;

            int maxSteps = Mathf.FloorToInt(Stats.CurrentAP);
            if (path.Count - 1 > maxSteps)
                path = path.GetRange(0, maxSteps + 1);

            Vector3 worldTarget = floorTilemap.GetCellCenterWorld(path[^1]);
            movement.MoveToPosition(worldTarget);

            // Preview with raw start & raw end
            PathPreviewLine.ShowDirect(
                rawStart: transform.position,
                rawEnd: mouseWorld,
                maxAP: Stats.CurrentAP,
                unitsPerAP: unitsPerAP
            );
        } */
    }

    private void OnCombatMoveComplete()
    {
        movement.OnPathComplete -= OnCombatMoveComplete;
        float moved = Vector3.Distance(combatStartPos, transform.position);
        int spentAP = Mathf.CeilToInt(moved / unitsPerAP);
        Stats.CurrentAP = Mathf.Max(0, Stats.CurrentAP - spentAP);

        if (Stats.CurrentAP > 0)
        {
            combatStartPos = transform.position;
            movement.OnPathComplete += OnCombatMoveComplete;
        }
        else
        {
            EndPlayerTurn();
        }
    }

    public void EndPlayerTurn()
    {
        inCombatTurn = false;
    }

    public override void RestoreActionPoints()
    {
        base.RestoreActionPoints();
    }
}
