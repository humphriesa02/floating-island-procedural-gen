using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IslandManager))]
public class IslandManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default fields first
        DrawDefaultInspector();

        IslandManager island = (IslandManager)target;

        GUILayout.Space(10);
        GUILayout.Label("Debug Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Island Mesh"))
        {
            island.Generate();
        }

        if (GUILayout.Button("Build Island Mesh"))
        {
            island.Build();
        }

        if (GUILayout.Button("Populate Island"))
        {
            island.Populate();
        }

        if (GUILayout.Button("Create Island"))
        {
            island.CreateIsland("None", true);
        }

        if (GUILayout.Button("Clear Island"))
        {
            island.ClearIsland();
        }
    }
}
