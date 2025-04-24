using UnityEngine;
using System.IO;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IslandManager : MonoBehaviour
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
    [Tooltip("Number of attempts at placing an object")]
    [SerializeField] private int maxAttempts = 15;
    [Tooltip("The maximum distance from the center of the island to spawn objects.")]
    [SerializeField] private float maxDistanceFromCenter = 0.2f; // Maximum distance from the center of the island to spawn objects
    [Tooltip("The maximum slope angle for the objects to be placed on the island.")]
    [Range(0, 90)] [SerializeField] private float maxSlopeAngle = 45f; // Maximum slope angle for the objects to be placed on the island

    private readonly IslandMeshGenerator meshBuilder = new();
    private readonly IslandPopulator islandPopulator = new();
    private readonly IslandStats islandStats = new();
    private readonly IslandMorpher islandMorpher = new();
    private IslandVisualizer islandVisualizer;
    private IslandGenerationData generationData;
    private IslandMeshResult meshResult;
    private IslandPopulationData populationData;

    // Components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // Only use this for placing objects for now. Can be removed if too slow
    private MeshCollider meshCollider;

    private void Awake(){
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        islandVisualizer = GetComponent<IslandVisualizer>();
    }

    void Start()
    {
        Generate();
        Build();
        Populate();
        islandStats.ResolveConflicts();
        islandStats.CalculateAffinity();
        Morph();
        islandVisualizer.ApplyVisuals(islandStats, meshResult.Mesh, meshRenderer);

        gameObject.isStatic = true;
    }

    public float GetRadius(){
        if (generationData == null) return 0f;
		return Mathf.Max(generationData.IslandCrustBottomRadius, generationData.IslandCrustTopRadius);
	}

	public float GetHeight(){
        if (generationData == null) return 0f;
		return generationData.IslandBaseHeight + generationData.IslandCrustHeight;
	}

    public string GetAffinity() => islandStats == null ? "None" : islandStats.Affinity;

    [ContextMenu("Generate Island Data")]
	void GenerateIslandDataContextMenu(){
		Generate();
	}

    [ContextMenu("Build Island Mesh")]
    void BuildIslandMeshContextMenu(){
        Build();
    }

	[ContextMenu("Populate Island")]
	void PopulateIslandContextMenu(){
		Populate();
	}

	[ContextMenu("Clear Island")]
	void ClearIslandContextMenu(){
		ClearIsland();
	}


    public void Generate(){
        float yPos = Random.Range(0.5f, 0.9f);
        float islandBaseHeight = Random.Range(islandBaseHeightMin, islandBaseHeightMax);
        generationData = new IslandGenerationData(
            Random.Range(islandCrustBottomRadiusMin, islandCrustBottomRadiusMax),
            islandBaseHeight,
            Random.Range(islandCrustTopRadiusMin, islandCrustTopRadiusMax),
            Random.Range(islandCrustHeightMin, islandCrustHeightMax),
            Random.Range(totalIslandVerticesMin, totalIslandVerticesMax),
            Random.Range(perlinNoiseIntensityMin, perlinNoiseIntensityMax),
            Mathf.Lerp(0, -islandBaseHeight, yPos),
            Random.Range(innerRingScaleMin, innerRingScaleMax),
            crustColor,
            baseColor,
            intermediateColor,
            bottomColor
        );
    }

    public void Build(){
        meshResult = meshBuilder.Build(generationData);
        meshFilter.mesh = meshResult.Mesh;
    }
    
	void OnDrawGizmosSelected() {
		var mesh = meshFilter.sharedMesh;
		if (!mesh) return;
		Gizmos.color = Color.yellow;
		foreach (var v in mesh.vertices) {
			Gizmos.DrawSphere(transform.TransformPoint(v), 0.1f);
		}
        islandMorpher.DrawMorphDebugGizmos(transform, meshFilter.sharedMesh);
	}

	public void Populate(){
        if (possibleObjects.Length == 0) return;
        int objectsToSpawn = Random.Range(minObjectsToSpawn, maxObjectsToSpawn);

		populationData = new IslandPopulationData(
            possibleObjects,
            minDistanceBetweenObjects,
            maxDistanceBetweenObjects,
            objectsToSpawn,
            this.transform,
            maxAttempts,
            maxSlopeAngle,
            maxDistanceFromCenter
        );
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.enabled = true;
        islandPopulator.PopulateIsland(generationData, populationData, islandStats, meshCollider);
        meshCollider.enabled = false;
	}

    public void Morph()
    {
        if (meshResult?.Mesh != null && islandStats != null)
        {
            islandMorpher.ApplyStatMorph(meshResult.Mesh, islandStats, meshResult.TopVertexIndices);
        }
    }

	public void ClearIsland(){
		var mesh = meshFilter.sharedMesh;
		if (mesh) DestroyImmediate(mesh);
		meshFilter.mesh = null;
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}

    public void SaveToJSON(string path = "island_save.json")
    {
        var saveData = new {
            generation = new {
                crustBottomRadius = generationData.IslandCrustBottomRadius,
                baseHeight = generationData.IslandBaseHeight,
                crustTopRadius = generationData.IslandCrustTopRadius,
                crustHeight = generationData.IslandCrustHeight,
                vertices = generationData.TotalIslandVertices,
                noise = generationData.PerlinNoiseIntensity,
                innerRingY = generationData.InnerRingY,
                innerRingScale = generationData.InnerRingScale,
                colors = new {
                    crust = generationData.CrustColor,
                    baseCol = generationData.BaseColor,
                    intermediate = generationData.IntermediateColor,
                    bottom = generationData.BottomColor
                }
            },
            population = new {
                objectsToSpawn = populationData.ObjectsToSpawn,
                minDistance = populationData.MinDistanceBetweenObjects,
                maxDistance = populationData.MaxDistanceBetweenObjects,
                objectNames = populationData.PossibleObjects.Select(o => o != null ? o.name : "null").ToList()
            }
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.dataPath, path), json);
        Debug.Log("Island data saved to: " + path);
    }
}
