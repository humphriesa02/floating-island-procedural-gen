using UnityEngine;

public class GenerateIsland : MonoBehaviour
{
    [SerializeField] private GameObject[] possibleObjects;
    [SerializeField] private GameObject[] spawnLocations;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		for (int i = 0; i < spawnLocations.Length; i++)
		{
			if (possibleObjects.Length > 0)
			{
				// Choose a random object from possibleObjects
				GameObject randomObject = possibleObjects[Random.Range(0, possibleObjects.Length)];

				// Instantiate the object as a child of the spawn location
				GameObject spawnedObject = Instantiate(randomObject, spawnLocations[i].transform.position, Quaternion.identity);

				// Set the parent to maintain hierarchy
				spawnedObject.transform.SetParent(spawnLocations[i].transform);
			}
		}
    }
}
