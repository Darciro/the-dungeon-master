using System;
using System.Collections;
using UnityEngine;

public enum TurnPhase { Exploration, PlayerTurn, EnemyTurn }
public class TurnManager : MonoBehaviour
{
    public static TurnManager I;
    public TurnPhase Phase { get; private set; }
    public event Action<TurnPhase> OnPhaseChanged;

    void Awake() => I = this;

    // public void StartCombat() => BeginPhase(TurnPhase.PlayerTurn);

    /* void BeginPhase(TurnPhase ph)
    {
        Phase = ph;
        OnPhaseChanged?.Invoke(ph);
        if (ph == TurnPhase.PlayerTurn) PlayerManager.I.StartTurn();
        else StartCoroutine(RunEnemyTurns());
    }
    
    IEnumerator RunEnemyTurns()
    {
        foreach (var e in EnemyManager.AllEnemies)
        {
            e.StartTurn();
            yield return e.ExecuteAISteps(); // see next
        }
        BeginPhase(TurnPhase.PlayerTurn);
    } */
}
