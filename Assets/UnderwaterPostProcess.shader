
Shader "Hidden/UnderwaterPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VignetteIntensity ("Vignette Intensity", Range(0,1)) = 0.4
        _VignetteSmoothness ("Vignette Smoothness", Range(0,1)) = 0.5
        _VignetteColor ("Vignette Color", Color) = (0,0.1,0.2,1)
        _UnderwaterTint ("Underwater Tint", Color) = (0.6,0.85,1,1)
        _TintStrength ("Tint Strength", Range(0,1)) = 0.3
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _VignetteIntensity;
            float _VignetteSmoothness;
            float4 _VignetteColor;
            float4 _UnderwaterTint;
            float _TintStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply underwater tint
                col.rgb = lerp(col.rgb, col.rgb * _UnderwaterTint.rgb, _TintStrength);
                
                // Calculate vignette
                float2 center = i.uv - 0.5;
                float vignette = length(center);
                vignette = smoothstep(0.5 - _VignetteSmoothness, 0.5, vignette);
                vignette = pow(vignette, 2);
                
                // Apply vignette
                col.rgb = lerp(col.rgb, _VignetteColor.rgb, vignette * _VignetteIntensity);
                
                return col;
            }
            ENDCG
        }
    }
}