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
        TurnsManager.Instance.EnterCombatMode();
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
        var map = FindFirstObjectByType<DungeonGenerator>().FloorTilemap;

        if (player != null)
        {
            Vector3Int playerCell = map.WorldToCell(player.transform.position);
            if (playerCell == cell) return true;
        }

        foreach (var enemy in FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
        {
            Vector3Int enemyCell = map.WorldToCell(enemy.transform.position);
            if (enemyCell == cell) return true;
        }

        return false;
    }

}
