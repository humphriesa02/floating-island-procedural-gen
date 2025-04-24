// --- IslandLODGenerator.cs ---
using UnityEngine;

public class IslandLODGenerator
{
    public GameObject GenerateLODIsland(IslandGenerationData data, Material lodMaterial)
    {
        GameObject lodIsland = new GameObject("Island_LOD1");

        Mesh mesh = BuildSimpleIsland(data);

        MeshFilter mf = lodIsland.AddComponent<MeshFilter>();
        MeshRenderer mr = lodIsland.AddComponent<MeshRenderer>();

        mf.sharedMesh = mesh;
        mr.sharedMaterial = lodMaterial;

        return lodIsland;
    }

    private Mesh BuildSimpleIsland(IslandGenerationData data)
    {
        int ringVerts = Mathf.Max(8, data.TotalIslandVertices / 3);
        int ringSize = ringVerts + 1;

        Vector3[] vertices = new Vector3[ringSize * 2 + 2];
        Vector2[] uvs = new Vector2[vertices.Length];

        int vertIndex = 0;

        float topY = data.IslandCrustHeight * 0.6f;
        float bottomY = 0f;

        for (int i = 0; i <= ringVerts; i++)
        {
            float angle = i * Mathf.PI * 2f / ringVerts;
            float x = Mathf.Cos(angle) * data.IslandCrustTopRadius;
            float z = Mathf.Sin(angle) * data.IslandCrustTopRadius;
            vertices[vertIndex] = new Vector3(x, topY, z);
            uvs[vertIndex] = new Vector2((Mathf.Cos(angle) + 1f) * 0.5f, (Mathf.Sin(angle) + 1f) * 0.5f);
            vertIndex++;
        }

        for (int i = 0; i <= ringVerts; i++)
        {
            float angle = i * Mathf.PI * 2f / ringVerts;
            float x = Mathf.Cos(angle) * data.IslandCrustBottomRadius;
            float z = Mathf.Sin(angle) * data.IslandCrustBottomRadius;
            vertices[vertIndex] = new Vector3(x, bottomY, z);
            uvs[vertIndex] = new Vector2((Mathf.Cos(angle) + 1f) * 0.5f, (Mathf.Sin(angle) + 1f) * 0.5f);
            vertIndex++;
        }

        int bottomCenter = vertIndex;
        vertices[vertIndex] = new Vector3(0, bottomY, 0);
        uvs[vertIndex++] = new Vector2(0.5f, 0.5f);

        int topCenter = vertIndex;
        vertices[vertIndex] = new Vector3(0, topY, 0);
        uvs[vertIndex++] = new Vector2(0.5f, 0.5f);

        Mesh mesh = new Mesh();
        mesh.name = "Island_LOD_Mesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;

        int[] triangles = new int[ringVerts * 12];
        int tri = 0;

        for (int i = 0; i < ringVerts; i++)
        {
            int topA = i;
            int topB = i + 1;
            int bottomA = ringSize + i;
            int bottomB = ringSize + i + 1;

            triangles[tri++] = topA;
            triangles[tri++] = bottomB;
            triangles[tri++] = bottomA;

            triangles[tri++] = topA;
            triangles[tri++] = topB;
            triangles[tri++] = bottomB;
        }

        for (int i = 0; i < ringVerts; i++)
        {
            triangles[tri++] = bottomCenter;
            triangles[tri++] = ringSize + i + 1;
            triangles[tri++] = ringSize + i;
        }

        for (int i = 0; i < ringVerts; i++)
        {
            triangles[tri++] = topCenter;
            triangles[tri++] = i;
            triangles[tri++] = i + 1;
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
