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
				// Pick a random candidate position on the xz-plane of the island.
				Vector2 rand = Random.insideUnitCircle * islandRadius * populationData.MaxDistanceFromCenter;
				// Initial candidate position (using the island's base height as y) in island-local space.
				Vector3 candidatePos = new Vector3(rand.x, yHeight, rand.y);

				// Ensure candidatePos isn't too close to any previously placed objects.
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

				// Convert candidate position from island-local space to world space.
				Vector3 worldCandidatePos = populationData.ParentTransform.TransformPoint(candidatePos);

				// Raycast from above the candidate point to hit the deformed island surface.
				// Adjust rayStartOffset if your island's maximum height is higher.
				float rayStartOffset = 50f;
				Vector3 rayOrigin = new Vector3(worldCandidatePos.x, worldCandidatePos.y + rayStartOffset, worldCandidatePos.z);
				Ray ray = new Ray(rayOrigin, Vector3.down);
				RaycastHit hit;

				// Try the raycast against the island's mesh collider.
				if (meshCollider.Raycast(ray, out hit, rayStartOffset * 2))
				{
					// Check the slope using the angle between the hit.normal and Vector3.up.
					float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
					if (slopeAngle > populationData.MaxSlopeAngle)
					{
						// The slope is too steep; skip this candidate.
						continue;
					}
					// Use the hit point for the object's position.
					worldCandidatePos = hit.point;
				}
				// Optionally, if no hit is found you might want to skip this candidate:
				else
				{
					continue;
				}

				GameObject obj = Instantiate(prefab);
				obj.transform.position = worldCandidatePos;
				//obj.transform.localScale = Vector3.one * uniformScale;
				obj.transform.SetParent(populationData.ParentTransform);

				// Align the object's rotation with the surface normal, then add some random y rotation.
				if (hit.collider != null)
				{
					obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) *
											Quaternion.Euler(0, Random.Range(0, 360), 0);
				}
				else
				{
					obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
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
