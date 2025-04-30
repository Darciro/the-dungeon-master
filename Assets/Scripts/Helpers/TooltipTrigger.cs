using UnityEngine;

public class TooltipTrigger : MonoBehaviour
{
    private CharacterManager character;

    private void Awake()
    {
        character = GetComponent<CharacterManager>();
    }

    private void OnMouseEnter()
    {
        UIManager.Instance.ShowEnemyTooltip(character, transform.position);
    }

    private void OnMouseExit()
    {
        UIManager.Instance.HideEnemyTooltip();
    }
}
