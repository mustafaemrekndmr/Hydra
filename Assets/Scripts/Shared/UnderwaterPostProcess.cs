using UnityEngine;

/// <summary>
/// Post-processing effect for underwater vignette and color grading
/// Attach to camera for underwater visual enhancement
/// </summary>
[RequireComponent(typeof(Camera))]
public class UnderwaterPostProcess : MonoBehaviour
{
    [Header("Vignette Settings")]
    [Range(0f, 1f)]
    public float vignetteIntensity = 0.4f; // Moderate vignette
    [Range(0f, 1f)]
    public float vignetteSmoothness = 0.5f; // Smooth transition
    public Color vignetteColor = new Color(0.0f, 0.08f, 0.15f, 1f); // Subtle dark blue
    
    [Header("Color Grading")]
    public Color underwaterTint = new Color(0.6f, 0.8f, 0.95f, 1f); // Subtle blue tint
    [Range(0f, 1f)]
    public float tintStrength = 0.3f; // Moderate tint
    
    [Header("Depth Fade")]
    public bool enableDepthFade = true;
    [Range(0f, 100f)]
    public float depthFadeDistance = 30f;
    public Color deepWaterColor = new Color(0.02f, 0.15f, 0.3f, 1f);
    
    private Material postProcessMaterial;
    
    void Start()
    {
        // Create shader for post-processing
        Shader shader = Shader.Find("Hidden/UnderwaterPostProcess");
        if (shader == null)
        {
            // Create a simple shader if not found
            CreateShader();
        }
        else
        {
            postProcessMaterial = new Material(shader);
        }
    }
    
    void CreateShader()
    {
        // Create a simple post-process shader programmatically
        string shaderCode = @"
Shader ""Hidden/UnderwaterPostProcess""
{
    Properties
    {
        _MainTex (""Texture"", 2D) = ""white"" {}
        _VignetteIntensity (""Vignette Intensity"", Range(0,1)) = 0.4
        _VignetteSmoothness (""Vignette Smoothness"", Range(0,1)) = 0.5
        _VignetteColor (""Vignette Color"", Color) = (0,0.1,0.2,1)
        _UnderwaterTint (""Underwater Tint"", Color) = (0.6,0.85,1,1)
        _TintStrength (""Tint Strength"", Range(0,1)) = 0.3
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include ""UnityCG.cginc""

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
}";
        
        // Save shader to file
        System.IO.File.WriteAllText(Application.dataPath + "/UnderwaterPostProcess.shader", shaderCode);
        Debug.Log("Created UnderwaterPostProcess shader at Assets/UnderwaterPostProcess.shader");
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (postProcessMaterial != null)
        {
            postProcessMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
            postProcessMaterial.SetFloat("_VignetteSmoothness", vignetteSmoothness);
            postProcessMaterial.SetColor("_VignetteColor", vignetteColor);
            postProcessMaterial.SetColor("_UnderwaterTint", underwaterTint);
            postProcessMaterial.SetFloat("_TintStrength", tintStrength);
            
            Graphics.Blit(source, destination, postProcessMaterial);
        }
        else
        {
            // Fallback: just copy
            Graphics.Blit(source, destination);
        }
    }
}
