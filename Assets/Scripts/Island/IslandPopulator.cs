using System.Collections.Generic;
using UnityEngine;

public class IslandPopulator : MonoBehaviour
{
    public void PopulateIsland(IslandGenerationData generationData, IslandPopulationData populationData, IslandStats islandStats)
    {
        if (populationData.PossibleObjects.Length == 0) return;

		float islandRadius = generationData.IslandCrustTopRadius;
		float yHeight = generationData.IslandCrustHeight;
		int objectsToSpawn = populationData.ObjectsToSpawn;
		List<Vector3> usedPositions = new List<Vector3>();
		for (int i = 0; i < objectsToSpawn; i++)
		{
			GameObject prefab = populationData.PossibleObjects[Random.Range(0, populationData.PossibleObjects.Length)];
			float uniformScale = Random.Range(0.5f, 2.0f);
			float distanceBetweenObjects = uniformScale * Random.Range(populationData.MinDistanceBetweenObjects, populationData.MaxDistanceBetweenObjects);
			for (int attempt = 0; attempt < 10; attempt++){
				Vector2 rand = Random.insideUnitCircle * islandRadius * 0.6f;
				Vector3 candidatePos = new Vector3(rand.x, yHeight, rand.y);

				bool tooClose = false;
				for (int j = 0; j < usedPositions.Count; j++)
				{
					if (Vector3.Distance(candidatePos, usedPositions[j]) < distanceBetweenObjects)
					{
						tooClose = true;
						break;
					}
				}
				if (tooClose) continue;
				usedPositions.Add(candidatePos);


				GameObject obj = Instantiate(prefab);
				obj.transform.position = populationData.ParentTransform.TransformPoint(candidatePos);
				obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
				obj.transform.SetParent(populationData.ParentTransform);

				Structure structure = obj.GetComponent<Structure>();
                if (structure != null)
                {
                    float modifier = Mathf.Lerp(0.75f, 1.25f, Mathf.InverseLerp(0.5f, 2.0f, uniformScale));
					islandStats.AddFromStructure(structure, modifier);
                }

				break;
			}
		}
    }
}
