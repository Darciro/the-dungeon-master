using System.Collections.Generic;
using UnityEngine;

public class TurnsManager : MonoBehaviour
{
    public static TurnsManager Instance { get; private set; }

    // List of all combatants in this encounter
    private List<CharacterManager> combatants;
    private int currentTurnIndex;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called by EnemyManager when any enemy spots the player.
    /// </summary>
    public void EnterCombatMode()
    {
        if (GameManager.Instance.Mode != GameMode.Combat) return;

        Debug.Log("[DM] Entering Combat Mode");

        // TODO: collect all Combatant instances (player + enemies)
        combatants = new List<CharacterManager>(FindObjectsOfType<CharacterManager>());

        // Kick off turn order setup
        StartCombat();
    }

    private void StartCombat()
    {
        // 1. Roll initiative, reset AP
        foreach (var c in combatants)
        {
            c.RollInitiative();
        }

        // 2. Sort by initiative descending
        combatants.Sort((a, b) => b.Initiative.CompareTo(a.Initiative));

        currentTurnIndex = 0;
        BeginTurn(combatants[currentTurnIndex]);
    }

    private void BeginTurn(CharacterManager c)
    {
        Debug.Log($"[DM] {c.name}'s turn (Init: {c.Initiative})");
        // c.OnTurnStart();

        // If it’s an AI, invoke its decision loop; if player, enable UI
        if (c is EnemyManager enemy)
        {
            enemy.StartTurn();
        }
        else
        {
            Debug.Log($"[DM] Player's turn");
            c.StartTurn();
        }

        //UIManager.Instance.ShowPlayerTurn();
    }

    public void EndTurn()
    {
        // Clean up current
        // combatants[currentTurnIndex].OnTurnEnd();

        // Next index (wrap around)
        currentTurnIndex = (currentTurnIndex + 1) % combatants.Count;
        BeginTurn(combatants[currentTurnIndex]);
    }
}
