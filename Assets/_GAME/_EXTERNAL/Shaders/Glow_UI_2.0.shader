Shader "UI/Premium Glow"
{
    Properties
    {
        [Header(Shape)]
        _BoxSize   ("Inner Box Size", Range(0.05, 1.0)) = 0.75
        _Roundness ("Corner Roundness", Range(0.0, 1)) = 0.15

        [Header(Glow Layers)]
        [HDR] _GlowColor     ("Glow Color (HDR)", Color) = (0.25, 1, 1, 1)
        _HaloSpread          ("Halo Spread", Range(0.01, 2.0)) = 0.22
        _HaloIntensity       ("Halo Intensity", Range(0.0, 4.0)) = 0.6
        _HaloFalloff         ("Halo Falloff Curve", Range(0.5, 8.0)) = 2.2
        _CoreTightness       ("Core Tightness", Range(2.0, 60.0)) = 18.0
        _CoreIntensity       ("Core Intensity", Range(0.0, 8.0)) = 2.0
        _CoreWhiteness       ("Core Whiteness", Range(0.0, 1.0)) = 0.55
        _CoreOffset          ("Core Offset", Range(0.0, 0.15)) = 0.0

        [Header(Finishing)]
        _InnerGlow           ("Inner Rim Light", Range(0.0, 2.0)) = 0.15
        _PulseSpeed          ("Pulse Speed (Hz, 0 = off)", Float) = 0.0
        _PulseStrength       ("Pulse Strength", Range(0.0, 0.5)) = 0.08
        _Dither              ("Dither (Anti-Banding)", Range(0.0, 0.05)) = 0.005

        [Header(UI Masking)]
        _Stencil ("Stencil ID", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        // Premultiplied: свечение ведёт себя как ИСТОЧНИК СВЕТА —
        // складывается с фоном, не затемняет его и не тускнеет в зоне спада.
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "Default"

            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            ColorMask [_ColorMask]

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 rectSize : TEXCOORD1;   // (width, height) — запекает UIGlowRect

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 rectSize      : TEXCOORD2;
                float4 color         : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _GlowColor;

            float _BoxSize;
            float _Roundness;
            float _HaloSpread;
            float _HaloIntensity;
            float _HaloFalloff;
            float _CoreTightness;
            float _CoreIntensity;
            float _CoreWhiteness;
            float _CoreOffset;
            float _InnerGlow;
            float _PulseSpeed;
            float _PulseStrength;
            float _Dither;

            float4 _ClipRect;


            v2f vert(appdata_t v)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex        = UnityObjectToClipPos(v.vertex);
                OUT.texcoord      = v.texcoord;
                OUT.worldPosition = v.vertex;
                OUT.rectSize      = v.rectSize;
                OUT.color         = v.color;

                return OUT;
            }


            float sdRoundBox(float2 p, float2 halfSize, float radius)
            {
                float2 q = abs(p) - halfSize + radius;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radius;
            }


            half4 frag(v2f IN) : SV_Target
            {
                // 0..1 -> -1..1
                float2 p = IN.texcoord * 2.0 - 1.0;

                // ----------------------------------------------------
                // Коррекция пропорций: SDF в "физическом" пространстве,
                // чтобы толщина свечения и углы были одинаковыми по всем осям.
                // ----------------------------------------------------
                float2 scale = 1.0;
                if (IN.rectSize.x > 0.0 && IN.rectSize.y > 0.0)
                {
                    scale = IN.rectSize.xy / max(IN.rectSize.x, IN.rectSize.y);
                }

                float2 halfSize = _BoxSize * scale;
                float radius = min(_Roundness, _BoxSize) * min(scale.x, scale.y);

                float dist = sdRoundBox(p * scale, halfSize, radius);

                // Скорость изменения расстояния в пикселях -> идеальный AA на любом DPI.
                float aa = max(fwidth(dist), 1e-5);

                // ----------------------------------------------------
                // Dither (Interleaved Gradient Noise) против бандинга.
                // Шумим ВХОДНОЕ расстояние, а не результат —
                // там, где свечения нет, шум тоже невозможен.
                // ----------------------------------------------------
                float noise = frac(52.9829189 * frac(dot(IN.vertex.xy, float2(0.06711056, 0.00583715))));
                float d = max(dist + (noise - 0.5) * _Dither, 0.0);

                // ----------------------------------------------------
                // Пульс ("дыхание"), опционально
                // ----------------------------------------------------
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed * 6.2831853) * _PulseStrength;

                // ----------------------------------------------------
                // Слой 1: Halo — широкое мягкое гало
                // ----------------------------------------------------
                float t = saturate(1.0 - d / _HaloSpread);
                float halo = pow(t, _HaloFalloff) * _HaloIntensity;

                // ----------------------------------------------------
                // Слой 2: Core — раскалённая кромка у границы
                // ----------------------------------------------------
                float core = exp(-max(d - _CoreOffset, 0.0) * _CoreTightness) * _CoreIntensity;

                // ----------------------------------------------------
                // Слой 3: Inner — лёгкая подсветка рамки изнутри
                // ----------------------------------------------------
                float inner = exp(min(dist, 0.0) * _CoreTightness) * _InnerGlow;

                // Маски с честным AA
                float outerMask = smoothstep(-aa, aa, dist);
                float innerMask = smoothstep(-aa, aa, -dist);

                // Цвет: к кромке сдвиг к белому — эффект "раскалённого ядра"
                float3 glowColor  = _GlowColor.rgb;
                float3 coreColor  = lerp(glowColor, float3(1, 1, 1), _CoreWhiteness);
                float3 innerColor = lerp(glowColor, float3(1, 1, 1), _CoreWhiteness * 0.5);

                float3 col = glowColor * (halo * pulse * outerMask)
                           + coreColor * (core * pulse * outerMask)
                           + innerColor * (inner * innerMask);

                float alpha = saturate((halo + core) * pulse * outerMask + inner * innerMask);

                // ----------------------------------------------------
                // Тинт и fade из UI (Image.color, CanvasGroup)
                // ----------------------------------------------------
                col   *= IN.color.rgb * IN.color.a;
                alpha *= IN.color.a;

                // ----------------------------------------------------
                // RectMask2D
                // ----------------------------------------------------
                #if UNITY_UI_CLIP_RECT
                    float clipA = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    col   *= clipA;
                    alpha *= clipA;
                #endif

                #if UNITY_UI_ALPHACLIP
                    clip(alpha - 0.001);
                #endif

                return half4(col, alpha);
            }

            ENDCG
        }
    }
}