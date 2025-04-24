using System.Collections.Generic;
using UnityEngine;

public class IslandPopulator : MonoBehaviour
{
    public void PopulateIsland(IslandGenerationData generationData, IslandPopulationData populationData, IslandStats islandStats, MeshCollider meshCollider)
	{
		if (populationData.PossibleObjects.Length == 0)
		return;

		float islandRadius = generationData.IslandCrustTopRadius;
		float yHeight = generationData.IslandCrustHeight;
		int objectsToSpawn = populationData.ObjectsToSpawn;
		List<Vector3> usedPositions = new List<Vector3>();

		for (int i = 0; i < objectsToSpawn; i++)
		{
			GameObject prefab = populationData.PossibleObjects[Random.Range(0, populationData.PossibleObjects.Length)];
			float uniformScale = Random.Range(0.5f, 2.0f);
			float distanceBetweenObjects = uniformScale * Random.Range(populationData.MinDistanceBetweenObjects, populationData.MaxDistanceBetweenObjects);

			for (int attempt = 0; attempt < populationData.MaxAttempts; attempt++)
			{
				Vector2 rand = Random.insideUnitCircle * islandRadius * populationData.MaxDistanceFromCenter;
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
				if (tooClose)
					continue;
				usedPositions.Add(candidatePos);

				Vector3 worldCandidatePos = populationData.ParentTransform.TransformPoint(candidatePos);
				GameObject obj = Instantiate(prefab);
				obj.transform.localScale = prefab.transform.localScale * uniformScale;
				obj.transform.SetParent(populationData.ParentTransform);

				float rayStartOffset = 50f;
				Vector3 rayOrigin = new Vector3(worldCandidatePos.x, worldCandidatePos.y + rayStartOffset, worldCandidatePos.z);
				Ray ray = new Ray(rayOrigin, Vector3.down);
				RaycastHit hit;

				if (meshCollider.Raycast(ray, out hit, rayStartOffset * 2))
				{
					float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
					if (slopeAngle > populationData.MaxSlopeAngle)
					{
						Destroy(obj);
						continue;
					}
					obj.transform.position = hit.point;
					obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) *
											Quaternion.Euler(0, Random.Range(0, 360), 0);
				}
				else
				{
					Destroy(obj);
					continue;
				}

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
