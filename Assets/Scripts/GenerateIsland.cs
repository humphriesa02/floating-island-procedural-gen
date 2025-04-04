using UnityEngine;

public class GenerateIsland : MonoBehaviour
{
    [SerializeField] private GameObject[] possibleObjects; // Objects that can be spawned on an island
    [SerializeField] private GameObject[] spawnLocations; // Locations to spawn the objects on the island

    void Start()
    {
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
