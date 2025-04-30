using UnityEngine;

public enum CursorType { Default, Move, Grab, Attack, Interact }

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    private Texture2D defaultCursor;
    private Texture2D moveCursor;
    private Texture2D grabCursor;
    private Texture2D attackCursor;
    private Texture2D interactCursor;

    private CursorType currentCursor = CursorType.Default;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadCursors();

        // Set the default cursor on startup
        SetCursor(CursorType.Default);
    }

    private void LoadCursors()
    {
        Cursor.visible = true;

        defaultCursor = Resources.Load<Texture2D>("Cursors/defaultCursor");
        moveCursor = Resources.Load<Texture2D>("Cursors/defaultCursor");
        grabCursor = Resources.Load<Texture2D>("Cursors/grabCursor");
        attackCursor = Resources.Load<Texture2D>("Cursors/attackCursor");
        interactCursor = Resources.Load<Texture2D>("Cursors/interactCursor");
    }

    private void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = new Vector2(mouseWorld.x, mouseWorld.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                SetCursor(CursorType.Attack);
            }
            else if (hit.collider.CompareTag("Interactable"))
            {
                SetCursor(CursorType.Interact);
            }
            else
            {
                SetCursor(CursorType.Move);
            }
        }
        else
        {
            SetCursor(CursorType.Default);
        }
    }

    public void SetCursor(CursorType type)
    {
        // if (type == currentCursor) return; // prevent redundant changes

        Texture2D cursorTex = GetCursorTexture(type);
        Cursor.SetCursor(cursorTex, Vector2.zero, CursorMode.Auto);
        currentCursor = type;
    }

    private Texture2D GetCursorTexture(CursorType type)
    {
        switch (type)
        {
            case CursorType.Move: return moveCursor;
            case CursorType.Grab: return grabCursor;
            case CursorType.Attack: return attackCursor;
            case CursorType.Interact: return interactCursor;
            case CursorType.Default:
            default: return defaultCursor;
        }
    }
}