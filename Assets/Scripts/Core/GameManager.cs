using System;
using UnityEngine;

public enum GameMode { Exploration, Combat }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameMode Mode { get; private set; }
    public event Action<GameMode> OnModeChanged;
    public PlayerManager player;

    void Awake()
    {
        Instance = this;
        EnterExploration();
    }

    public void EnterExploration()
    {
        SetMode(GameMode.Exploration);
    }

    public void EnterCombat()
    {
        SetMode(GameMode.Combat);
    }

    private void SetMode(GameMode newMode)
    {
        Mode = newMode;
        OnModeChanged?.Invoke(Mode);
    }

    /// <summary>
    /// Returns true if any character occupies the given cell.
    /// Uses MovementController.MovementTilemap to map world→cell.
    /// </summary>
    public bool IsTileOccupied(Vector3Int cell)
    {
        // 1. Check the player
        if (player != null)
        {
            var mover = player.GetComponent<MovementController>();
            if (mover != null)
            {
                Vector3Int playerCell = mover.MovementTilemap.WorldToCell(player.transform.position);
                if (playerCell == cell) return true;
            }
        }

        // 2. Check all enemies
        //    (we assume you’ve switched to EnemyManager)
        foreach (var enemy in FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
        {
            if (enemy == null) continue;

            var mover = enemy.GetComponent<MovementController>();
            if (mover == null) continue;

            Vector3Int enemyCell = mover.MovementTilemap.WorldToCell(enemy.transform.position);
            if (enemyCell == cell) return true;
        }

        return false;
    }
}
