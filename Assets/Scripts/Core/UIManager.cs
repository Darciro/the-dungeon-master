using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private RectTransform canvasRoot;

    [Header("Turn Phase UI")]
    public TextMeshProUGUI gameModeText;

    [Header("Log")]
    public Text logText;

    [Header("Player Vitals")]
    public TMP_Text healthPointsText;
    public TMP_Text actionPointsText;
    public TMP_Text hungerText;
    public TMP_Text thirstText;

    [Header("Enemy Tooltip")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI damageText;

    public GameObject damagePopupPrefab;


    private void Awake()
    {
        Instance = this;
        if (canvasRoot == null)
            canvasRoot = GameObject.FindGameObjectWithTag("GameUI")?.GetComponent<RectTransform>();
    }

    void Start()
    {
        GameManager.Instance.OnModeChanged += HandleModeChanged;
        // Initialize display
        HandleModeChanged(GameManager.Instance.Mode);
    }

    void HandleModeChanged(GameMode mode)
    {
        if (mode == GameMode.Combat)
        {
            gameModeText.text = "<color=red>COMBAT MODE</color>";
        }
        else
        {
            gameModeText.text = "<color=white>EXPLORATION MODE</color>";
        }
    }

    public void ShowEnemyTooltip(CharacterManager enemy, Vector3 worldPosition)
    {
        if (enemy == null) return;

        nameText.text = enemy.name;
        hpText.text = $"HP: {enemy.Stats.CurrentHP} / {enemy.Stats.MaxHP}";
        damageText.text = $"DMG: {enemy.Attributes.Strength}";

        // Convert world pos to screen pos
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        screenPos += new Vector2(100, 100); // X = right, Y = up
        tooltipPanel.transform.position = screenPos;


        tooltipPanel.SetActive(true);
    }

    public void HideEnemyTooltip()
    {
        tooltipPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnModeChanged -= HandleModeChanged;
    }

    /// <summary>
    /// Update the player vitals display. Call with current and maximum values.
    /// </summary>
    public void UpdatePlayerVitals(int currentHP, int maxHP, int currentAP, int maxAP, int hunger, int thirst)
    {
        healthPointsText.text = $"Health Points: {currentHP}/{maxHP}";
        actionPointsText.text = $"Action Points: {currentAP}/{maxAP}";
        hungerText.text = $"Hunger: {hunger}";
        thirstText.text = $"Thirst: {thirst}";
    }

    public void AddLog(string message)
    {
        logText.text += message + "\n";
    }
}
