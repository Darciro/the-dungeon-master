using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Movement controller for isometric grid-based movement, suitable for both player and enemies.
/// Uses A* pathfinding on a Tilemap floor grid and moves smoothly between cell centers.
/// </summary>
public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Units per second movement speed")]
    public float moveSpeed = 2f;
    [Tooltip("Time in seconds to wait before starting next move")]
    public float turnDelay = 0.1f;

    [SerializeField] private Tilemap floorTilemap;
    public Tilemap MovementTilemap => floorTilemap;

    private Vector3 moveTarget;
    private Coroutine moveCoroutine;
    private Queue<Vector3Int> path = new Queue<Vector3Int>();

    public event Action OnPathComplete;
    private bool isMoving = false;
    public bool IsMoving => isMoving;

    public void Configure(Tilemap floor) => floorTilemap = floor;

    /// <summary>
    /// Enqueue a list of tile‐coordinates (from A*) and start moving along them.
    /// Skips the first cell (current position).
    /// </summary>
    public void FollowPath(List<Vector3Int> cellPath)
    {
        if (floorTilemap == null)
        {
            Debug.LogError("MovementController: Floor Tilemap not configured.");
            return;
        }

        // Stop whatever we were doing
        StopAllCoroutines();
        path.Clear();

        // Enqueue every step AFTER the starting cell
        for (int i = 1; i < cellPath.Count; i++)
            path.Enqueue(cellPath[i]);

        // Kick off our movement coroutine
        StartCoroutine(FollowPathRoutine());
    }

    private IEnumerator FollowPathRoutine()
    {
        isMoving = true;

        while (path.Count > 0)
        {
            Vector3Int nextCell = path.Dequeue();
            Vector3 targetWorld = floorTilemap.GetCellCenterWorld(nextCell);

            // Smooth move towards that cell center
            while ((Vector3)transform.position != targetWorld)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                                                         targetWorld,
                                                         moveSpeed * Time.deltaTime);
                yield return null;
            }

            // small delay between steps
            yield return new WaitForSeconds(turnDelay);
        }

        isMoving = false;
        // Notify subscribers
        OnPathComplete?.Invoke();
    }

    /// <summary>
    /// Stop any free‐move in progress.
    /// </summary>
    public void StopMovement()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        isMoving = false;
    }

    /// <summary>
    /// Freely move toward this world position; updates the target if called again.
    /// </summary>
    public void MoveToPosition(Vector3 targetWorld)
    {
        moveTarget = targetWorld;

        // if already moving, just update moveTarget and return
        if (isMoving) return;

        // otherwise start the chase
        moveCoroutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        // keep chasing while we're not at the target (or it's moving away)
        while ((Vector3)transform.position != moveTarget)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                moveTarget,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        isMoving = false;
        moveCoroutine = null;
        OnPathComplete?.Invoke();
    }

    /// <summary>
    /// Simple A* implementation on the floor grid.
    /// </summary>
    private List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        var openSet = new PriorityQueue<Vector3Int>();
        openSet.Enqueue(start, 0);

        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == goal) return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                int tentativeG = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    int fScore = tentativeG + Heuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }
        return null; // no path found
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var totalPath = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    private int Heuristic(Vector3Int a, Vector3Int b)
    {
        // Manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        var dirs = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var d in dirs)
        {
            var n = cell + d;
            if (floorTilemap.HasTile(n))
                yield return n;
        }
    }

    private class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new List<(T, int)>();
        public int Count => elements.Count;
        public void Enqueue(T item, int priority) => elements.Add((item, priority));
        public T Dequeue()
        {
            int bestIndex = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[bestIndex].priority)
                    bestIndex = i;

            var bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
