#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator))]
public class EditorDungeonGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the serialised fields automatically
        DrawDefaultInspector();

        // Add your “Generate Dungeon” button
        if (GUILayout.Button("Generate Dungeon"))
        {
            // Cast target to your type, then call Generate()
            (target as DungeonGenerator)?.Generate();
        }

        // Add your “Generate Dungeon” button
        if (GUILayout.Button("Delete Dungeon"))
        {
            // Cast target to your type, then call Generate()
            (target as DungeonGenerator)?.ClearMaps();
        }
    }
}
#endif
