using System.Collections.Generic;
using UnityEngine;

public class IslandPopulator : MonoBehaviour
{
	[SerializeField] private float raycastHeight = 50f;
    public void PopulateIsland(IslandGenerationData generationData, IslandPopulationData populationData, IslandStats islandStats, MeshCollider meshCollider)
	{

		float islandRadius = generationData.IslandCrustTopRadius;
		float yHeight = generationData.IslandCrustHeight;
		List<Vector3> usedPositions = new List<Vector3>();

		for (int i = 0; i < populationData.ObjectsToSpawn; i++)
		{
			GameObject prefab = SelectPrefabByAffinity(populationData);
			float randScale = Random.Range(0.5f, 2.0f);
			float distanceBetweenObjects = randScale * Random.Range(populationData.MinDistanceBetweenObjects, populationData.MaxDistanceBetweenObjects);

			for (int attempt = 0; attempt < populationData.MaxAttempts; attempt++)
			{
				Vector2 rand = Random.insideUnitCircle * islandRadius * populationData.MaxDistanceFromCenter;
				Vector3 candidatePos = new Vector3(rand.x, yHeight, rand.y);

				if (IsTooClose(candidatePos, usedPositions, distanceBetweenObjects)) continue;

				GameObject instance = Instantiate(prefab);
                instance.transform.localScale = prefab.transform.localScale * randScale;
                instance.transform.SetParent(populationData.ParentTransform);

				if (TrySnapToSurface(candidatePos, meshCollider, out Vector3 snappedPos, out Quaternion rotation, populationData))
                {
                    usedPositions.Add(candidatePos);
                    instance.transform.position = snappedPos;
                    instance.transform.rotation = rotation;

                    Structure structure = instance.GetComponent<Structure>();
                    if (structure != null)
                    {
                        float contribution = Mathf.Lerp(0.75f, 1.25f, Mathf.InverseLerp(0.5f, 2.0f, randScale));
                        islandStats.AddFromStructure(structure, contribution);
                    }

                    break;
                }

                Destroy(instance);
			}
		}
	}

	private bool IsTooClose(Vector3 candidate, List<Vector3> placed, float minDist)
    {
        foreach (Vector3 pos in placed)
            if (Vector3.Distance(candidate, pos) < minDist)
                return true;

        return false;
    }

	private bool TrySnapToSurface(Vector3 localPos, MeshCollider collider, out Vector3 worldPos, out Quaternion rotation, IslandPopulationData data)
    {
        worldPos = Vector3.zero;
        rotation = Quaternion.identity;

        Vector3 worldRayOrigin = data.ParentTransform.TransformPoint(localPos + Vector3.up * raycastHeight);
        Ray ray = new Ray(worldRayOrigin, Vector3.down);

        if (collider.Raycast(ray, out RaycastHit hit, raycastHeight * 2))
        {
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            if (slope <= data.MaxSlopeAngle)
            {
                worldPos = hit.point;
                rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0f); // Flat rotation only
                return true;
            }
        }

        return false;
    }

	private GameObject SelectPrefabByAffinity(IslandPopulationData populationData)
	{
		var pool = new List<(GameObject prefab, float weight)>();

		foreach (var prefab in populationData.PossibleObjects)
		{
			var structure = prefab.GetComponent<Structure>();
			if (structure == null) continue;

			float weight = 1f;

			switch (populationData.IntendedAffinity)
			{
				case "Food": weight += structure.Food * 0.5f; break;
				case "People": weight += structure.People * 0.5f; break;
				case "Defense": weight += structure.Defense * 0.5f; break;
				case "Danger": weight += structure.Danger * 0.5f; break;
			}

			pool.Add((prefab, weight));
		}

		float totalWeight = 0f;
		foreach (var item in pool) totalWeight += item.weight;
		float r = Random.value * totalWeight;

		float cumulative = 0f;
		foreach (var item in pool)
		{
			cumulative += item.weight;
			if (r <= cumulative)
				return item.prefab;
		}

		return populationData.PossibleObjects[Random.Range(0, populationData.PossibleObjects.Length)];
	}
}
