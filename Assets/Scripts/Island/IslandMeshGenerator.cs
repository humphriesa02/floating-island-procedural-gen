using UnityEngine;

public class IslandMeshResult
{
	public Mesh Mesh;
	public Vector3[] Vertices;
	public int[] TopVertexIndices;
}

public class IslandMeshGenerator
{

	public IslandMeshResult Build(IslandGenerationData generationData){
		
		int ringSize = generationData.TotalIslandVertices + 1;
		// Index references for key vertices
		int coneTipIndex = 0;
		int bottomCenterIndex = 0;
		int topCapRingStart = 0;
		int topCenterIndex = 0;

		/* Vertices */
        Vector3[] vertices = GenerateIslandVertices(generationData, ringSize, ref coneTipIndex, ref bottomCenterIndex, ref topCapRingStart, ref topCenterIndex);

		/* Triangles */
		int[] triangles = GenerateIslandTriangles(generationData, ringSize, coneTipIndex, topCenterIndex, topCapRingStart);

        /* UVs */
		Vector2[] uvs = GenerateIslandUVs(generationData, vertices.Length, coneTipIndex, bottomCenterIndex, topCapRingStart, topCenterIndex);

		/* Colors */
		Color[] colors = GenerateIslandColors(generationData, ref vertices, ringSize, coneTipIndex, bottomCenterIndex, topCenterIndex);

		/* Apply mesh */
		var mesh = ApplyMesh(vertices, triangles, uvs, colors);

		int[] topVertexIndices = new int[ringSize];
		for (int i = 0; i < ringSize; i++) topVertexIndices[i] = i;

		return new IslandMeshResult {
			Mesh = mesh,
			Vertices = vertices,
			TopVertexIndices = topVertexIndices
		};
	}
	Mesh ApplyMesh(Vector3[] vertices , int[] triangles, Vector2[] uvs, Color[] colors){
		// Assign mesh
        var mesh = new Mesh
        {
            name = "Island Mesh",
            vertices = vertices,
            triangles = triangles,
            colors = colors,
            uv = uvs
        };
        mesh.RecalculateNormals();

        return mesh;
	}

	public Vector3[] GenerateIslandVertices(IslandGenerationData data, int ringSize, ref int coneTipIndex, ref int bottomCenterIndex, ref int topCapRingStart, ref int topCenterIndex){
		Vector3[] vertices = new Vector3[4 * ringSize + 3]; // 4 rings + cone tip + bottom center + top center
		// Keep track of where we are in the vertices array
		int vertIndex = 0;

		// Top ring of the crust
		vertIndex = CreateVertexRing(data.TotalIslandVertices, data.IslandCrustTopRadius, data.IslandCrustHeight, data.PerlinNoiseIntensity, ref vertices, vertIndex);
		// Bottom ring of the crust
		vertIndex = CreateVertexRing(data.TotalIslandVertices, data.IslandCrustBottomRadius, 0.0f, data.PerlinNoiseIntensity, ref vertices, vertIndex);
		// Inner cone ring near the bottom post point of the island
		float innerRingRadius = data.IslandCrustBottomRadius * data.InnerRingScale;
		vertIndex = CreateVertexRing(data.TotalIslandVertices, innerRingRadius, data.InnerRingY, data.PerlinNoiseIntensity, ref vertices, vertIndex, 0.1f);

		// Island base cone tip vertex
		coneTipIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, -data.IslandBaseHeight, 0);
		
