using UnityEngine;

public class IslandVisualizer : MonoBehaviour
{
    [SerializeField] Material foodMat, dangerMat, defenseMat, peopleMat;

    public void ApplyVisuals(IslandStats stats, Mesh mesh, MeshRenderer renderer)
    {
        if (renderer != null)
        {
            renderer.sharedMaterial = GetMaterialByAffinity(stats.Affinity);
        }

        ApplyVertexColorTint(stats, mesh);
    }
    public void ApplyVertexColorTint(IslandStats stats, Mesh mesh)
    {
        if (mesh == null || mesh.vertexCount == 0) return;

        Color[] originalColors = mesh.colors;
        if (originalColors == null || originalColors.Length != mesh.vertexCount)
        {
            originalColors = new Color[mesh.vertexCount];
            for (int i = 0; i < originalColors.Length; i++) originalColors[i] = Color.white;
        }

        Color tint = GetTintColor(stats);
        Color[] blendedColors = new Color[mesh.vertexCount];

        for (int i = 0; i < blendedColors.Length; i++)
        {
            blendedColors[i] = Color.Lerp(originalColors[i], tint, 0.5f); // Blend 50% with stat tint
        }

        mesh.colors = blendedColors;
    }

    private Color GetTintColor(IslandStats stats)
    {
        float intensity;
        switch (stats.Affinity)
        {
            case "Food":
                intensity = Mathf.Clamp01(stats.Food / 10f);
                return Color.Lerp(Color.gray, Color.green, intensity);
            case "Danger":
                intensity = Mathf.Clamp01(stats.Danger / 10f);
                return Color.Lerp(Color.black, Color.red, intensity);
            case "Defense":
                intensity = Mathf.Clamp01(stats.Defense / 10f);
                return Color.Lerp(Color.gray, Color.blue, intensity);
            case "People":
                intensity = Mathf.Clamp01(stats.People / 10f);
                return Color.Lerp(Color.white, Color.yellow, intensity);
            default:
                return Color.white;
        }
    }

    private Material GetMaterialByAffinity(string affinity)
    {
        return affinity switch
        {
            "Food" => foodMat,
            "Danger" => dangerMat,
            "Defense" => defenseMat,
            "People" => peopleMat,
            _ => foodMat // fallback
        };
    }
}
