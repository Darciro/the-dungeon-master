using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Attributes")]
    public CharacterAttributes Attributes = new CharacterAttributes();

    [Header("Stats")]
    public CharacterStats Stats = new CharacterStats();

    [Header("Combat Settings")]
    [Tooltip("How many Action Points this attack costs.")]
    public int attackAPCost = 1;
    [Tooltip("Base damage added on top of Strength.")]
    public int baseDamage = 0;

    [Header("View Settings")]
    [SerializeField] public float viewRadius = 5f;
    [SerializeField] public LayerMask obstructionMask;
    [SerializeField] public LayerMask targetMask;
    [SerializeField] public float attackRadius = 1.5f;

    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Stats.CalculateStatsFromAttributes(Attributes);
        Stats.CurrentAP = Stats.MaxAP;
    }

    public virtual void StartTurn()
    {
        // refill AP at start of turn
        RestoreActionPoints();
    }

    public virtual void Attack(CharacterManager target)
    {
        Debug.Log($"Inside Attack(): AP={Stats.CurrentAP}/{attackAPCost}");
        if (Stats.CurrentAP < attackAPCost)
        {
            Debug.Log("  – Not enough AP");
            return;
        }
        Debug.Log("  – Consuming AP and dealing damage");

        // spend AP
        UseActionPoints(attackAPCost);

        // calculate damage
        int damage = Attributes.Strength + baseDamage;
        Debug.Log($"{name} takes {damage} dmg (HP before: {Stats.CurrentHP})");

        // play attack animation
        if (animator != null)
            animator.SetTrigger("Attack");

        // apply damage
        target.TakeDamage(damage);
    }

    public virtual void TakeDamage(int damage)
    {
        // subtract HP
        Stats.CurrentHP = Mathf.Max(0, Stats.CurrentHP - damage);

        // play hit animation/sfx
        if (animator != null)
            animator.SetTrigger("Hit");

        ShowDamagePopup(damage);
        // TODO: update health bar UI here, e.g.
        UIManager.Instance.UpdatePlayerVitals(Stats.CurrentHP, Stats.MaxHP, Stats.CurrentAP, Stats.MaxAP, Stats.Hunger, Stats.Thirst);

        // check death
        if (Stats.CurrentHP <= 0)
            Die();
    }

    void ShowDamagePopup(int amount)
    {
        if (UIManager.Instance.damagePopupPrefab == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.25f);
        GameObject popupGO = Instantiate(UIManager.Instance.damagePopupPrefab, screenPos, Quaternion.identity, UIManager.Instance.GetComponent<RectTransform>());
        DamagePopup popup = popupGO.GetComponent<DamagePopup>();
        popup.Setup(amount);
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
        // play death VFX/sound
        if (animator != null)
            animator.SetTrigger("Die");

        // destroy after a short delay to let animation play
        Destroy(gameObject, 0.5f);
    }
}
