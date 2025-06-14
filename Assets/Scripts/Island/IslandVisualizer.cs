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
            blendedColors[i] = Color.Lerp(originalColors[i], tint, 0.75f); // Blend 50% with stat tint
        }

        mesh.colors = blendedColors;
    }

    private Color GetTintColor(IslandStats stats)
    {
        float total = stats.Food + stats.Danger + stats.Defense + stats.People;
        if (total == 0) return Color.white;

        float food = stats.Food / total;
        float danger = stats.Danger / total;
        float defense = stats.Defense / total;
        float people = stats.People / total;

        Color foodTint = Color.green;
        Color dangerTint = Color.red;
        Color defenseTint = Color.blue;
        Color peopleTint = Color.yellow;

        // Weighted blend
        return foodTint * food + dangerTint * danger + defenseTint * defense + peopleTint * people;
    }

    public void ApplyLODTint(IslandStats stats, Material lodMaterial)
    {
        if (lodMaterial == null) return;
        Color tint = GetTintColor(stats);
        lodMaterial.color = tint;
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
