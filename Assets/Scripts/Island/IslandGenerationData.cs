using UnityEngine;

public class IslandGenerationData
{
    public float IslandCrustBottomRadius { get; set; }
	public float IslandBaseHeight { get; set; }
	public float IslandCrustTopRadius { get; set; }
	public float IslandCrustHeight { get; set; }
	public int TotalIslandVertices { get; set; }
	public float PerlinNoiseIntensity { get; set; }
	public float InnerRingY { get; set; }
	public float InnerRingScale { get; set; }

    public Color CrustColor { get; set; }
	public Color BaseColor  { get; set; }
	public Color IntermediateColor { get; set; }
	public Color BottomColor { get; set; }

    public IslandGenerationData(
        float islandCrustBottomRadius, float islandBaseHeight, float islandCrustTopRadius,
        float islandCrustHeight, int totalIslandVertices, float perlinNoiseIntensity, 
        float innerRingY, float innerRingScale,
        Color crustColor, Color baseColor, Color intermediateColor, Color bottomColor
        )
    {
        IslandCrustBottomRadius = islandCrustBottomRadius;
        IslandBaseHeight = islandBaseHeight;
        IslandCrustTopRadius = islandCrustTopRadius;
        IslandCrustHeight = islandCrustHeight;
        TotalIslandVertices = totalIslandVertices;
        PerlinNoiseIntensity = perlinNoiseIntensity;
        InnerRingY = innerRingY;
        InnerRingScale = innerRingScale;
        CrustColor = crustColor;
        BaseColor = baseColor;
        IntermediateColor = intermediateColor;
        BottomColor = bottomColor;
    }
}
