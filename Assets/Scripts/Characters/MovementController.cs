using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Units per second movement speed")]
    public float moveSpeed = 5f;

    [Tooltip("Assign your dungeon floor tilemap here (or let it auto-find).")]
    [SerializeField] private Tilemap floorTilemap;

    private Coroutine moveRoutine;
    private bool isMoving;

    public bool IsMoving => isMoving;
    public event Action OnMovementComplete;

    void Start()
    {
        // if you forgot to wire it up in the Inspector, try to grab it now
        if (floorTilemap == null)
        {
            var dg = FindFirstObjectByType<DungeonGenerator>();
            floorTilemap = dg != null ? dg.FloorTilemap : null;
        }
        if (floorTilemap == null)
            Debug.LogError("MovementController: no FloorTilemap!", this);
    }

    public void MoveToWorld(Vector3 targetWorld)
    {
        if (floorTilemap == null) return;

        // 1) A* on the grid
        var start = floorTilemap.WorldToCell(transform.position);
        var goal = floorTilemap.WorldToCell(targetWorld);
        var path = Pathfinding.FindPath(start, goal, floorTilemap);
        if (path == null || path.Count == 0) return;

        // 2) Build world‐space waypoints (skip your own cell)
        var worldPath = new List<Vector3>(path.Count);
        for (int i = 1; i < path.Count; i++)
            worldPath.Add(floorTilemap.GetCellCenterWorld(path[i]));

        // 3) Last waypoint is the exact click point
        if (worldPath.Count > 0)
            worldPath[^1] = targetWorld;
        else
            worldPath.Add(targetWorld);

        // 4) Restart the coroutine
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(FollowWorldPath(worldPath));
    }

    public void StopMovement()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = null;
        isMoving = false;
    }

    private IEnumerator FollowWorldPath(List<Vector3> path)
    {
        isMoving = true;

        foreach (var wp in path)
        {
            while (Vector3.Distance(transform.position, wp) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    wp,
                    moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;
        moveRoutine = null;
        OnMovementComplete?.Invoke();
    }
}
