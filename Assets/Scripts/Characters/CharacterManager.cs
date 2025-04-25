using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Attributes")]
    public CharacterAttributes Attributes = new CharacterAttributes();

    [Header("Stats")]
    public CharacterStats Stats = new CharacterStats();

    [Header("View Settings")]
    [Tooltip("How far this character can see/detect")]
    [SerializeField] private float viewRadius = 5f;

    [Tooltip("Which layers block sight (walls, obstacles)")]
    [SerializeField] private LayerMask obstructionMask;

    [Tooltip("Which layers count as targets (e.g. Player)")]
    [SerializeField] private LayerMask targetMask;


    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Stats.CalculateStatsFromAttributes(Attributes);
    }

    public virtual void StartTurn()
    {
        // When in combat mode
    }

    public virtual void Attack(CharacterManager target)
    {
        // Handle entity attacks
    }


    public virtual void TakeDamage(int damage)
    {
        if (Stats.CurrentHP <= 0)
            Die();
    }

    public virtual void UseActionPoints(int cost)
    {
        Stats.CurrentAP = Mathf.Max(0, Stats.CurrentAP - cost);
    }

    public virtual void RestoreActionPoints()
    {
        Stats.CurrentAP = Stats.MaxAP;
    }

    public virtual void ConsumeResources(int hungerCost = 1, int thirstCost = 1)
    {
        Stats.Hunger = Mathf.Max(0, Stats.Hunger - hungerCost);
        Stats.Thirst = Mathf.Max(0, Stats.Thirst - thirstCost);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
