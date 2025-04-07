using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateIsland : MonoBehaviour
{
	[Header("Island Generation Settings")]
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
	[Tooltip("Minimum height of the island's base (underneath the platform).")]
	[SerializeField] private float islandBaseHeightMin = 5f;
	[Tooltip("Maximum height of the island's base (underneath the platform).")]
	[SerializeField] private float islandBaseHeightMax = 10f;
	[Tooltip("Minimum number of vertices in the island mesh.")]
	[SerializeField] private int totalIslandVerticesMin = 25;
	[Tooltip("Maximum number of vertices in the island mesh.")]
	[SerializeField] private int totalIslandVerticesMax = 100; 
	[Tooltip("Color of the island's crust.")]
	[SerializeField] private Color crustColor = Color.green;
	[Tooltip("Color of the island's base.")]
	[SerializeField] private Color baseColor = new Color(150f / 255f, 75f / 255f, 0f / 255f);
	[Tooltip("Color for the bottom most part of the base.")]
	[SerializeField] private Color bottomColor = new Color(192f / 255f, 64f / 255f, 0f / 255f);
	[Tooltip("Minimum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMin = 2.0f; // Amount of randomness to add to the island shape
	[Tooltip("Maximum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMax = 2.0f; // Amount of randomness to add to the island shape

	[Header("Island Population Settings")]
	[Tooltip("The possible objects to be spawned on the island.")]
    [SerializeField] private GameObject[] possibleObjects; // Objects that can be spawned on an island
    [SerializeField] private GameObject[] spawnLocations; // Locations to spawn the objects on the island

	

    void Start()
    {
		GenerateIslandMesh();
		PopulateIsland();
    }

	void GenerateIslandMesh(){
		// Generate random values for the island parameters
		float islandCrustBottomRadius = Random.Range(islandCrustBottomRadiusMin, islandCrustBottomRadiusMax);
		float islandBaseHeight = Random.Range(islandBaseHeightMin, islandBaseHeightMax);
		float islandCrustTopRadius = Random.Range(islandCrustTopRadiusMin, islandCrustTopRadiusMax);
		float islandCrustHeight = Random.Range(islandCrustHeightMin, islandCrustHeightMax);
		int totalIslandVertices = Random.Range(totalIslandVerticesMin, totalIslandVerticesMax);
		float perlinNoiseIntensity = Random.Range(perlinNoiseIntensityMin, perlinNoiseIntensityMax);
		Debug.Log($"Island Parameters: Bottom Radius: {islandCrustBottomRadius}, Base Height: {islandBaseHeight}, Top Radius: {islandCrustTopRadius}, Crust Height: {islandCrustHeight}, Vertices: {totalIslandVertices}, Perlin Noise Intensity: {perlinNoiseIntensity}");
		/* Vertices */

        // Vertices we need to create the island shape
        Vector3[] vertices = new Vector3[totalIslandVertices * 2 + 3]; // 2 rings + cone tip + bottom center + top center

		// Keep track of where we are in the vertices array
		int vertIndex = 0;


       	// Top ring (upper cylinder)
        for (int i = 0; i < totalIslandVertices; i++)
        {
            float angle = i * Mathf.PI * 2f / totalIslandVertices;
            float x = Mathf.Cos(angle) * islandCrustTopRadius;
            float z = Mathf.Sin(angle) * islandCrustTopRadius;
			float sample = Mathf.PerlinNoise(x / islandCrustTopRadius, z / islandCrustTopRadius) * perlinNoiseIntensity; // Perlin noise for randomness
			Debug.Log(sample);
            vertices[vertIndex++] = new Vector3(x + sample, islandCrustHeight + sample, z + sample);
        }

		// Bottom ring (lower cylinder)
        for (int i = 0; i < totalIslandVertices; i++)
        {
            float angle = i * Mathf.PI * 2f / totalIslandVertices;
            float x = Mathf.Cos(angle) * islandCrustBottomRadius;
            float z = Mathf.Sin(angle) * islandCrustBottomRadius;
			float sample = Mathf.PerlinNoise(x / islandCrustBottomRadius, z / islandCrustBottomRadius) * perlinNoiseIntensity;
            vertices[vertIndex++] = new Vector3(x + sample, sample, z + sample); // Bottom ring at y = 0
        }
		// Island base tip vertex
		int coneTipIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, -islandBaseHeight, 0);
		
		// Top face of the cylinder vertices
		// Bottom center
		int bottomCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, 0, 0);

		// Top center to cap the top ring
		int topCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, islandCrustHeight, 0);

		// Triangles:
		int crustTriangleCount = totalIslandVertices * 6; // Two triangles per segment for the crust sides
		int baseTriangleCount = totalIslandVertices * 3; // One triangle per segment for the cone connecting the bottom ring to the cone tip
		int topCapTriangleCount = totalIslandVertices * 3; // One triangle per segment for the top cap connecting the top ring to the top center
		int[] triangles = new int[crustTriangleCount + baseTriangleCount + topCapTriangleCount]; // Total triangles
		// Keep track of where we are in the triangles array
		int triIndex = 0;

		// 1. Crust sides (two triangles per segment)
		for (int i = 0; i < totalIslandVertices; i++)
		{
			int next = (i + 1) % totalIslandVertices;
			// First triangle
			triangles[triIndex++] = i;
			triangles[triIndex++] = totalIslandVertices + next;
			triangles[triIndex++] = totalIslandVertices + i;

			// Second triangle
			triangles[triIndex++] = i;
			triangles[triIndex++] = next;
			triangles[triIndex++] = totalIslandVertices + next;
		}

		// 2. Cone connecting bottom ring to cone tip
		for (int i = 0; i < totalIslandVertices; i++)
		{
			int next = (i + 1) % totalIslandVertices;
			triangles[triIndex++] = totalIslandVertices + i;
			triangles[triIndex++] = totalIslandVertices + next;
			triangles[triIndex++] = coneTipIndex;
		}

		// 3. Top cap connecting top ring to top center
		for (int i = 0; i < totalIslandVertices; i++)
		{
			int next = (i + 1) % totalIslandVertices;
			triangles[triIndex++] = topCenterIndex;
			triangles[triIndex++] = next;
			triangles[triIndex++] = i;
		}

        /* UVs */
		Vector2[] uvs = new Vector2[vertices.Length];
		// Top ring UVs
		for (int i = 0; i < totalIslandVertices; i++){
			float angle = i * Mathf.PI * 2f / totalIslandVertices;
			uvs[i] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
		}
		// Bottom ring UVs
		for (int i = totalIslandVertices; i < totalIslandVertices * 2; i++){
			float angle = (i - totalIslandVertices) * Mathf.PI * 2f / totalIslandVertices;
			uvs[i] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
		}
		// Cone tip, bottom center, and top center UVs:
		uvs[coneTipIndex] = new Vector2(0.5f, 0.0f);
		uvs[bottomCenterIndex] = new Vector2(0.5f, 0.5f);
		uvs[topCenterIndex] = new Vector2(0.5f, 0.5f);


		/* Colors */
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < totalIslandVertices; i++)
        {
            colors[i] = crustColor; // Top ring (grass)
            colors[totalIslandVertices + i] = baseColor;
        }
		colors[vertices.Length - 1] = crustColor;
        colors[coneTipIndex] = bottomColor;
        
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

		// Set up the material
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material dynamicMaterial = new Material(Shader.Find("Custom/IslandShader"));
        meshRenderer.material = dynamicMaterial;
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
