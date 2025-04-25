#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "FloorAwareIsometricRuleTile",
                 menuName = "2D/Tiles/Floor Aware Iso Rule Tile")]
#endif
public class FloorAwareIsometricRuleTile : IsometricRuleTile
{
    [Tooltip("Drag your Floor Tilemap here at runtime.")]
    public Tilemap floorTilemap;

    // Override the only virtual neighbor-matching entry point
    public override bool RuleMatches(TilingRule rule,
                                     Vector3Int position,
                                     ITilemap tilemap,
                                     ref Matrix4x4 transform)
    {
        // We’re not doing any rotations/mirrors here:
        transform = Matrix4x4.identity;

        int count = Mathf.Min(rule.m_Neighbors.Count,
                              rule.m_NeighborPositions.Count);
        for (int i = 0; i < count; i++)
        {
            int neighborFlag = rule.m_Neighbors[i];
            Vector3Int offset = rule.m_NeighborPositions[i];
            Vector3Int checkPos = position + offset;

            // ONLY check the floor tilemap now:
            bool hasFloor = floorTilemap != null
                         && floorTilemap.HasTile(checkPos);

            // If the rule says “This” but there’s no floor → fail
            if (neighborFlag == TilingRuleOutput.Neighbor.This
                && !hasFloor)
                return false;

            // If the rule says “NotThis” but there IS a floor → fail
            if (neighborFlag == TilingRuleOutput.Neighbor.NotThis
                && hasFloor)
                return false;

            // All other flags (“Don’t care”) pass
        }

        return true;
    }
}
