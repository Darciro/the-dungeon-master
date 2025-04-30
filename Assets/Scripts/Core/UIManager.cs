using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private RectTransform canvasRoot;

    [Header("Turn Phase UI")]
    public TextMeshProUGUI gameModeText;

    [Header("Log")]
    public Text logText;

    [Header("Player Vitals")]
    public TMP_Text healthPointsText;
    public TMP_Text ActionPoints;
    public TMP_Text hungerText;
    public TMP_Text thirstText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        else Destroy(gameObject);

        if (canvasRoot == null)
        {
            canvasRoot = GameObject.FindGameObjectWithTag("GameUI")?.GetComponent<RectTransform>();
        }
    }

    public void UpdatePlayerVitals(int hp, int ap, int hunger, int thirst)
    {
        healthPointsText.text = $"Health Points: {hp}";
        hungerText.text = $"Hunger: {hunger}";
        thirstText.text = $"Thirst: {thirst}";
        ActionPoints.text = $"Action Points: {ap}";
    }

    public void AddLog(string message)
    {
        logText.text += message + "\n";
    }
}
