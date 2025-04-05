Shader "Custom/IslandShader" {
    Properties {
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        half _Glossiness;
        half _Metallic;

        struct Input {
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Use the vertex color as the base color.
            o.Albedo = IN.color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}