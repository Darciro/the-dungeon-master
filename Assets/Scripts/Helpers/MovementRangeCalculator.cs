using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class MovementRangeCalculator : MonoBehaviour
{
    [Tooltip("The Tilemap that defines your ground/walkable area")]
    [SerializeField] private Tilemap floorTilemap;

    [Header("Highlight Prefab")]
    [Tooltip("A simple quad/sprite to show on each reachable tile.")]
    [SerializeField] private GameObject highlightPrefab;
    private List<GameObject> _highlights = new List<GameObject>();

    void Start()
    {
        floorTilemap = FindFirstObjectByType<DungeonGenerator>().FloorTilemap;
    }

    // 4-way connectivity; add diagonals if you like
    private static readonly Vector3Int[] Directions = new Vector3Int[]
    {
        new Vector3Int( 1,  0, 0),
        new Vector3Int(-1,  0, 0),
        new Vector3Int( 0,  1, 0),
        new Vector3Int( 0, -1, 0),
    };

    /// <summary>
    /// After Calculate() is run, this dictionary maps each reachable cell
    /// to the *shortest* distance (in world units) from startCell.
    /// </summary>
    public Dictionary<Vector3Int, float> Reachable { get; private set; }

    /// <summary>
    /// Compute all tiles within maxDistance of startCell.
    /// Call this once when the player’s turn begins (or when AP changes).
    /// </summary>
    /// <param name="startWorldPos">Your player’s world position</param>
    /// <param name="maxDistance">= currentAP * 0.5f</param>
    public void Calculate(Vector3 startWorldPos, float maxDistance)
    {
        // 1) Convert world → cell
        Vector3Int startCell = floorTilemap.WorldToCell(startWorldPos);

        // 2) Prepare Dijkstra
        Reachable = new Dictionary<Vector3Int, float>();
        var frontier = new List<Vector3Int>();

        Reachable[startCell] = 0f;
        frontier.Add(startCell);

        // 3) Flood-fill / Dijkstra (linear search PQ—ok for a few hundred tiles)
        while (frontier.Count > 0)
        {
            // pick the frontier cell with smallest distance so far
            Vector3Int current = frontier[0];
            float bestD = Reachable[current];
            for (int i = 1; i < frontier.Count; i++)
            {
                float d = Reachable[frontier[i]];
                if (d < bestD)
                {
                    bestD = d;
                    current = frontier[i];
                }
            }
            frontier.Remove(current);

            // scan its 4 neighbors
            foreach (var dir in Directions)
            {
                Vector3Int neigh = current + dir;

                // skip if not a ground tile
                if (!floorTilemap.HasTile(neigh))
                    continue;

                // cost per tile is 1 unit (you can multiply by tile size if non-1)
                float nd = bestD + 1f;
                if (nd > maxDistance)
                    continue;

                // if unseen or found shorter path
                if (!Reachable.ContainsKey(neigh) || nd < Reachable[neigh])
                {
                    Reachable[neigh] = nd;
                    frontier.Add(neigh);
                }
            }
        }
    }

    /// <summary>
    /// Instantiate one highlightPrefab at the center of every cell in Reachable.
    /// </summary>
    public void ShowHighlights()
    {
        ClearHighlights();
        foreach (var kv in Reachable)
        {
            Vector3Int cell = kv.Key;
            Vector3 world = floorTilemap.GetCellCenterWorld(cell);
            var go = Instantiate(highlightPrefab, world + Vector3.up * 0.01f, Quaternion.identity);
            _highlights.Add(go);
        }
    }

    /// <summary>
    /// Destroy any existing highlights.
    /// </summary>
    public void ClearHighlights()
    {
        for (int i = 0; i < _highlights.Count; i++)
            if (_highlights[i] != null) Destroy(_highlights[i]);
        _highlights.Clear();
    }
}
