using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

public class IslandLayoutB : SkyIslandLayout
{
    [SerializeField] private GameObject islandStructure; // Prefabs to use for generating islands in this layout
    [SerializeField] private int lowerBound = 2;
    [SerializeField] private int upperBound = 5;
        
    private string mostRecentAffinity = "None";

    private float numFood = 0;
    private float numPeople = 0;
    private float numDefense = 0;
    private float numDanger = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GenerateIslands();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void GenerateIslands()
    {
        int islandCount = Random.Range(lowerBound, upperBound);
        List<GameObject> islands = new List<GameObject>();

        SkyIslandLayout prevLayout = GetPreviousLayout();
        SkyIslandLayout nextLayout = GetNextLayout(); 
        if(prevLayout != null)
        {
            GameObject[] prevIslands = prevLayout.getSkyIslands();
        }
        if(nextLayout != null)
        {
            GameObject[] nextIslands = nextLayout.getSkyIslands();
        }

        for (int i = 0; i < islandCount; i++)
        {
            // Create a new empty GameObject at random positions within the layout bounds for spawn locations
            GameObject newSpawnLocation = new GameObject($"SpawnLocation_{i}");
            bool valid = false;
            int iterationLimit = 100; // Prevent infinite loop in case of an error
            Vector3 pos = Vector3.zero;

            while (!valid && iterationLimit > 0)
            {
                float xPos = Random.Range(-getHalfWidth(), getHalfWidth());
                float yPos = Random.Range(-getHalfHeight(), getHalfHeight());
                float zPos = Random.Range(-getHalfLength(), getHalfLength());
                pos = new Vector3(xPos + transform.position.x, yPos + transform.position.y, zPos + transform.position.z);

                GameObject tempIsland = Instantiate(islandStructure);
                string newAffinity = DetermineNextAffinity(mostRecentAffinity);
                tempIsland.GetComponent<IslandManager>().CreateIsland(newAffinity);
                mostRecentAffinity = tempIsland.GetComponent<IslandManager>().GetAffinity();
                UpdateAffinityCount(mostRecentAffinity, 1);
                float currRad = tempIsland.GetComponent<IslandManager>().GetRadius();
                float currHeight = tempIsland.GetComponent<IslandManager>().GetHeight();
                Debug.Log($"Prefab Island Initialized: Radius={currRad}, Height={currHeight}");

                valid = true;

                foreach (GameObject prevIsland in islands)
                {
                    float prevRad = prevIsland.GetComponent<IslandManager>().GetRadius();
                    float prevHeight = prevIsland.GetComponent<IslandManager>().GetHeight();
                    Debug.Log($"Previous Island: Radius={prevRad}, Height={prevHeight}");

                    float closestDistance = CalculateClosestDistance(pos, currRad, currHeight, prevIsland.transform.position, prevRad, prevHeight);
                    Debug.Log($"Closest Distance: {closestDistance}");

                    if (closestDistance <= 10.0f)
                    {
                        Debug.Log($"Island {i} is too close to another island. Distance: {closestDistance}");
                        valid = false;
                        break;
                    }
                }
                if (valid && prevLayout != null)
                {
                    foreach (GameObject prevIsland in prevLayout.SkyIslands)
                    {
                        float prevRad = prevIsland.GetComponent<IslandManager>().GetRadius();
                        float prevHeight = prevIsland.GetComponent<IslandManager>().GetHeight();

                        float closestDistance = CalculateClosestDistance(pos, currRad, currHeight, prevIsland.transform.position, prevRad, prevHeight);
                        if (closestDistance <= 10.0f)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid && nextLayout != null)
                {
                    foreach (GameObject nextIsland in nextLayout.SkyIslands)
                    {
                        float nextRad = nextIsland.GetComponent<IslandManager>().GetRadius();
                        float nextHeight = nextIsland.GetComponent<IslandManager>().GetHeight();

                        float closestDistance = CalculateClosestDistance(pos, currRad, currHeight, nextIsland.transform.position, nextRad, nextHeight);
                        if (closestDistance <= 10.0f)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                iterationLimit--;
                if (iterationLimit <= 0)
                {
                    Debug.LogWarning($"Failed to find a valid position for island {i}.");
                    Destroy(newSpawnLocation);
                    continue;
                }
                if (valid)
                {
                    tempIsland.transform.parent = newSpawnLocation.transform;
                    tempIsland.transform.Rotate(0, Random.Range(0, 360), 0);
                    islands.Add(tempIsland);
                    newSpawnLocation.transform.position = pos;
                }
                else
                {
                    Destroy(tempIsland);
                    Destroy(newSpawnLocation);
                }
            }
        }
        skyIslands = islands.ToArray();
    }

    private void UpdateAffinityCount(string affinity, int count)
    {
        switch (affinity)
        {
            case "Food":
                numFood += count;
                break;
            case "People":
                numPeople += count;
                break;
            case "Defense":
                numDefense += count;
                break;
            case "Danger":
                numDanger += count;
                break;
            default:
                Debug.LogWarning($"Unknown affinity: {affinity}");
                break;
        }
    }

    private float CalculateClosestDistance(Vector3 pos1, float radius1, float height1, Vector3 pos2, float radius2, float height2)
    {
        // Calculate horizontal distance in the XZ plane
        Vector3 horizontalVector = new Vector3(pos2.x - pos1.x, 0, pos2.z - pos1.z);
        float horizontalDistance = horizontalVector.magnitude - (radius1 + radius2);
        horizontalDistance = Mathf.Max(0, horizontalDistance); // If overlapping, set to 0

        // Calculate vertical distance in the Y axis
        float verticalDistance = Mathf.Abs(pos2.y - pos1.y) - (height1 / 2 + height2 / 2);
        verticalDistance = Mathf.Max(0, verticalDistance); // If overlapping, set to 0

        // Combine horizontal and vertical distances using Pythagorean theorem
        return Mathf.Sqrt(horizontalDistance * horizontalDistance + verticalDistance * verticalDistance);
    }

    public string DetermineNextAffinity(string str)
    {
        
        // Weight accumulation
        float totalWeight = 0.0f;
        Dictionary<string, float> weights = new();

        for (int i = 0; i < 4; i++)
        {
            if (i == 0){
                // Food
                float weight = 1f / (1f + numFood);
                weights["Food"] = weight;
                totalWeight += weight;
            }
            else if (i == 1){
                // People
                float weight = 1f / (1f + numPeople);
                weights["People"] = weight;
                totalWeight += weight;
            }
            else if (i == 2){
                // Defense
                float weight = 1f / (1f + numDefense);
                weights["Defense"] = weight;
                totalWeight += weight;
            }
            else if (i == 3){
                // Danger
                float weight = 1f / (1f + numDanger);
                weights["Danger"] = weight;
                totalWeight += weight;
            }
        }

        switch (mostRecentAffinity){
            case "Food":
                weights["Defense"] *= 1.5f;
                totalWeight += weights["Defense"];
                break;
            case "People":
                weights["Danger"] *= 1.5f;
                totalWeight += weights["Danger"];
                break;
            case "Defense":
                weights["Food"] *= 1.5f;
                totalWeight += weights["Food"];
                break;
            case "Danger":
                weights["People"] *= 1.5f;
                totalWeight += weights["People"];
                break;
        }

        // Weighted random select
        
        float randomVal = Random.Range(0, totalWeight);
        float cumulativeWeight = 0.0f;
        foreach (var kvp in weights)
        {
            cumulativeWeight += kvp.Value;
            if (randomVal < cumulativeWeight)
            {
                // Return the selected affinity
                return kvp.Key;
            }
        }

        return "Food";
    }
}