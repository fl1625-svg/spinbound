Shader "SPINBOUND/Stylized Foliage"
{
    Properties
    {
        _BaseColor("Base Color", Color)=(0.28,0.72,0.24,1)
        _TipColor("Tip Color", Color)=(0.72,0.92,0.38,1)
        _WindStrength("Wind Strength", Range(0,0.35))=0.09
        _WindScale("Wind Scale", Range(0.1,4))=1.25
        _Translucency("Translucency", Range(0,1))=0.35
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Cull Off
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _TipColor;
            float _WindStrength;
            float _WindScale;
            float _Translucency;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; };
            Varyings vert(Attributes v)
            {
                float3 pos = v.positionOS.xyz;
                float phase = _Time.y * 1.35 + pos.x * _WindScale + pos.z * 0.73;
                pos.xz += float2(sin(phase), cos(phase * 0.83)) * _WindStrength * saturate(v.uv.y);
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(pos);
                VertexNormalInputs n=GetVertexNormalInputs(v.normalOS);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS; o.normalWS=n.normalWS; o.uv=v.uv;
                return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                float3 n=normalize(i.normalWS);
                Light l=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float ndl=saturate(dot(n,l.direction));
                float back=saturate(dot(-n,l.direction))*_Translucency;
                float3 base=lerp(_BaseColor.rgb,_TipColor.rgb,saturate(i.uv.y));
                float3 c=base*(0.45+0.65*ndl*l.shadowAttenuation)+base*back*0.38;
                return half4(c,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
