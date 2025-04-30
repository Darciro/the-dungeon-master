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
}
