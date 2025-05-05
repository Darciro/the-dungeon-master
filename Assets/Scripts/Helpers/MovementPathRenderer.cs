using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MovementPathRenderer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("LineRenderer used to draw the path.")]
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Dashed Line Settings")]
    [Tooltip("Material with a 1D dashed texture (white dashes on transparent).")]
    [SerializeField] private Material dashedMaterial;
    [Tooltip("How many world-units correspond to one repetition of the dash texture.")]
    [SerializeField] private float unitsPerTextureRepeat = 1f;

    private void Reset()
    {
        // auto-assign if you forget in Inspector
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
    }

    private void Awake()
    {
        // make sure the LR is set up for tiling
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.alignment = LineAlignment.TransformZ;
        lineRenderer.material = dashedMaterial;
    }

    /// <summary>
    /// Draws a dashed path through the given world positions.
    /// </summary>
    public void DrawPath(Vector3[] worldPositions)
    {
        if (worldPositions == null || worldPositions.Length < 2)
        {
            ClearPath();
            return;
        }

        // 1) Set points
        lineRenderer.positionCount = worldPositions.Length;
        lineRenderer.SetPositions(worldPositions);

        // 2) Compute total length
        float totalLen = 0f;
        for (int i = 1; i < worldPositions.Length; i++)
            totalLen += Vector3.Distance(worldPositions[i - 1], worldPositions[i]);

        // 3) Tile the texture so the dashes repeat along the length
        float repeatCount = totalLen / unitsPerTextureRepeat;
        lineRenderer.material.mainTextureScale = new Vector2(repeatCount, 1);
    }

    /// <summary>Hides the line.</summary>
    public void ClearPath()
    {
        lineRenderer.positionCount = 0;
    }
}