		// bottom center vertex for the crust
		bottomCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, 0, 0);

		// Another ring for the top for UVs
		topCapRingStart = vertIndex;
		vertIndex = CreateVertexRing(data.TotalIslandVertices, data.IslandCrustTopRadius, data.IslandCrustHeight, data.PerlinNoiseIntensity, ref vertices, vertIndex);

		// Top center vertex for the crust
		topCenterIndex = vertIndex;
    	vertices[vertIndex++] = new Vector3(0, data.IslandCrustHeight, 0);

		return vertices;
	}

	int CreateVertexRing(int numVertices, float ringRadius, float ringHeight, float PerlinNoiseIntensity, ref Vector3[] vertices, int vertIndex, float perlinNoiseModifer=1.0f)
	{
		for (int i = 0; i <= numVertices; i++) {
			float angle = i * Mathf.PI * 2f / numVertices;
			float x = Mathf.Cos(angle) * ringRadius;
			float z = Mathf.Sin(angle) * ringRadius;
			float s = Mathf.PerlinNoise(x / ringRadius, z / ringRadius) * PerlinNoiseIntensity * perlinNoiseModifer;
			vertices[vertIndex++] = new Vector3(x + s, ringHeight + s, z + s);
		}
		return vertIndex;
	}

	public int[] GenerateIslandTriangles(IslandGenerationData data, int ringSize, int coneTipIndex, int topCenterIndex, int topCapRingStart){
		int[] triangles = new int[data.TotalIslandVertices * 18];
		// Keep track of where we are in the triangles array
		int triIndex = 0;

		// 1. Crust sides (two triangles per segment)
		for (int i = 0; i < data.TotalIslandVertices; i++)
		{
			triangles[triIndex++] = i;
			triangles[triIndex++] = ringSize + i + 1;
			triangles[triIndex++] = ringSize + i;

			triangles[triIndex++] = i;
			triangles[triIndex++] = i + 1;
			triangles[triIndex++] = ringSize + i + 1;
		}

		int bStart = ringSize;
		int iStart = ringSize * 2;
		// 2. Cone lower section: Connect bottom ring to inner cone ring.
		for (int i = 0; i < data.TotalIslandVertices; i++){
			triangles[triIndex++] = bStart + i;
			triangles[triIndex++] = bStart + i + 1;
			triangles[triIndex++] = iStart + i;

			triangles[triIndex++] = iStart + i;
			triangles[triIndex++] = bStart + i + 1;
			triangles[triIndex++] = iStart + i + 1;
    	}

		// 3. Connect inner cone ring to the cone tip
		for (int i = 0; i < data.TotalIslandVertices; i++)
		{
			triangles[triIndex++] = iStart + i;
			triangles[triIndex++] = iStart + i + 1;
			triangles[triIndex++] = coneTipIndex;
		}

		// 4. Top cap connecting top ring to top center
		for (int i = 0; i < data.TotalIslandVertices; i++)
		{
			triangles[triIndex++] = topCenterIndex;
			triangles[triIndex++] = topCapRingStart + i + 1;
			triangles[triIndex++] = topCapRingStart + i;
		}

		return triangles;
	}

	public Vector2[] GenerateIslandUVs(IslandGenerationData data, int uvSize, int coneTipIndex, int bottomCenterIndex, int topCapRingStart, int topCenterIndex){
		Vector2[] uvs = new Vector2[uvSize];
    
		int uvIndex = 0;
		for (int r = 0; r < 4; r++) {
			for (int i = 0; i <= data.TotalIslandVertices; i++) {
				float u = (float)i / data.TotalIslandVertices;
				float v = 1.0f - 0.33f * r;
				uvs[uvIndex++] = new Vector2(u, v);
			}
		}
		uvs[coneTipIndex] = new Vector2(0.5f, -1);
		uvs[bottomCenterIndex] = new Vector2(0.5f, 0.5f);
		for (int i = 0; i <= data.TotalIslandVertices; i++) {
			float angle = i * Mathf.PI * 2f / data.TotalIslandVertices;
			uvs[topCapRingStart + i] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
		}
		uvs[topCenterIndex] = new Vector2(0.5f, 0.5f);

		return uvs;
	}

	public Color[] GenerateIslandColors(IslandGenerationData data, ref Vector3[] vertices, int ringSize, int coneTipIndex, int bottomCenterIndex, int topCenterIndex){
		Color[] colors = new Color[vertices.Length];
		for (int i = 0; i < ringSize; i++) {
			float g = Mathf.PerlinNoise(vertices[i].x * 0.1f, vertices[i].z * 0.1f);
			colors[i] = Color.Lerp(data.CrustColor, data.IntermediateColor, g);
			float g2 = Mathf.PerlinNoise(vertices[ringSize + i].x * 0.1f, vertices[ringSize + i].z * 0.1f);
			colors[ringSize + i] = Color.Lerp(data.BaseColor, data.BottomColor, g2);
			colors[ringSize * 2 + i] = Color.Lerp(data.IntermediateColor, data.BottomColor, 0.5f);
		}
		colors[coneTipIndex] = data.BottomColor;
		colors[bottomCenterIndex] = data.BaseColor;
		colors[topCenterIndex] = data.CrustColor;

		return colors;
	}
}
