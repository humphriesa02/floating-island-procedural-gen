using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(GenerateIsland))]
public class IslandStats : MonoBehaviour
{
    private float fertility;
    private float biodiversity;
    private float ruggedness;

    [SerializeField] private float fertilityWeight = 5.0f;
    [SerializeField] private float biodiversityWeight = 0.3f;
    [SerializeField] private float ruggednessWeight = 20.0f;

    public void EvaluateIsland(){
        GenerateIsland island = GetComponent<GenerateIsland>();
        if (island == null)
        {
            Debug.LogError("GenerateIsland component not found on this GameObject.");
            return;
        }

        // Mesh-based heuristics
        float radius = island.IslandCrustTopRadius;
        float height = island.GetHeight();
        float noise = island.PerlinNoiseIntensity;

        ruggedness = Mathf.Clamp01(noise / ruggednessWeight);
        fertility = Mathf.Clamp01(radius / fertilityWeight + 1f - ruggedness);
        biodiversity = 0;
        
        
        Debug.Log($"Island Stats: Fertility: {fertility}, Biodiversity: {biodiversity}, Ruggedness: {ruggedness}");
    }
}
