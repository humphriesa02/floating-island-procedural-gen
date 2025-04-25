using UnityEngine;

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
	[SerializeField] private float innerRingScaleMin = 0.2f;
	[Tooltip("Maximum size of the inner ring close to the cone tip.")]
	[SerializeField] private float innerRingScaleMax = 0.6f;
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
	[SerializeField] private float perlinNoiseIntensityMin = 2.0f;
	[Tooltip("Maximum amount of randomness for the island vertices.")]
	[SerializeField] private float perlinNoiseIntensityMax = 2.0f;

    [Header("Island Population Settings")]
	[Tooltip("The possible objects to be spawned on the island.")]
    [SerializeField] private GameObject[] possibleObjects;
	[Tooltip("The minimum distance between objects on the island.")]
	[SerializeField] private float minDistanceBetweenObjects = 2.0f;
	[Tooltip("The maximum distance between objects on the island.")]
	[SerializeField] private float maxDistanceBetweenObjects = 5.0f;
	[Tooltip("The minimum number of objects to spawn on the island.")]
	[SerializeField] private int minObjectsToSpawn = 5;
	[Tooltip("The maximum number of objects to spawn on the island.")]
	[SerializeField] private int maxObjectsToSpawn = 10;
    [Tooltip("Number of attempts at placing an object")]
    [SerializeField] private int maxAttempts = 15;
    [Tooltip("The maximum distance from the center of the island to spawn objects.")]
    [SerializeField] private float maxDistanceFromCenter = 0.2f;
    [Tooltip("The maximum slope angle for the objects to be placed on the island.")]
    [Range(0, 90)] [SerializeField] private float maxSlopeAngle = 45f;

    [Header("Island LOD Settings")]
    [SerializeField] private Material lodIslandMaterial;
    [SerializeField] private float lod0Threshold = 1.0f;
    [SerializeField] private float lod1Threshold = 0.5f;

    [Header("Components")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    // Only use this for placing objects for now. Can be removed if too slow
    private MeshCollider meshCollider;

    [SerializeField] private LODGroup lodGroup;

    private MeshRenderer lodRenderer;

    // Island Scripts
    private readonly IslandMeshGenerator meshBuilder = new();
    private IslandPopulator islandPopulator;
    private readonly IslandStats islandStats = new();
    private readonly IslandMorpher islandMorpher = new();
    private readonly IslandLODGenerator lodGenerator = new();
    private IslandVisualizer islandVisualizer;
    private IslandGenerationData generationData;
    private IslandMeshResult meshResult;
    private IslandPopulationData populationData;

    private void Awake(){
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        islandPopulator = GetComponent<IslandPopulator>();
        islandVisualizer = GetComponent<IslandVisualizer>();
        if (lodGroup == null)
            lodGroup = GetComponent<LODGroup>();
    }

    void Start()
    {
        //CreateIsland("None");
    }

    public float GetRadius(){
        if (generationData.IslandCrustBottomRadius != 0 && generationData.IslandCrustTopRadius != 0)
            return Mathf.Max(generationData.IslandCrustBottomRadius, generationData.IslandCrustTopRadius);
        else
            return 0f;
    }

	public float GetHeight(){
        if (generationData.IslandBaseHeight != 0 && generationData.IslandCrustHeight != 0)
		    return generationData.IslandBaseHeight + generationData.IslandCrustHeight;
        else
            return 0f;
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


    public void CreateIsland(string affinity = "None"){
        Generate(); // Generate mesh data
        Build(); // Build the mesh and LODs
        Populate(affinity); // Populate the island with objects

        // Simulation to find conflict and affinity
        islandStats.ResolveConflicts();
        islandStats.CalculateAffinity();

        // Actually morph the island mesh based on the affinity
        Morph();

        // Apply the island stats to visually update the mesh and LODs
        islandVisualizer.ApplyVisuals(islandStats, meshResult.Mesh, meshRenderer);

        if (lodRenderer != null)
        {
            islandVisualizer.ApplyLODTint(islandStats, lodRenderer.material);
        }

        lodGroup.RecalculateBounds(); // Recalculate the bounds of the LODGroup
        // Set the object to be static for performance
        gameObject.isStatic = true;
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
        meshFilter.sharedMesh = meshResult.Mesh;

        SetUpLOD();
    }

    private void SetUpLOD(){
        GameObject lodMeshObj = lodGenerator.GenerateLODIsland(generationData, lodIslandMaterial);
        lodMeshObj.transform.SetParent(transform, false);
        lodRenderer = lodMeshObj.GetComponent<MeshRenderer>();

        var lod0 = new LOD(lod0Threshold, new Renderer[] { meshRenderer });
        var lod1 = new LOD(lod1Threshold, new Renderer[] { lodMeshObj.GetComponent<MeshRenderer>() });

        lodGroup.SetLODs(new LOD[] { lod0, lod1});   
        lodGroup.RecalculateBounds();
        
        lodMeshObj.isStatic = true;
    }
    
	public void Populate(string affinity = "None"){
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
            maxDistanceFromCenter,
            affinity
        );
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.enabled = true;
        islandPopulator.PopulateIsland(generationData, populationData, islandStats, meshCollider);
        meshCollider.enabled = false;
	}

    public void Morph()
    {
        if (meshResult.Mesh != null && islandStats != null)
        {
            islandMorpher.ApplyStatMorph(meshResult.Mesh, islandStats, meshResult.TopVertexIndices);
        }
    }

	public void ClearIsland(){
		if (meshFilter.sharedMesh != null) DestroyImmediate(meshFilter.sharedMesh);
		meshFilter.mesh = null;
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
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
}
