Shader "Custom/IslandShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Glossiness;
        half _Metallic;

        struct Input {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Sample albedo texture
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            
            // Multiply texture by vertex color if desired
            o.Albedo = tex.rgb * IN.color.rgb;

            // Sample and apply normal map
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }

    FallBack "Diffuse"
}