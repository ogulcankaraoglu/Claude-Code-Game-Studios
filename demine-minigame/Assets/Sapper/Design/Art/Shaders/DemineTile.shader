Shader "Sapper/DemineTile"
{
    Properties
    {
        // Main color tint — driven per-tile state via Material.SetColor
        _Color ("Color", Color) = (1,1,1,1)

        // Sprite / icon texture
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        // Rounded-rect border
        _BorderColor ("Border Color", Color) = (0.15, 0.15, 0.2, 1)
        _BorderSize  ("Border Size (px)", Float) = 3.0

        // Physical size of the tile in pixels — set this to match tile RectTransform size
        _RectSize ("Rect Size (px)", Vector) = (64, 64, 0, 0)

        // Corner radius in pixels
        _CornerRadius ("Corner Radius (px)", Float) = 8.0

        // Highlight stripe — simulates a subtle bevel/light from top-left
        _HighlightColor  ("Highlight Color", Color) = (1,1,1,0.12)
        _HighlightAngle  ("Highlight Angle (deg)", Float) = 135.0
        _HighlightWidth  ("Highlight Width (0-1)", Range(0,0.5)) = 0.18

        // Stencil — required by Unity UI
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4    _Color;
            fixed4    _BorderColor;
            float     _BorderSize;
            float4    _RectSize;
            float     _CornerRadius;
            fixed4    _HighlightColor;
            float     _HighlightAngle;
            float     _HighlightWidth;
            float4    _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex   = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color    = v.color * _Color;
                return OUT;
            }

            // Signed-distance function for a rounded rectangle.
            // p  : position relative to rect center in pixels
            // b  : half-extents in pixels
            // r  : corner radius in pixels
            float RoundedRectSDF(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // --- Clip (UI masking) ---
                half4 color = IN.color * tex2D(_MainTex, IN.texcoord);
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                clip(color.a - 0.001);

                // --- UV -> pixel space (origin at rect center) ---
                float2 rectPx   = _RectSize.xy;
                float2 halfRect = rectPx * 0.5;
                float2 p        = (IN.texcoord - 0.5) * rectPx; // px, center-origin

                // --- Outer rounded rect ---
                float outerDist = RoundedRectSDF(p, halfRect, _CornerRadius);

                // Discard pixels outside the rounded rect
                clip(-outerDist);

                // --- Inner rounded rect (inset by border) ---
                float innerR    = max(_CornerRadius - _BorderSize, 0.0);
                float innerDist = RoundedRectSDF(p, halfRect - _BorderSize, innerR);

                // border mask — 1 in border zone, 0 inside
                float borderMask = saturate(-innerDist * 0.0); // will be replaced
                // smooth border using SDF
                float borderAlpha = saturate(-innerDist / max(fwidth(innerDist), 0.001));
                borderMask = 1.0 - borderAlpha; // 1 = border, 0 = interior

                // --- Highlight stripe ---
                float  rad        = _HighlightAngle * (3.14159265 / 180.0);
                float2 dir        = float2(cos(rad), sin(rad));
                float  proj       = dot(IN.texcoord - 0.5, dir); // -0.5..0.5
                float  halfW      = _HighlightWidth * 0.5;
                float  highlightM = smoothstep(halfW, halfW - 0.04, abs(proj - halfW));

                // --- Compose ---
                fixed4 fillColor    = color;
                fixed4 borderOut    = lerp(fillColor, _BorderColor,   borderMask);
                fixed4 highlighted  = lerp(borderOut, borderOut + _HighlightColor, highlightM * (1.0 - borderMask));

                // anti-alias outer edge
                float edgeAlpha = saturate(-outerDist / max(fwidth(outerDist), 0.001));
                highlighted.a  *= edgeAlpha;

                return highlighted;
            }
            ENDCG
        }
    }
}
