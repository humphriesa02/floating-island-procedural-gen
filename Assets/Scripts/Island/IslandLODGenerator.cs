using UnityEngine;

public class IslandLODGenerator
{
    public GameObject GenerateLODIsland(IslandGenerationData data, Material lodMaterial)
    {
        GameObject lodIsland = new GameObject("LOD1");

        Mesh mesh = GenerateSimplifiedMesh(data);

        MeshFilter filter = lodIsland.AddComponent<MeshFilter>();
        MeshRenderer renderer = lodIsland.AddComponent<MeshRenderer>();

        filter.sharedMesh = mesh;
        renderer.sharedMaterial = lodMaterial;

        return lodIsland;
    }

    private Mesh GenerateSimplifiedMesh(IslandGenerationData data)
    {
        int reducedVerts = Mathf.Max(8, data.TotalIslandVertices / 2);
        int ringSize = reducedVerts + 1;

        Vector3[] vertices = new Vector3[ringSize * 2 + 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int vertIndex = 0;

        float topY = data.IslandCrustHeight;
        float bottomY = 0f;

        vertIndex = FillRing(vertices, uvs, vertIndex, reducedVerts, data.IslandCrustTopRadius, topY);
        vertIndex = FillRing(vertices, uvs, vertIndex, reducedVerts, data.IslandCrustBottomRadius, bottomY);

        int bottomCenter = vertIndex;
        vertices[vertIndex] = new Vector3(0, bottomY, 0);
        uvs[vertIndex++] = new Vector2(0.5f, 0.5f);

        int topCenter = vertIndex;
        vertices[vertIndex] = new Vector3(0, topY, 0);
        uvs[vertIndex++] = new Vector2(0.5f, 0.5f);

        Mesh mesh = new Mesh();
        mesh.name = "Island LOD Mesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = GenerateTriangles(reducedVerts, ringSize, bottomCenter, topCenter);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private int FillRing(Vector3[] vertices, Vector2[] uvs, int startIndex, int segments, float radius, float y)
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[startIndex] = new Vector3(x, y, z);
            uvs[startIndex++] = new Vector2((x / radius + 1f) * 0.5f, (z / radius + 1f) * 0.5f);
        }
        return startIndex;
    }

    private int[] GenerateTriangles(int segments, int ringSize, int bottomCenter, int topCenter)
    {
        int[] triangles = new int[segments * 12];
        int tri = 0;

        for (int i = 0; i < segments; i++)
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

            triangles[tri++] = bottomCenter;
            triangles[tri++] = ringSize + i + 1;
            triangles[tri++] = ringSize + i;

            triangles[tri++] = topCenter;
            triangles[tri++] = i;
            triangles[tri++] = i + 1;
        }

        return triangles;
    }

}
