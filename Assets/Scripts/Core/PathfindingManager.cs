// PathfindingManager.cs
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager I;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;

    private Dictionary<Vector3Int, Node> grid = new Dictionary<Vector3Int, Node>();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        BuildGrid();
    }

    /// <summary>Call on start or after regenerating dungeon.</summary>
    public void BuildGrid()
    {
        grid.Clear();
        var bounds = floorTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            bool floor = floorTilemap.HasTile(pos);
            bool wall = wallTilemap != null && wallTilemap.HasTile(pos);
            grid[pos] = new Node(pos, walkable: floor && !wall);
        }
    }

    /// <summary>True if no wall & has floor.</summary>
    public bool IsWalkable(Vector3Int cell)
        => grid.TryGetValue(cell, out var n) && n.walkable;

    /// <summary>8-way A* w/ Chebyshev heuristic.</summary>
    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        if (!grid.ContainsKey(start) || !grid.ContainsKey(goal))
            return new List<Vector3Int>();

        // reset
        foreach (var n in grid.Values) { n.gCost = int.MaxValue; n.parent = null; }

        var openSet = new Heap<Node>(grid.Count);
        var closedSet = new HashSet<Node>();
        var startNode = grid[start];
        var goalNode = grid[goal];

        startNode.gCost = 0;
        startNode.hCost = Heuristic(start, goal);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var current = openSet.RemoveFirst();
            if (current == goalNode) break;
            closedSet.Add(current);

            foreach (var (nei, cost) in GetNeighbors(current))
            {
                if (!nei.walkable || closedSet.Contains(nei)) continue;
                int tentative = current.gCost + cost;
                if (tentative < nei.gCost)
                {
                    nei.gCost = tentative;
                    nei.hCost = Heuristic(nei.pos, goal);
                    nei.parent = current;
                    if (!openSet.Contains(nei)) openSet.Add(nei);
                    else openSet.UpdateItem(nei);
                }
            }
        }

        // retrace
        var path = new List<Vector3Int>();
        var node = goalNode;
        while (node != startNode)
        {
            path.Add(node.pos);
            node = node.parent;
            if (node == null) break;
        }
        path.Reverse();
        return path;
    }

    private IEnumerable<(Node, int)> GetNeighbors(Node n)
    {
        var dirs = new Vector3Int[]
        {
            new Vector3Int( 1,  0, 0), new Vector3Int(-1,  0, 0),
            new Vector3Int( 0,  1, 0), new Vector3Int( 0, -1, 0),
            new Vector3Int( 1,  1, 0), new Vector3Int(-1,  1, 0),
            new Vector3Int( 1, -1, 0), new Vector3Int(-1, -1, 0)
        };
        foreach (var d in dirs)
        {
            var p = n.pos + d;
            if (grid.TryGetValue(p, out var nei))
                yield return (nei, 1);
        }
    }

    private int Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy);
    }

    private class Node : IHeapItem<Node>
    {
        public Vector3Int pos;
        public bool walkable;
        public int gCost, hCost;
        public Node parent;
        public Node(Vector3Int p, bool walkable)
        {
            pos = p;
            this.walkable = walkable;
            gCost = int.MaxValue;
        }
        public int fCost => gCost + hCost;
        public int HeapIndex { get; set; }
        public int CompareTo(Node other)
        {
            int cmp = fCost.CompareTo(other.fCost);
            if (cmp == 0) cmp = hCost.CompareTo(other.hCost);
            return -cmp;
        }
    }
}
