Shader "SPINBOUND/Stylized PBR"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5,0.8,0.4,1)
        _ShadowColor("Shadow Tint", Color) = (0.18,0.28,0.22,1)
        _RimColor("Rim Color", Color) = (0.75,0.95,1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0.32
        _Metallic("Metallic", Range(0,1)) = 0.0
        _RimPower("Rim Power", Range(1,8)) = 3.0
        _RimStrength("Rim Strength", Range(0,2)) = 0.25
        _MatcapStrength("Matcap Strength", Range(0,1)) = 0.22
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _ShadowColor;
            float4 _RimColor;
            float _Smoothness;
            float _Metallic;
            float _RimPower;
            float _RimStrength;
            float _MatcapStrength;
            CBUFFER_END

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float3 viewDirWS:TEXCOORD2; };

            Varyings vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs n = GetVertexNormalInputs(input.normalOS);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = NormalizeNormalPerVertex(n.normalWS);
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(p.positionWS);
                return o;
            }

            half4 frag(Varyings i):SV_Target
            {
                float3 n = normalize(i.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float ndl = saturate(dot(n, mainLight.direction));
                float wrap = saturate(ndl * 0.72 + 0.28);
                float3 ramp = lerp(_ShadowColor.rgb, _BaseColor.rgb, smoothstep(0.15, 0.92, wrap));
                float3 h = normalize(mainLight.direction + i.viewDirWS);
                float spec = pow(saturate(dot(n,h)), lerp(24.0, 128.0, _Smoothness));
                float rim = pow(1.0 - saturate(dot(n, i.viewDirWS)), _RimPower) * _RimStrength;
                float matcap = pow(saturate(n.y * 0.5 + 0.5), 2.0) * _MatcapStrength;
                float3 color = ramp * mainLight.color * mainLight.shadowAttenuation;
                color += spec * lerp(0.08, 0.9, _Metallic);
                color += _RimColor.rgb * rim + _BaseColor.rgb * matcap;
                return half4(color,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
