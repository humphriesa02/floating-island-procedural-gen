using UnityEngine;

public struct IslandPopulationData
{
    public GameObject[] PossibleObjects;
    public float MinDistanceBetweenObjects;
    public float MaxDistanceBetweenObjects;
    public int ObjectsToSpawn;
    public Transform ParentTransform;
    public float MaxSlopeAngle;
    public int MaxAttempts;
    public float MaxDistanceFromCenter;
    public string IntendedAffinity;

    public IslandPopulationData(GameObject[] possibleObjects, float minDist, float maxDist, int count, Transform parent, int maxAttempts, float maxSlopeAngle = 45f, float maxDistanceFromCenter = 0.2f, string affinity = "None")
    {
        PossibleObjects = possibleObjects;
        MinDistanceBetweenObjects = minDist;
        MaxDistanceBetweenObjects = maxDist;
        ObjectsToSpawn = count;
        ParentTransform = parent;
        MaxAttempts = maxAttempts;
        MaxSlopeAngle = maxSlopeAngle;
        MaxDistanceFromCenter = maxDistanceFromCenter;
        IntendedAffinity = affinity;
    }
}
