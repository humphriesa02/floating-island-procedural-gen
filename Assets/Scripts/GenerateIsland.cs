using UnityEditor.EditorTools;
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
	[Tooltip("Minimum size of the inner ring close to the cone tip.")]
	[SerializeField] private float innerRingScaleMin = 0.2f; // Change this value to adjust the inner ring size.
	[Tooltip("Maximum size of the inner ring close to the cone tip.")]
	[SerializeField] private float innerRingScaleMax = 0.6f; // Change this value to adjust the inner ring size.
	[Tooltip("Minimum number of vertices in the island mesh.")]
	[SerializeField] private int totalIslandVerticesMin = 25;
	[Tooltip("Maximum number of vertices in the island mesh.")]
	[SerializeField] private int totalIslandVerticesMax = 100; 
	[Tooltip("Color of the island's crust.")]
	[SerializeField] private Color crustColor = Color.green;
	[Tooltip("Color of the island's base.")]
	[SerializeField] private Color baseColor = new Color(150f / 255f, 75f / 255f, 0f / 255f);
	[Tooltip("Color of the island's intermediate.")]
	[SerializeField] private Color intermediateColor = new Color(255f / 255f, 224f / 255f, 189f / 255f);
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
		// Determine the Y position of the inner ring by lerping between the bottom ring (y=0) and cone tip (y = -islandBaseHeight).
		float yPos = Random.Range(0.5f, 0.9f); // Random value between 0 and 1
		float innerRingY = Mathf.Lerp(0, -islandBaseHeight, yPos); // Midway; adjust as needed.
		float innerRingScale = Random.Range(innerRingScaleMin, innerRingScaleMax);

		Debug.Log($"Island Parameters: Bottom Radius: {islandCrustBottomRadius}, Base Height: {islandBaseHeight}, Top Radius: {islandCrustTopRadius}, Crust Height: {islandCrustHeight}, Vertices: {totalIslandVertices}, Perlin Noise Intensity: {perlinNoiseIntensity}");
		/* Vertices */

        // Vertices we need to create the island shape
        Vector3[] vertices = new Vector3[4 * totalIslandVertices + 3]; // 3 rings + cone tip + bottom center + top center

		// Keep track of where we are in the vertices array
		int vertIndex = 0;

       	// Top ring (upper cylinder)
        for (int i = 0; i < totalIslandVertices; i++)
        {
            float angle = i * Mathf.PI * 2f / totalIslandVertices;
            float x = Mathf.Cos(angle) * islandCrustTopRadius;
            float z = Mathf.Sin(angle) * islandCrustTopRadius;
			float sample = Mathf.PerlinNoise(x / islandCrustTopRadius, z / islandCrustTopRadius) * perlinNoiseIntensity; // Perlin noise for randomness
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

		// 3. Inner cone ring (smaller than bottom ring) positioned at innerRingY,
		// with a radius that is a fraction of the bottom ring's radius.
		float innerRingRadius = islandCrustBottomRadius * innerRingScale;
		for (int i = 0; i < totalIslandVertices; i++){
			float angle = i * Mathf.PI * 2f / totalIslandVertices;
			float x = Mathf.Cos(angle) * innerRingRadius;
			float z = Mathf.Sin(angle) * innerRingRadius;
			// Apply Perlin noise similarly if desired.
			float sample = Mathf.PerlinNoise(x / innerRingRadius, z / innerRingRadius) * perlinNoiseIntensity * 0.1f;
			vertices[vertIndex++] = new Vector3(x + sample, innerRingY + sample, z + sample);
		}

		// Island base cone tip vertex
		int coneTipIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, -islandBaseHeight, 0);
		
		// Top face of the cylinder vertices
		// Bottom center
		int bottomCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, 0, 0);

		int topCapRingStart = vertIndex;
		for (int i = 0; i < totalIslandVertices; i++){
			float angle = i * Mathf.PI * 2f / totalIslandVertices;
			float x = Mathf.Cos(angle) * islandCrustTopRadius;
			float z = Mathf.Sin(angle) * islandCrustTopRadius;
			// Optionally add similar noise if you like
			float sample = Mathf.PerlinNoise(x / islandCrustTopRadius, z / islandCrustTopRadius) * perlinNoiseIntensity;
			vertices[vertIndex++] = new Vector3(x + sample, islandCrustHeight + sample, z + sample);
		}

		int topCenterIndex = vertIndex;
    	vertices[vertIndex++] = new Vector3(0, islandCrustHeight, 0);

		// Triangles:
		int crustTriangleCount = totalIslandVertices * 6;
		int coneBottomInnerTriangleCount = totalIslandVertices * 6;
		int coneInnerTipTriangleCount = totalIslandVertices * 3;
		int topCapTriangleCount = totalIslandVertices * 3;
		int totalTrianglesCount = crustTriangleCount + coneBottomInnerTriangleCount + coneInnerTipTriangleCount + topCapTriangleCount;
		int[] triangles = new int[totalTrianglesCount];
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

		// 2. Cone lower section: Connect bottom ring to inner cone ring.
		for (int i = 0; i < totalIslandVertices; i++){
			int next = (i + 1) % totalIslandVertices;
			int bottomCurrent = totalIslandVertices + i;
			int bottomNext = totalIslandVertices + next;
			int innerCurrent = totalIslandVertices * 2 + i;
			int innerNext = totalIslandVertices * 2 + next;
			// Two triangles for the quad between bottom and inner rings:
			triangles[triIndex++] = bottomCurrent;
			triangles[triIndex++] = bottomNext;
			triangles[triIndex++] = innerCurrent;

			triangles[triIndex++] = innerCurrent;
			triangles[triIndex++] = bottomNext;
			triangles[triIndex++] = innerNext;
    	}

		// 3. Connect inner cone ring to the cone tip
		for (int i = 0; i < totalIslandVertices; i++)
		{
			int next = (i + 1) % totalIslandVertices;
			int innerCurrent = totalIslandVertices * 2 + i;
			int innerNext = totalIslandVertices * 2 + next;
			// One triangle per segment that brings the inner ring to the tip:
			triangles[triIndex++] = innerCurrent;
			triangles[triIndex++] = innerNext;
			triangles[triIndex++] = coneTipIndex;
		}

		// 4. Top cap connecting top ring to top center
		for (int i = 0; i < totalIslandVertices; i++)
		{
			int next = (i + 1) % totalIslandVertices;
			triangles[triIndex++] = topCenterIndex;
			triangles[triIndex++] = next;
			triangles[triIndex++] = i;
		}

        /* UVs */
		Vector2[] uvs = new Vector2[vertices.Length];
    
		// For the side rings (cylindrical mapping):
		// Top ring (side): indices 0 .. totalIslandVertices-1, V = 1.
		for (int i = 0; i < totalIslandVertices; i++){
			float angle = i * Mathf.PI * 2f / totalIslandVertices;
			float u = angle / (Mathf.PI * 2f);
			uvs[i] = new Vector2(u, 1.0f);
		}
		// Bottom ring: indices totalIslandVertices .. 2*totalIslandVertices-1, V = 0.
		for (int i = totalIslandVertices; i < 2 * totalIslandVertices; i++){
			int j = i - totalIslandVertices;
			float angle = j * Mathf.PI * 2f / totalIslandVertices;
			float u = angle / (Mathf.PI * 2f);
			uvs[i] = new Vector2(u, 0.0f);
		}
		// Inner ring: indices 2*totalIslandVertices .. 3*totalIslandVertices-1.
		// For simplicity we set V = -0.5 (adjust as needed).
		for (int i = 2 * totalIslandVertices; i < 3 * totalIslandVertices; i++){
			int j = i - 2 * totalIslandVertices;
			float angle = j * Mathf.PI * 2f / totalIslandVertices;
			float u = angle / (Mathf.PI * 2f);
			uvs[i] = new Vector2(u, -0.5f);
		}
		
		// For the top cap ring: use radial UV mapping (polar coordinates).
		// Map the center (0,0) to (0.5,0.5) and use the cosine/sine to get the UVs.
		for (int i = topCapRingStart; i < topCapRingStart + totalIslandVertices; i++){
			int j = i - topCapRingStart;
			float angle = j * Mathf.PI * 2f / totalIslandVertices;
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
		for (int i = 0; i < totalIslandVertices; i++){
			// Use a gradient for the top ring.
			float gradient = Mathf.PerlinNoise(vertices[i].x * 0.1f, vertices[i].z * 0.1f);
			colors[i] = Color.Lerp(crustColor, intermediateColor, gradient); // Top ring color
		}
		for (int i = totalIslandVertices; i < totalIslandVertices * 2; i++){
			float gradient = Mathf.PerlinNoise(vertices[i].x * 0.1f, vertices[i].z * 0.1f);
			colors[i] = Color.Lerp(baseColor, bottomColor, gradient); // Bottom ring color
		}
		for (int i = totalIslandVertices * 2; i < totalIslandVertices * 3; i++){
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

		// Set up the material
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Material dynamicMaterial = new Material(Shader.Find("Custom/IslandShader"));
        Texture2D diffuseTexture = Resources.Load<Texture2D>("IslandDiffuse");
		Texture2D normalMapTexture = Resources.Load<Texture2D>("IslandNormalMap");
		if(diffuseTexture != null)
			Debug.Log($"Diffuse texture loaded: {diffuseTexture.name}");
			dynamicMaterial.SetTexture("_MainTex", diffuseTexture);
		if(normalMapTexture != null)
		{
			Debug.Log($"Normal map texture loaded: {normalMapTexture.name}");
			dynamicMaterial.SetTexture("_BumpMap", normalMapTexture);
			dynamicMaterial.EnableKeyword("_NORMALMAP");
		}
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
