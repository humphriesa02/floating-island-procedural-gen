using UnityEngine;

public class IslandPopulationData
{
    public GameObject[] PossibleObjects;
    public float MinDistanceBetweenObjects;
    public float MaxDistanceBetweenObjects;
    public int ObjectsToSpawn;
    public Transform ParentTransform;

    public IslandPopulationData(GameObject[] possibleObjects, float minDist, float maxDist, int count, Transform parent)
    {
        PossibleObjects = possibleObjects;
        MinDistanceBetweenObjects = minDist;
        MaxDistanceBetweenObjects = maxDist;
        ObjectsToSpawn = count;
        ParentTransform = parent;
    }
}
