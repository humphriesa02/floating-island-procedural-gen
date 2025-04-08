using NUnit.Framework;
using UnityEngine;

public class IslandLayoutB : SkyIslandLayout
{
    [SerializeField] private GameObject[] islandStructures; // Prefabs to use for generating islands in this layout
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
        GameObject[] spawnLocations = new GameObject[Random.Range(lowerBound, upperBound)];

        for(int i = 0; i < spawnLocations.Length; i++)
        {
            // Create a new empty GameObject at random positions within the layout bounds for spawn locations
            GameObject newSpawnLocation = new GameObject($"SpawnLocation_{i}");
            bool valid = false;
            int iterationLimit = 100; // Prevent infinite loop in case of an error
            while (!valid)
            {
                float xPos = Random.Range(-getHalfWidth(), getHalfWidth());
                float yPos = Random.Range(-getHalfHeight(), getHalfHeight());
                float zPos = Random.Range(-getHalfLength(), getHalfLength());
                Vector3 pos = transform.position;
                newSpawnLocation.transform.position = new Vector3(xPos + pos.x, yPos + pos.y, zPos + pos.z);
                if (i > 0)
                {
                    for(int j = 0; j < i; j++)
                    {
                        float distance = Vector3.Distance(newSpawnLocation.transform.position, spawnLocations[j].transform.position);
                        if(distance < 5.0f) // Adjust this value to control the minimum distance between islands
                        {
                            valid = false;
                            break;
                        }
                        else
                        {
                            valid = true;
                        }
                        
                    }
                }
                iterationLimit--;
                if (iterationLimit <= 0)
                {
                    valid = true;
                    break;
                }
            }
            
            spawnLocations[i] = newSpawnLocation;

            GameObject island = islandStructures[Random.Range(0, islandStructures.Length)];
            GameObject tempIsland = Instantiate(island, spawnLocations[i].transform.position, Quaternion.identity, spawnLocations[i].transform);
            tempIsland.transform.Rotate(0, Random.Range(0, 360), 0);
        }

        
    }

}
