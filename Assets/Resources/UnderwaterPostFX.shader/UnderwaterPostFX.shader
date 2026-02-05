Shader "Hidden/UnderwaterPostFX"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (0, 0.4, 0.7, 1)
        _TintParams ("Tint Strength", Float) = 0.2
        _VignetteIntensity ("Vignette Intensity", Float) = 0.5
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            fixed4 _TintColor;
            float _TintParams;
            float _VignetteIntensity;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Blue Tint
                col = lerp(col, col * _TintColor, _TintParams);
                
                // Vignette with a bit of "depth blur" fake by darkening edges more
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                // Simple vignette
                float vignette = 1.0 - smoothstep(0.4, 1.0, dist * _VignetteIntensity * 2.0);
                
                return col * vignette;
            }
            ENDCG
        }
    }
}
