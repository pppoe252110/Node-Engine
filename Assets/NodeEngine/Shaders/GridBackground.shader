Shader "Custom/GridBackground"
{
    Properties
    {
        _Color1("Color1", Color) = (0, 0, 0, 1)
        _Color2("Color2", Color) = (1, 1, 1, 1)
        _GridScale("GridScale", Range(0, 1)) = 0.07
        _GridSize("GridSize", Vector,2) = (5, 5, 0, 0)
        _GridOffset("GridOffset", Vector,2) = (0, 0, 0, 0)
        _GridScaleOffset("GridScaleOffset", Float) = 1
        _OutlineSize("OutlineSize", Vector,2) = (1, 1, 0, 0)
        [HideInInspector]_MainTex("MainTex", 2D) = "white" {}
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("ColorMask", Float) = 15
        [HideInInspector]_ClipRect("ClipRect", Vector) = (0, 0, 0, 0)
        [HideInInspector]_UIMaskSoftnessX("UIMaskSoftnessX", Float) = 1
        [HideInInspector]_UIMaskSoftnessY("UIMaskSoftnessY", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Pass
        {
            Name "Default"
            Cull Off
            Blend One OneMinusSrcAlpha
            ZTest [unity_GUIZTestMode]
            ZWrite Off
            ColorMask [_ColorMask]
            Stencil
            {
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Ref [_Stencil]
                CompFront [_StencilComp]
                PassFront [_StencilOp]
                CompBack [_StencilComp]
                PassBack [_StencilOp]
            }
            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color1;
            float4 _Color2;
            float _GridScale;
            float2 _GridSize;
            float2 _GridOffset;
            float _GridScaleOffset;
            float2 _OutlineSize;
            float _Stencil;
            float _StencilOp;
            float _StencilWriteMask;
            float _StencilReadMask;
            float _ColorMask;
            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float4 color : COLOR;
                float4 uv0 : TEXCOORD1;
                float4 uv1 : TEXCOORD2;
            };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.color = input.color;
                output.uv0 = input.uv0;
                output.uv1 = input.uv1;
                return output;
            }
            void CalcLinearSomething_float(float targetValue, float initialValue, float adjustmentFactor, float maxIterations, out float Scale)
            {
                float currentValue = initialValue;
                float closestValue = initialValue;
                float smallestDistance = abs(initialValue - targetValue);
                int iteration = 0;
                float closenessThreshold = 0.01f;
                while (iteration < maxIterations && smallestDistance > closenessThreshold)
                {
                    bool shouldMultiply = currentValue < targetValue;
                    currentValue = shouldMultiply ? currentValue * adjustmentFactor : currentValue / adjustmentFactor;
                    float currentDistance = abs(currentValue - targetValue);
                    if (currentDistance < smallestDistance)
                    {
                        smallestDistance = currentDistance;
                        closestValue = currentValue;
                    }
                    iteration++;
                }
                Scale = closestValue;
            }
            float4 frag(Varyings input) : SV_Target
            {
                float4 color1 = _Color1;
                float4 color2 = _Color2;
                float2 gridOffset = _GridOffset;
                float2 pos = input.positionWS.xy + gridOffset;
                float gridScaleOffset = _GridScaleOffset;
                float2 gridSize = _GridSize;
                float2 divided = gridSize / float2(1080, 1080);
                float2 multiplied = gridScaleOffset * divided;
                float invScale = 1.0 / gridScaleOffset;
                float scale;
                CalcLinearSomething_float(invScale, gridSize.x, 2.0, 10.0, scale);
                float2 tiling = multiplied * scale;
                float2 tiled = pos * tiling;
                float2 fractional = frac(tiled);
                float gridScale = _GridScale;
                float oneMinus = 1 - gridScale;
                float2 d = abs(fractional * 2 - 1) - float2(oneMinus, oneMinus);
                d = saturate(1 - d / fwidth(d));
                float rect = min(d.x, d.y);
                float4 lerped = lerp(color1, color2, rect);
                float4 finalColor = lerped;
                finalColor.a = 1;
                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, input.positionWS.xy) * step(input.positionWS.xy, _ClipRect.zw);
                finalColor.a *= inside.x * inside.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}