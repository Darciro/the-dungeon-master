using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
What is A* (A-Star) Pathfinding Logic

A* (pronounced A-star) is a smart pathfinding algorithm used in games and AI to find the shortest and most efficient path between two points — like from a player to a goal — while avoiding obstacles like walls, pits, enemies, etc.
*/
public static class Pathfinding
{
    private static readonly Vector2Int[] directions = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, Tilemap walkableTilemap)
    {
        if (start == goal)
            return new List<Vector3Int> { start };

        var openSet = new PriorityQueue<Vector3Int>();
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };

        openSet.Enqueue(start, 0);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var dir in directions)
            {
                var neighbor = current + new Vector3Int(dir.x, dir.y, 0);
                if (!walkableTilemap.HasTile(neighbor)) continue;

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

        return null; // No path found
    }

    private static int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> path = new List<Vector3Int> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }
}
