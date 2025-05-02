using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class WallTransparencyController : MonoBehaviour
{
    [Header("Fade Settings")]
    [Range(0f, 1f)]
    [Tooltip("Opacity of faded walls when the player is behind them")]
    [SerializeField] private float fadedAlpha = 0.4f;

    private Tilemap wallTilemap;
    private Transform player;
    private List<Vector3Int> fadedCells = new List<Vector3Int>();
    private Color originalColor;
    private Color fadedColor;
    [SerializeField] private float fadeRadius = 1.5f;

    private void Awake()
    {
        wallTilemap = GetComponent<Tilemap>();
        if (wallTilemap == null)
        {
            Debug.LogError("WallTransparencyController: No Tilemap component found. Disabling.");
            enabled = false;
            return;
        }

        originalColor = wallTilemap.color;
        fadedColor = new Color(originalColor.r, originalColor.g, originalColor.b, fadedAlpha);
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        // Restore previous fades
        foreach (var cell in fadedCells)
        {
            wallTilemap.SetTileFlags(cell, TileFlags.None);
            wallTilemap.SetColor(cell, originalColor);
        }
        fadedCells.Clear();

        // Find all tiles in a square around the player
        Vector3 worldPos = player.position;
        Vector3Int centerCell = wallTilemap.WorldToCell(worldPos);
        int radius = Mathf.CeilToInt(fadeRadius / wallTilemap.cellSize.x);

        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
            {
                var cellPos = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                if (!wallTilemap.HasTile(cellPos)) continue;

                // Check actual distance
                Vector3 cellWorld = wallTilemap.GetCellCenterWorld(cellPos);
                if (Vector3.Distance(cellWorld, worldPos) <= fadeRadius)
                {
                    wallTilemap.SetTileFlags(cellPos, TileFlags.None);
                    wallTilemap.SetColor(cellPos, fadedColor);
                    fadedCells.Add(cellPos);
                }
            }
    }

    private void LateUpdateOLD()
    {
        if (!enabled || wallTilemap == null) return;

        // Reacquire player if destroyed/missing
        if (player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
                player = GameManager.Instance.player.transform;
            else
                player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player == null) return;
        }

        // Restore previously faded tiles
        foreach (var cell in fadedCells)
        {
            wallTilemap.SetTileFlags(cell, TileFlags.None);
            wallTilemap.SetColor(cell, originalColor);
        }
        fadedCells.Clear();

        // Calculate the player's cell (offset by half tile height)
        Vector3 offsetPos = player.position + Vector3.up * (wallTilemap.cellSize.y * 0.5f);
        Vector3Int playerCell = wallTilemap.WorldToCell(offsetPos);
        Vector3 cellCenter = wallTilemap.GetCellCenterWorld(playerCell);
        Vector3 halfExtents = new Vector3(wallTilemap.cellSize.x / 2f, wallTilemap.cellSize.y / 2f, 0f);

        // Debug: draw the player's current cell in magenta
        DrawBox(cellCenter, halfExtents, Color.magenta);

        // Determine direction from cell to camera in 2D
        Vector3 cameraPos = Camera.main ? Camera.main.transform.position : Vector3.zero;
        Vector2 viewDir = ((Vector2)cameraPos - (Vector2)cellCenter).normalized;

        // Cardinal offsets to check
        Vector3Int[] offsets = new[] {
            new Vector3Int(1, 0, 0),   // East
            new Vector3Int(-1,0, 0),   // West
            new Vector3Int(0, 1, 0),    // North
            new Vector3Int(0,-1, 0)     // South
        };

        // Find the neighbor most aligned with viewDir
        Vector3Int bestOffset = Vector3Int.zero;
        float bestDot = -Mathf.Infinity;
        foreach (var off in offsets)
        {
            var neighborCell = playerCell + off;
            Vector3 neighborCenter = wallTilemap.GetCellCenterWorld(neighborCell);
            Vector2 dir = ((Vector2)neighborCenter - (Vector2)cellCenter).normalized;
            float dot = Vector2.Dot(dir, viewDir);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestOffset = off;
            }
        }

        // Fade the best neighbor cell
        Vector3Int fadeCell = playerCell + bestOffset;
        if (wallTilemap.HasTile(fadeCell))
        {
            Vector3 fadeCenter = wallTilemap.GetCellCenterWorld(fadeCell);
            DrawBox(fadeCenter, halfExtents, Color.cyan);

            wallTilemap.SetTileFlags(fadeCell, TileFlags.None);
            wallTilemap.SetColor(fadeCell, fadedColor);
            fadedCells.Add(fadeCell);
        }
    }

    private void DrawBox(Vector3 center, Vector3 halfExtents, Color color)
    {
        Vector3 bl = center + new Vector3(-halfExtents.x, -halfExtents.y);
        Vector3 br = center + new Vector3(halfExtents.x, -halfExtents.y);
        Vector3 tr = center + new Vector3(halfExtents.x, halfExtents.y);
        Vector3 tl = center + new Vector3(-halfExtents.x, halfExtents.y);
        Debug.DrawLine(bl, br, color);
        Debug.DrawLine(br, tr, color);
        Debug.DrawLine(tr, tl, color);
        Debug.DrawLine(tl, bl, color);
    }
}