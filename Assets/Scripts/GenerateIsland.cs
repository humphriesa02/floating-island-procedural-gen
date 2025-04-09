using UnityEditor.EditorTools;
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
	[SerializeField] private Color baseColor = new Color(150f / 255f, 75f / 255f, 0f / 255f);
	[Tooltip("Color of the island's intermediate.")]
	[SerializeField] private Color intermediateColor = new Color(255f / 255f, 224f / 255f, 189f / 255f);
	[Tooltip("Color for the bottom most part of the base.")]
	[SerializeField] private Color bottomColor = new Color(192f / 255f, 64f / 255f, 0f / 255f);
	[Header("Island Generation Noise")]
	[Tooltip("Minimum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMin = 2.0f; // Amount of randomness to add to the island shape
	[Tooltip("Maximum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMax = 2.0f; // Amount of randomness to add to the island shape

	[Header("Island Population Settings")]
	[Tooltip("The possible objects to be spawned on the island.")]
    [SerializeField] private GameObject[] possibleObjects; // Objects that can be spawned on an island
    [SerializeField] private GameObject[] spawnLocations; // Locations to spawn the objects on the island


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
		//PopulateIsland();
    }

	void GenerateIslandMesh(){
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

		/* Vertices */

        // Vertices we need to create the island shape
        Vector3[] vertices = new Vector3[4 * TotalIslandVertices + 3]; // 4 rings + cone tip + bottom center + top center

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
		int coneTipIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, -IslandBaseHeight, 0);
		
		// bottom center vertex for the crust
		int bottomCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, 0, 0);

		// Another ring for the top for UVs
		int topCapRingStart = vertIndex;
		vertIndex = CreateVertexRing(TotalIslandVertices, IslandCrustTopRadius, IslandCrustHeight, PerlinNoiseIntensity, ref vertices, vertIndex);

		// Top center vertex for the crust
		int topCenterIndex = vertIndex;
    	vertices[vertIndex++] = new Vector3(0, IslandCrustHeight, 0);

		/* Triangles */
		int crustTriangleCount = TotalIslandVertices * 6;
		int topCapTriangleCount = TotalIslandVertices * 3;
		int coneBottomInnerTriangleCount = TotalIslandVertices * 6;
		int coneInnerTipTriangleCount = TotalIslandVertices * 3;
		int totalTrianglesCount = crustTriangleCount + coneBottomInnerTriangleCount + coneInnerTipTriangleCount + topCapTriangleCount;
		int[] triangles = new int[totalTrianglesCount];
		// Keep track of where we are in the triangles array
		int triIndex = 0;

		// 1. Crust sides (two triangles per segment)
		for (int i = 0; i < TotalIslandVertices; i++)
		{
			int next = (i + 1) % TotalIslandVertices;
			// First triangle
			triangles[triIndex++] = i;
			triangles[triIndex++] = TotalIslandVertices + next;
			triangles[triIndex++] = TotalIslandVertices + i;

			// Second triangle
			triangles[triIndex++] = i;
			triangles[triIndex++] = next;
			triangles[triIndex++] = TotalIslandVertices + next;
		}

		// 2. Cone lower section: Connect bottom ring to inner cone ring.
		for (int i = 0; i < TotalIslandVertices; i++){
			int next = (i + 1) % TotalIslandVertices;
			int bottomCurrent = TotalIslandVertices + i;
			int bottomNext = TotalIslandVertices + next;
			int innerCurrent = TotalIslandVertices * 2 + i;
			int innerNext = TotalIslandVertices * 2 + next;
			// Two triangles for the quad between bottom and inner rings:
			triangles[triIndex++] = bottomCurrent;
			triangles[triIndex++] = bottomNext;
			triangles[triIndex++] = innerCurrent;

			triangles[triIndex++] = innerCurrent;
			triangles[triIndex++] = bottomNext;
			triangles[triIndex++] = innerNext;
    	}

		// 3. Connect inner cone ring to the cone tip
		for (int i = 0; i < TotalIslandVertices; i++)
		{
			int next = (i + 1) % TotalIslandVertices;
			int innerCurrent = TotalIslandVertices * 2 + i;
			int innerNext = TotalIslandVertices * 2 + next;
			// One triangle per segment that brings the inner ring to the tip:
			triangles[triIndex++] = innerCurrent;
			triangles[triIndex++] = innerNext;
			triangles[triIndex++] = coneTipIndex;
		}

		// 4. Top cap connecting top ring to top center
		for (int i = 0; i < TotalIslandVertices; i++)
		{
			int next = (i + 1) % TotalIslandVertices;
			// Use the top cap ring indices instead of [i].
			int ringCurrent = topCapRingStart + i;
			int ringNext = topCapRingStart + next;

			triangles[triIndex++] = topCenterIndex;
			triangles[triIndex++] = ringNext;
			triangles[triIndex++] = ringCurrent;
		}

        /* UVs */
		Vector2[] uvs = new Vector2[vertices.Length];
    
		// For the side rings (cylindrical mapping):
		// Top ring (side): indices 0 .. TotalIslandVertices-1, V = 1.
		for (int i = 0; i < TotalIslandVertices; i++){
			float angle = i * Mathf.PI * 2f / TotalIslandVertices;
			float u = angle / (Mathf.PI * 2f);
			uvs[i] = new Vector2(u, 1.0f);
		}
		uvs[TotalIslandVertices - 1] = new Vector2(1.0f, 1.0f);
		// Bottom ring: indices TotalIslandVertices .. 2*TotalIslandVertices-1, V = 0.
		for (int i = TotalIslandVertices; i < 2 * TotalIslandVertices; i++){
			int j = i - TotalIslandVertices;
			float angle = j * Mathf.PI * 2f / TotalIslandVertices;
			float u = angle / (Mathf.PI * 2f);
			uvs[i] = new Vector2(u, 0.0f);
		}
		uvs[2 * TotalIslandVertices - 1] = new Vector2(1.0f, 0.0f);
		// Inner ring: indices 2*TotalIslandVertices .. 3*TotalIslandVertices-1.
		// For simplicity we set V = -0.5 (adjust as needed).
		for (int i = 2 * TotalIslandVertices; i < 3 * TotalIslandVertices; i++){
			int j = i - 2 * TotalIslandVertices;
			float angle = j * Mathf.PI * 2f / TotalIslandVertices;
			float u = angle / (Mathf.PI * 2f);
			uvs[i] = new Vector2(u, -0.5f);
		}
		uvs[3 * TotalIslandVertices - 1] = new Vector2(1.0f, -0.5f);
		// For the top cap ring: use radial UV mapping (polar coordinates).
		// Map the center (0,0) to (0.5,0.5) and use the cosine/sine to get the UVs.
		for (int i = topCapRingStart; i < topCapRingStart + TotalIslandVertices; i++){
			int j = i - topCapRingStart;
			float angle = j * Mathf.PI * 2f / TotalIslandVertices;
			float u = (Mathf.Cos(angle) + 1) * 0.5f;
			float v = (Mathf.Sin(angle) + 1) * 0.5f;
			uvs[i] = new Vector2(u, v);
		}
		
		// Assign fixed UVs for cone tip, bottom center, and top center.
		uvs[coneTipIndex] = new Vector2(0.5f, -1.0f);
		uvs[bottomCenterIndex] = new Vector2(0.5f, 0.5f);
		uvs[topCenterIndex] = new Vector2(0.5f, 0.5f);


		/* Colors */
        Color[] colors = new Color[vertices.Length];
		for (int i = 0; i < TotalIslandVertices; i++){
			// Use a gradient for the top ring.
			float gradient = Mathf.PerlinNoise(vertices[i].x * 0.1f, vertices[i].z * 0.1f);
			colors[i] = Color.Lerp(crustColor, intermediateColor, gradient); // Top ring color
		}
		for (int i = TotalIslandVertices; i < TotalIslandVertices * 2; i++){
			float gradient = Mathf.PerlinNoise(vertices[i].x * 0.1f, vertices[i].z * 0.1f);
			colors[i] = Color.Lerp(baseColor, bottomColor, gradient); // Bottom ring color
		}
		for (int i = TotalIslandVertices * 2; i < TotalIslandVertices * 3; i++){
			// Inner ring could be an intermediate color.
			colors[i] = Color.Lerp(intermediateColor, bottomColor, 0.5f);
		}
		colors[coneTipIndex] = bottomColor;
		colors[bottomCenterIndex] = baseColor;
		colors[topCenterIndex] = crustColor;
        
 		// Assign mesh
		var mesh = new Mesh {
			name = "Island Mesh"
		};
        mesh.vertices = vertices;
        mesh.triangles = triangles;
		mesh.colors = colors;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

		// To note, the material is set before runtime,
		// requires the colors set here.
	}

	int CreateVertexRing(int numVertices, float ringRadius, float ringHeight, float PerlinNoiseIntensity, ref Vector3[] vertices, int vertIndex, float perlinNoiseModifer=1.0f)
	{
		for (int i = 0; i < numVertices; i++)
		{
			float angle = i * Mathf.PI * 2f / numVertices;
			float x = Mathf.Cos(angle) * ringRadius;
			float z = Mathf.Sin(angle) * ringRadius;
			float sample = Mathf.PerlinNoise(x / ringRadius, z / ringRadius) * PerlinNoiseIntensity * perlinNoiseModifer; // Perlin noise for randomness
			vertices[vertIndex++] = new Vector3(x + sample, ringHeight + sample, z + sample);
		}
		return vertIndex;
	}

	void PopulateIsland(){
		for (int i = 0; i < spawnLocations.Length; i++)
		{
			if (possibleObjects.Length > 0)
			{
				// Choose a random object from possibleObjects
				GameObject randomObject = possibleObjects[Random.Range(0, possibleObjects.Length)];

				// Instantiate the object as a child of the spawn location
				GameObject tempObj = Instantiate(randomObject, spawnLocations[i].transform.position, Quaternion.identity, spawnLocations[i].transform);
				tempObj.transform.Rotate(0, Random.Range(0, 360), 0);
			}
		}
	}
}
