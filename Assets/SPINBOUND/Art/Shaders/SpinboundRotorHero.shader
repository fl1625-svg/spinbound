Shader "SPINBOUND/Rotor Hero"
{
    Properties
    {
        _CeramicColor("Ceramic Color", Color) = (0.91,0.95,0.99,1)
        _MetalColor("Brushed Metal Color", Color) = (0.20,0.27,0.35,1)
        _MechanismColor("Mechanism Color", Color) = (0.028,0.042,0.068,1)
        _EnergyColor("Energy Glass Color", Color) = (0.07,0.67,1.0,1)
        _EmissionStrength("Emission Strength", Range(0,3)) = 0.8
        _SpeedState("Speed State", Range(0,2)) = 0
        _Role("Material Role", Range(0,3)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.55
        _Metallic("Metallic", Range(0,1)) = 0.0
        _RimColor("Rim Color", Color) = (0.72,0.94,1,1)
        _RimPower("Rim Power", Range(1,8)) = 3.4
        _RimStrength("Rim Strength", Range(0,1)) = 0.18
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
            float4 _CeramicColor;
            float4 _MetalColor;
            float4 _MechanismColor;
            float4 _EnergyColor;
            float4 _RimColor;
            float _EmissionStrength;
            float _SpeedState;
            float _Role;
            float _Smoothness;
            float _Metallic;
            float _RimPower;
            float _RimStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));

                float ceramicMask = 1.0 - step(0.5, _Role);
                float metalMask = step(0.5, _Role) * (1.0 - step(1.5, _Role));
                float mechanismMask = step(1.5, _Role) * (1.0 - step(2.5, _Role));
                float energyMask = step(2.5, _Role);

                float speed2 = step(0.5, _SpeedState);
                float speed3 = step(1.5, _SpeedState);
                float speed01 = saturate(_SpeedState * 0.5);

                float3 speed2Energy = float3(0.05, 0.80, 1.00);
                float3 speed3Energy = float3(0.42, 0.94, 1.00);
                float3 dynamicEnergy = lerp(_EnergyColor.rgb, speed2Energy, speed2);
                dynamicEnergy = lerp(dynamicEnergy, speed3Energy, speed3);

                float3 baseColor = _CeramicColor.rgb * ceramicMask
                    + _MetalColor.rgb * metalMask
                    + _MechanismColor.rgb * mechanismMask
                    + dynamicEnergy * energyMask;

                float brushed = 0.965 + 0.035 * sin(input.positionWS.x * 52.0 + input.positionWS.z * 19.0);
                baseColor *= lerp(1.0, brushed, metalMask);

                float ndl = saturate(dot(normalWS, mainLight.direction));
                float wrap = saturate(ndl * 0.76 + 0.24);
                float3 ambient = max(SampleSH(normalWS), float3(0.035, 0.045, 0.06));
                float shadow = lerp(0.62, 1.0, mainLight.shadowAttenuation);
                float3 direct = mainLight.color * mainLight.distanceAttenuation * shadow * (0.34 + wrap * 0.78);

                float roleSmoothness = saturate(_Smoothness + metalMask * 0.18 + energyMask * 0.10);
                float roleMetallic = saturate(_Metallic + metalMask * 0.06 + mechanismMask * 0.20);
                float3 halfVector = normalize(mainLight.direction + viewDir);
                float specular = pow(saturate(dot(normalWS, halfVector)), lerp(26.0, 150.0, roleSmoothness));
                float rim = pow(1.0 - saturate(dot(normalWS, viewDir)), _RimPower) * _RimStrength;

                float energyPulse = energyMask * _EmissionStrength * (0.52 + speed01 * 0.72 + speed3 * 0.40);
                float ceramicEdge = ceramicMask * (speed2 * 0.045 + speed3 * 0.085);
                float metalCharge = metalMask * (speed2 * 0.025 + speed3 * 0.055);

                float3 color = baseColor * (ambient * 0.72 + direct);
                color += specular * mainLight.color * lerp(0.08, 0.88, roleMetallic);
                color += _RimColor.rgb * rim * (0.70 + speed01 * 0.30);
                color += dynamicEnergy * (energyPulse + ceramicEdge + metalCharge);
                return half4(color, 1);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
