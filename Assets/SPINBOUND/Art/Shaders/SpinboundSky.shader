Shader "SPINBOUND/Highland Sky"
{
    Properties
    {
        _HorizonColor("Horizon", Color)=(0.72,0.88,1,1)
        _ZenithColor("Zenith", Color)=(0.17,0.48,0.86,1)
        _SunColor("Sun Glow", Color)=(1,0.88,0.62,1)
        _SunDirection("Sun Direction", Vector)=(0.35,0.55,0.75,0)
        _SunPower("Sun Power", Range(8,256))=64
        _SunStrength("Sun Strength", Range(0,2))=.55
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 _HorizonColor,_ZenithColor,_SunColor,_SunDirection;float _SunPower,_SunStrength;
            struct A{float4 positionOS:POSITION;};struct V{float4 positionCS:SV_POSITION;float3 dir:TEXCOORD0;};
            V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.dir=normalize(TransformObjectToWorldDir(a.positionOS.xyz));return o;}
            half4 frag(V i):SV_Target{float h=saturate(i.dir.y*.72+.28);float3 c=lerp(_HorizonColor.rgb,_ZenithColor.rgb,smoothstep(0,1,h));float sun=pow(saturate(dot(normalize(i.dir),normalize(_SunDirection.xyz))),_SunPower)*_SunStrength;c+=_SunColor.rgb*sun;return half4(c,1);}
            ENDHLSL
        }
    }
}
