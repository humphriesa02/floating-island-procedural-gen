using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateIsland))]
public class GenerateIslandEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default fields first
        DrawDefaultInspector();

        GenerateIsland island = (GenerateIsland)target;

        GUILayout.Space(10);
        GUILayout.Label("Debug Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Island Mesh"))
        {
            island.GenerateIslandMesh();
        }

        if (GUILayout.Button("Populate Island"))
        {
            island.PopulateIsland();
        }

        if (GUILayout.Button("Clear Island"))
        {
            island.ClearIsland();
        }
    }
}
