using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

public class IslandLayoutB : SkyIslandLayout
{
    [SerializeField] private GameObject islandStructure; // Prefabs to use for generating islands in this layout
    [SerializeField] private int lowerBound = 2;
    [SerializeField] private int upperBound = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateIslands();
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

    public string ParseString(string str)
    {
        string rec = "None";
        float fPercent = 25;
        float pPercent = 50;
        float dPercent = 75;
        float danPercent = 100;

        switch(str)
        {
            case "Food":
                fPercent = 20;
                pPercent = 60;
                dPercent = 80;
                break;
            case "People":
                fPercent = 20;
                pPercent = 40;
                dPercent = 60;

                break;
            case "Defense":
                fPercent = 40;
                pPercent = 60;
                dPercent = 80;
                break;
            case "Danger":
                fPercent = 20;
                pPercent = 40;
                dPercent = 80;
                break;
        }

        float randPercent = Random.Range(0, 100);
        if (randPercent <= fPercent)
        {
            rec = "People";
        }
        else if (randPercent <= pPercent)
        {
            rec = "Danger";
        }
        else if (randPercent <= dPercent)
        {
            rec = "Food";
        }
        else if (randPercent <= danPercent)
        {
            rec = "Defense";
        }

            return rec;
    }
}