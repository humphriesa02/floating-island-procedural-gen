using UnityEngine;

public struct IslandGenerationData
{
    public float IslandCrustBottomRadius;
	public float IslandBaseHeight;
	public float IslandCrustTopRadius;
	public float IslandCrustHeight;
	public int TotalIslandVertices;
	public float PerlinNoiseIntensity;
	public float InnerRingY;
	public float InnerRingScale;

    public Color CrustColor;
	public Color BaseColor;
	public Color IntermediateColor;
	public Color BottomColor;

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
