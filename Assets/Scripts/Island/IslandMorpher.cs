using UnityEngine;

public class IslandMorpher
{
    private Vector3[] originalVertices;
    public void ApplyStatMorph(Mesh mesh, IslandStats stats, int[] topVertexIndices)
    {
        if (mesh == null || stats == null || topVertexIndices == null) return;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < topVertexIndices.Length; i++)
        {
            int index = topVertexIndices[i];
            Vector3 v = vertices[index];

            float offset = 0f;

            // FOOD → smoother & raised
            offset += stats.Food * 0.05f;

            // DANGER → chaotic noise
            offset += Mathf.PerlinNoise(v.x * 2f, v.z * 2f) * stats.Danger * 0.02f;

            // PEOPLE → flattened out
            offset -= Mathf.Abs(v.x * v.z) * stats.People * 0.001f;

            // DEFENSE → perimeter bumps
            offset += Mathf.Sin(v.x + v.z) * stats.Defense * 0.005f;

            Vector3 original = vertices[index];
            for (int j = 0; j < vertices.Length; j++)
            {
                if (Vector3.Distance(vertices[j], original) < 0.001f)
                {
                    vertices[j].y += offset;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    public void DrawMorphDebugGizmos(Transform islandTransform, Mesh mesh)
    {
        if (originalVertices == null || mesh == null) return;

        Vector3[] modifiedVertices = mesh.vertices;
        Gizmos.color = Color.cyan;

        for (int i = 0; i < originalVertices.Length && i < modifiedVertices.Length; i++)
        {
            Vector3 from = islandTransform.TransformPoint(originalVertices[i]);
            Vector3 to = islandTransform.TransformPoint(modifiedVertices[i]);
            Gizmos.DrawLine(from, to);
        }
    }
}
