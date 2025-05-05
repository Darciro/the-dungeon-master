using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance { get; private set; }

    [Header("Tilemap & Settings")]
    [Tooltip("The same Tilemap you use for movement range & floor detection.")]
    [SerializeField] private Tilemap floorTilemap;

    // 4-way neighbors; add diagonals here if you like
    private static readonly Vector3Int[] Directions = new[]
    {
        new Vector3Int( 1,  0, 0),
        new Vector3Int(-1,  0, 0),
        new Vector3Int( 0,  1, 0),
        new Vector3Int( 0, -1, 0),
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        if (floorTilemap == null)
            floorTilemap = FindObjectOfType<DungeonGenerator>().FloorTilemap;
    }

    /// <summary>
    /// Finds a shortest path of grid cells from start→goal using A*.
    /// Returns an empty list if no path exists.
    /// </summary>
    public List<Vector3Int> FindTilePath(Vector3Int start, Vector3Int goal)
    {
        var openSet = new HashSet<Vector3Int> { start };
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, float> { [start] = 0f };
        var fScore = new Dictionary<Vector3Int, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            // pick node in openSet with lowest fScore
            var current = openSet
                .OrderBy(n => fScore.ContainsKey(n) ? fScore[n] : float.MaxValue)
                .First();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var dir in Directions)
            {
                var neigh = current + dir;
                if (!floorTilemap.HasTile(neigh))
                    continue;

                float tentativeG = gScore[current] + 1f;  // cost per tile = 1

                if (tentativeG < gScore.GetValueOrDefault(neigh, float.MaxValue))
                {
                    cameFrom[neigh] = current;
                    gScore[neigh] = tentativeG;
                    fScore[neigh] = tentativeG + Heuristic(neigh, goal);
                    openSet.Add(neigh);
                }
            }
        }

        // no path found
        return new List<Vector3Int>();
    }

    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        // Manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3Int> ReconstructPath(
        Dictionary<Vector3Int, Vector3Int> cameFrom,
        Vector3Int current)
    {
        var path = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}
