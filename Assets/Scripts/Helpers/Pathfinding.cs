// Pathfinding.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class Pathfinding
{
    // 8 directions: N, S, E, W and the 4 diagonals
    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1)
    };

    /// <summary>
    /// Returns a list of grid‐cells from start → goal, avoiding both missing tiles
    /// and dynamically occupied cells, and preventing corner‐cutting on diagonals.
    /// </summary>
    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, Tilemap walkable)
    {
        if (start == goal)
            return new List<Vector3Int> { start };

        var openSet = new PriorityQueue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };

        openSet.Enqueue(start, Heuristic(start, goal));

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var dir in directions)
            {
                var neighbor = current + new Vector3Int(dir.x, dir.y, 0);

                // 1) must be floor
                if (!walkable.HasTile(neighbor) ||
                    GameManager.Instance.IsTileOccupied(neighbor))
                    continue;

                // 2) if diagonal, prevent corner‐cutting
                if (dir.x != 0 && dir.y != 0)
                {
                    var c1 = current + new Vector3Int(dir.x, 0, 0);
                    var c2 = current + new Vector3Int(0, dir.y, 0);
                    if (!walkable.HasTile(c1) || !walkable.HasTile(c2) ||
                        GameManager.Instance.IsTileOccupied(c1) ||
                        GameManager.Instance.IsTileOccupied(c2))
                        continue;
                }

                // 3) cost: 10 for straight, 14 for diagonal
                int cost = (dir.x != 0 && dir.y != 0) ? 14 : 10;
                int tentativeG = gScore[current] + cost;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    int fScore = tentativeG + Heuristic(neighbor, goal);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        // no path found
        return null;
    }

    // Chebyshev distance times 10 (to match our cost scale)
    private static int Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 10 * Mathf.Max(dx, dy);
    }

    // backtrack to build the full path
    private static List<Vector3Int> ReconstructPath(
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

    // simple list­-based min-priority queue
    private class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements
            = new List<(T, int)>();

        public int Count => elements.Count;

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
        }

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
