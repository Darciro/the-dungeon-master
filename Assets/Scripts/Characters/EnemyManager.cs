using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private MovementController mover;
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private int stepsPerTurn = 1;

    void Awake()
    {
        mover = mover ?? GetComponent<MovementController>();
    }

    // Call this when you want the enemy to advance toward the player
    public void MoveTowardPlayer()
    {
        var floor = FindObjectOfType<DungeonGenerator>().FloorTilemap;
        Vector3Int start = floor.WorldToCell(transform.position);
        Vector3Int end = floor.WorldToCell(targetPlayer.position);

        List<Vector3Int> cellPath = PathfindingManager.I.FindPath(start, end);
        if (cellPath == null || cellPath.Count <= 1) return;

        // Skip current cell, then take a defined number of steps
        IEnumerable<Vector3Int> pathSegment = cellPath.Skip(1).Take(stepsPerTurn);

        List<Vector3> worldPath = new List<Vector3>();
        foreach (var c in pathSegment)
            worldPath.Add(floor.GetCellCenterWorld(c));

        // mover.SetPath(worldPath);
    }
}
