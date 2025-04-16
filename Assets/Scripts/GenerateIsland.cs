using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateIsland : MonoBehaviour
{
	[Header("Island Generation Crust")]
	[Tooltip("Minimum possible radius of the bottom most part of the crust.")]
	[SerializeField] private float islandCrustBottomRadiusMin = 5f;
	[Tooltip("Maximum radius of the bottom most part of the crust.")]
	[SerializeField] private float islandCrustBottomRadiusMax = 15f;
	[Tooltip("Minumum radius of the top most part of the crust.")]
	[SerializeField] private float islandCrustTopRadiusMin = 5f;
	[Tooltip("Maximum radius of the top most part of the crust.")]
	[SerializeField] private float islandCrustTopRadiusMax = 15f;
	[Tooltip("Minimum height of the island's crust (platform).")]
	[SerializeField] private float islandCrustHeightMin = 1f;
	[Tooltip("Maximum height of the island's crust (platform).")]
	[SerializeField] private float islandCrustHeightMax = 4f;
	[Header("Island Generation Base")]
	[Tooltip("Minimum height of the island's base (underneath the platform).")]
	[SerializeField] private float islandBaseHeightMin = 5f;
	[Tooltip("Maximum height of the island's base (underneath the platform).")]
	[SerializeField] private float islandBaseHeightMax = 10f;
	[Tooltip("Minimum size of the inner ring close to the cone tip.")]
	[Header("Island Generation Inner Ring")]
	[SerializeField] private float innerRingScaleMin = 0.2f; // Change this value to adjust the inner ring size.
	[Tooltip("Maximum size of the inner ring close to the cone tip.")]
	[SerializeField] private float innerRingScaleMax = 0.6f; // Change this value to adjust the inner ring size.
	[Tooltip("Minimum number of vertices in the island mesh.")]
	[Header("Island Generation Vertices")]
	[SerializeField] private int totalIslandVerticesMin = 25;
	[Tooltip("Maximum number of vertices in the island mesh.")]
	[SerializeField] private int totalIslandVerticesMax = 100;
	[Header("Island Generation Colors")]
	[Tooltip("Color of the island's crust.")]
	[SerializeField] private Color crustColor = Color.green;
	[Tooltip("Color of the island's base.")]
	[SerializeField] private Color baseColor = new Color(0.58f, 0.3f, 0f / 255f);
	[Tooltip("Color of the island's intermediate.")]
	[SerializeField] private Color intermediateColor = new Color(1.0f, 0.88f, 0.75f);
	[Tooltip("Color for the bottom most part of the base.")]
	[SerializeField] private Color bottomColor = new Color(0.75f, 0.25f, 0f);
	[Header("Island Generation Noise")]
	[Tooltip("Minimum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMin = 2.0f; // Amount of randomness to add to the island shape
	[Tooltip("Maximum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMax = 2.0f; // Amount of randomness to add to the island shape

	[Header("Island Population Settings")]
	[Tooltip("The possible objects to be spawned on the island.")]
    [SerializeField] private GameObject[] possibleObjects; // Objects that can be spawned on an island
	[Tooltip("The minimum distance between objects on the island.")]
	[SerializeField] private float minDistanceBetweenObjects = 2.0f; // Minimum distance between objects on the island
	[Tooltip("The maximum distance between objects on the island.")]
	[SerializeField] private float maxDistanceBetweenObjects = 5.0f; // Minimum distance between objects on the island
	[Tooltip("The minimum number of objects to spawn on the island.")]
	[SerializeField] private int minObjectsToSpawn = 5; // Minimum number of objects to spawn on the island
	[Tooltip("The maximum number of objects to spawn on the island.")]
	[SerializeField] private int maxObjectsToSpawn = 10; // Maximum number of objects to spawn on the island


	public float IslandCrustBottomRadius { get; private set; }
	public float IslandBaseHeight { get; private set; }
	public float IslandCrustTopRadius { get; private set; }
	public float IslandCrustHeight { get; private set; }
	public int TotalIslandVertices { get; private set; }
	public float PerlinNoiseIntensity { get; private set; }
	public float InnerRingY { get; private set; }
	public float InnerRingScale { get; private set; }

	public float GetRadius(){
		return Mathf.Max(IslandCrustBottomRadius, IslandCrustTopRadius);
	}

	public float GetHeight(){
		return IslandBaseHeight + IslandCrustHeight;
	}

    void Start()
    {
		GenerateIslandMesh();
		PopulateIsland();
    }

	public void GenerateIslandMesh(){
		// Randomly choose from min/max values
		IslandCrustBottomRadius = Random.Range(islandCrustBottomRadiusMin, islandCrustBottomRadiusMax);
		IslandBaseHeight = Random.Range(islandBaseHeightMin, islandBaseHeightMax);
		IslandCrustTopRadius = Random.Range(islandCrustTopRadiusMin, islandCrustTopRadiusMax);
		IslandCrustHeight = Random.Range(islandCrustHeightMin, islandCrustHeightMax);
		TotalIslandVertices = Random.Range(totalIslandVerticesMin, totalIslandVerticesMax);
		PerlinNoiseIntensity = Random.Range(perlinNoiseIntensityMin, perlinNoiseIntensityMax);
		float yPos = Random.Range(0.5f, 0.9f);
		InnerRingY = Mathf.Lerp(0, -IslandBaseHeight, yPos);
		InnerRingScale = Random.Range(innerRingScaleMin, innerRingScaleMax);
		int ringSize = TotalIslandVertices + 1;

		// Index references for key vertices
		int coneTipIndex = 0;
		int bottomCenterIndex = 0;
		int topCapRingStart = 0;
		int topCenterIndex = 0;

		/* Vertices */
        Vector3[] vertices = GenerateIslandVertices(ringSize, ref coneTipIndex, ref bottomCenterIndex, ref topCapRingStart, ref topCenterIndex);

		/* Triangles */
		int[] triangles = GenerateIslandTriangles(ringSize, coneTipIndex, topCenterIndex, topCapRingStart);

        /* UVs */
		Vector2[] uvs = GenerateIslandUVs(vertices.Length, coneTipIndex, bottomCenterIndex, topCapRingStart, topCenterIndex);

		/* Colors */
		Color[] colors = GenerateIslandColors(ref vertices, ringSize, coneTipIndex, bottomCenterIndex, topCenterIndex);

		/* Apply mesh */
		ApplyMesh(vertices, triangles, uvs, colors);

		// To note, the material is set before runtime,
		// requires the colors set here.
	}
	void ApplyMesh(Vector3[] vertices , int[] triangles, Vector2[] uvs, Color[] colors){
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

        GetComponent<MeshFilter>().mesh = mesh;
	}

	Vector3[] GenerateIslandVertices(int ringSize, ref int coneTipIndex, ref int bottomCenterIndex, ref int topCapRingStart, ref int topCenterIndex){
		Vector3[] vertices = new Vector3[4 * ringSize + 3]; // 4 rings + cone tip + bottom center + top center
		// Keep track of where we are in the vertices array
		int vertIndex = 0;

		// Top ring of the crust
		vertIndex = CreateVertexRing(TotalIslandVertices, IslandCrustTopRadius, IslandCrustHeight, PerlinNoiseIntensity, ref vertices, vertIndex);
		// Bottom ring of the crust
		vertIndex = CreateVertexRing(TotalIslandVertices, IslandCrustBottomRadius, 0.0f, PerlinNoiseIntensity, ref vertices, vertIndex);
		// Inner cone ring near the bottom post point of the island
		float innerRingRadius = IslandCrustBottomRadius * InnerRingScale;
		vertIndex = CreateVertexRing(TotalIslandVertices, innerRingRadius, InnerRingY, PerlinNoiseIntensity, ref vertices, vertIndex, 0.1f);

		// Island base cone tip vertex
		coneTipIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, -IslandBaseHeight, 0);
		
		// bottom center vertex for the crust
		bottomCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, 0, 0);

		// Another ring for the top for UVs
		topCapRingStart = vertIndex;
		vertIndex = CreateVertexRing(TotalIslandVertices, IslandCrustTopRadius, IslandCrustHeight, PerlinNoiseIntensity, ref vertices, vertIndex);

		// Top center vertex for the crust
		topCenterIndex = vertIndex;
    	vertices[vertIndex++] = new Vector3(0, IslandCrustHeight, 0);

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

	int[] GenerateIslandTriangles(int ringSize, int coneTipIndex, int topCenterIndex, int topCapRingStart){
		int[] triangles = new int[TotalIslandVertices * 18];
		// Keep track of where we are in the triangles array
		int triIndex = 0;

		// 1. Crust sides (two triangles per segment)
		for (int i = 0; i < TotalIslandVertices; i++)
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
		for (int i = 0; i < TotalIslandVertices; i++){
			triangles[triIndex++] = bStart + i;
			triangles[triIndex++] = bStart + i + 1;
			triangles[triIndex++] = iStart + i;

			triangles[triIndex++] = iStart + i;
			triangles[triIndex++] = bStart + i + 1;
			triangles[triIndex++] = iStart + i + 1;
    	}

		// 3. Connect inner cone ring to the cone tip
		for (int i = 0; i < TotalIslandVertices; i++)
		{
			triangles[triIndex++] = iStart + i;
			triangles[triIndex++] = iStart + i + 1;
			triangles[triIndex++] = coneTipIndex;
		}

		// 4. Top cap connecting top ring to top center
		for (int i = 0; i < TotalIslandVertices; i++)
		{
			triangles[triIndex++] = topCenterIndex;
			triangles[triIndex++] = topCapRingStart + i + 1;
			triangles[triIndex++] = topCapRingStart + i;
		}

		return triangles;
	}

	Vector2[] GenerateIslandUVs(int uvSize, int coneTipIndex, int bottomCenterIndex, int topCapRingStart, int topCenterIndex){
		Vector2[] uvs = new Vector2[uvSize];
    
		int uvIndex = 0;
		for (int r = 0; r < 4; r++) {
			for (int i = 0; i <= TotalIslandVertices; i++) {
				float u = (float)i / TotalIslandVertices;
				float v = 1.0f - 0.33f * r;
				uvs[uvIndex++] = new Vector2(u, v);
			}
		}
		uvs[coneTipIndex] = new Vector2(0.5f, -1);
		uvs[bottomCenterIndex] = new Vector2(0.5f, 0.5f);
		for (int i = 0; i <= TotalIslandVertices; i++) {
			float angle = i * Mathf.PI * 2f / TotalIslandVertices;
			uvs[topCapRingStart + i] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
		}
		uvs[topCenterIndex] = new Vector2(0.5f, 0.5f);

		return uvs;
	}

	Color[] GenerateIslandColors(ref Vector3[] vertices, int ringSize, int coneTipIndex, int bottomCenterIndex, int topCenterIndex){
		Color[] colors = new Color[vertices.Length];
		for (int i = 0; i < ringSize; i++) {
			float g = Mathf.PerlinNoise(vertices[i].x * 0.1f, vertices[i].z * 0.1f);
			colors[i] = Color.Lerp(crustColor, intermediateColor, g);
			float g2 = Mathf.PerlinNoise(vertices[ringSize + i].x * 0.1f, vertices[ringSize + i].z * 0.1f);
			colors[ringSize + i] = Color.Lerp(baseColor, bottomColor, g2);
			colors[ringSize * 2 + i] = Color.Lerp(intermediateColor, bottomColor, 0.5f);
		}
		colors[coneTipIndex] = bottomColor;
		colors[bottomCenterIndex] = baseColor;
		colors[topCenterIndex] = crustColor;

		return colors;
	}

	void OnDrawGizmosSelected() {
		var mesh = GetComponent<MeshFilter>().sharedMesh;
		if (!mesh) return;
		Gizmos.color = Color.yellow;
		foreach (var v in mesh.vertices) {
			Gizmos.DrawSphere(transform.TransformPoint(v), 0.1f);
		}
	}

	[ContextMenu("Generate Island")]
	void GenerateIslandContextMenu(){
		GenerateIslandMesh();
	}
	[ContextMenu("Populate Island")]
	void PopulateIslandContextMenu(){
		PopulateIsland();
	}

	[ContextMenu("Clear Island")]
	void ClearIslandContextMenu(){
		ClearIsland();
	}

	public void PopulateIsland(){
		if (possibleObjects.Length == 0) return;

		float islandRadius = IslandCrustTopRadius;
		float yHeight = IslandCrustHeight;
		int objectsToSpawn = Random.Range(minObjectsToSpawn, maxObjectsToSpawn);
		List<Vector3> usedPositions = new List<Vector3>();
		for (int i = 0; i < objectsToSpawn; i++)
		{
			GameObject prefab = possibleObjects[Random.Range(0, possibleObjects.Length)];
			float uniformScale = Random.Range(0.5f, 2.0f);
			float distanceBetweenObjects = uniformScale * Random.Range(minDistanceBetweenObjects, maxDistanceBetweenObjects);
			for (int attempt = 0; attempt < 10; attempt++){
				Vector2 rand = Random.insideUnitCircle * islandRadius * 0.6f;
				Vector3 candidatePos = new Vector3(rand.x, yHeight, rand.y);

				bool tooClose = false;
				for (int j = 0; j < usedPositions.Count; j++)
				{
					if (Vector3.Distance(candidatePos, usedPositions[j]) < distanceBetweenObjects)
					{
						tooClose = true;
						break;
					}
				}
				if (tooClose) continue;
				usedPositions.Add(candidatePos);
				GameObject obj = Instantiate(prefab);
				obj.transform.position = transform.TransformPoint(candidatePos);
				obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
				obj.transform.SetParent(transform);
				break;
			}
		}
	}

	public void ClearIsland(){
		var mesh = GetComponent<MeshFilter>().sharedMesh;
		if (mesh) DestroyImmediate(mesh);
		GetComponent<MeshFilter>().mesh = null;
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}
}
