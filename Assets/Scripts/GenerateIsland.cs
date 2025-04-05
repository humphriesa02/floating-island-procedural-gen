using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateIsland : MonoBehaviour
{
	[Header("Island Generation Settings")]
	[Tooltip("Radius of the bottom most part of the crust.")]
	[SerializeField] private float islandCrustBottomRadius = 10f;
	[Tooltip("Radius of the top most part of the crust.")]
	[SerializeField] private float islandCrustTopRadius = 10f;
	[Tooltip("Height of the island's crust (platform).")]
	[SerializeField] private float islandCrustHeight = 2f;
	[Tooltip("Height of the island's base (underneath the platform).")]
	[SerializeField] private float islandBaseHeight = 2f;
	[Tooltip("Number of vertices in the island mesh.")]
	[SerializeField] private int islandVertices = 100; 
	[Tooltip("Color of the island's crust.")]
	[SerializeField] private Color crustColor = Color.green;
	[Tooltip("Color of the island's base.")]
	[SerializeField] private Color baseColor = new Color(150f / 255f, 75f / 255f, 0f / 255f);
	[Tooltip("Color for the bottom most part of the base.")]
	[SerializeField] private Color bottomColor = new Color(192f / 255f, 64f / 255f, 0f / 255f);


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
        // Vertices we need to create the island shape
        Vector3[] vertices = new Vector3[islandVertices * 2 + 3];

		int vertIndex = 0;
       	// Top ring (upper cylinder)
        for (int i = 0; i < islandVertices; i++)
        {
            float angle = i * Mathf.PI * 2f / islandVertices;
            float x = Mathf.Cos(angle) * islandCrustTopRadius;
            float z = Mathf.Sin(angle) * islandCrustTopRadius;
            vertices[vertIndex++] = new Vector3(x, islandCrustHeight, z);
        }

		// Bottom ring (lower cylinder)
        for (int i = 0; i < islandVertices; i++)
        {
            float angle = i * Mathf.PI * 2f / islandVertices;
            float x = Mathf.Cos(angle) * islandCrustBottomRadius;
            float z = Mathf.Sin(angle) * islandCrustBottomRadius;
            vertices[vertIndex++] = new Vector3(x, 0, z); // Bottom ring at y = 0
        }

		// Island base tip
		int coneTipIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, -islandBaseHeight, 0);

		// Bottom center
		int bottomCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, 0, 0);

		// Top center to cap the top ring
		int topCenterIndex = vertIndex;
		vertices[vertIndex++] = new Vector3(0, islandCrustHeight, 0);

		// Triangles:
		int crustTriangleCount = islandVertices * 6;
		int baseTriangleCount = islandVertices * 3;
		int topCapTriangleCount = islandVertices * 3;
		int[] triangles = new int[crustTriangleCount + baseTriangleCount + topCapTriangleCount];
		int triIndex = 0;

		// 1. Crust sides (two triangles per segment)
		for (int i = 0; i < islandVertices; i++)
		{
			int next = (i + 1) % islandVertices;
			// First triangle
			triangles[triIndex++] = i;
			triangles[triIndex++] = islandVertices + next;
			triangles[triIndex++] = islandVertices + i;

			// Second triangle
			triangles[triIndex++] = i;
			triangles[triIndex++] = next;
			triangles[triIndex++] = islandVertices + next;
		}

		// 2. Cone connecting bottom ring to cone tip
		for (int i = 0; i < islandVertices; i++)
		{
			int next = (i + 1) % islandVertices;
			triangles[triIndex++] = islandVertices + i;
			triangles[triIndex++] = islandVertices + next;
			triangles[triIndex++] = coneTipIndex;
		}

		// 3. Top cap connecting top ring to top center
		for (int i = 0; i < islandVertices; i++)
		{
			int next = (i + 1) % islandVertices;
			triangles[triIndex++] = topCenterIndex;
			triangles[triIndex++] = next;
			triangles[triIndex++] = i;
		}

        /* UVs */
		Vector2[] uvs = new Vector2[vertices.Length];
		// Top ring UVs
		for (int i = 0; i < islandVertices; i++){
			float angle = i * Mathf.PI * 2f / islandVertices;
			uvs[i] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
		}
		// Bottom ring UVs
		for (int i = islandVertices; i < islandVertices * 2; i++){
			float angle = (i - islandVertices) * Mathf.PI * 2f / islandVertices;
			uvs[i] = new Vector2((Mathf.Cos(angle) + 1) * 0.5f, (Mathf.Sin(angle) + 1) * 0.5f);
		}
		// Cone tip, bottom center, and top center UVs:
		uvs[coneTipIndex] = new Vector2(0.5f, 0.0f);
		uvs[bottomCenterIndex] = new Vector2(0.5f, 0.5f);
		uvs[topCenterIndex] = new Vector2(0.5f, 0.5f);


		/* Colors */
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < islandVertices; i++)
        {
            colors[i] = crustColor; // Top ring (grass)
            colors[islandVertices + i] = baseColor;
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
