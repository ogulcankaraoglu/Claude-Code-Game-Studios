Shader "Sapper/DemineBoard"
{
    Properties
    {
        // Background fill color
        _Color ("Background Color", Color) = (0.08, 0.10, 0.14, 1)

        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        // Pixel-accurate dot grid
        // Set _BoardSize to the board RectTransform width (== height if square) in pixels
        _BoardSize   ("Board Size (px)", Float) = 500.0
        _DotColor    ("Dot Color", Color)  = (0.2, 0.22, 0.28, 1)
        _DotSpacing  ("Dot Spacing (px)", Float)  = 20.0
        _DotRadius   ("Dot Radius (px)",  Float)  = 1.0

        // Vignette — subtle darkening toward edges
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.35
        _VignetteRadius   ("Vignette Radius",   Range(0, 2)) = 1.0

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
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
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
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4    _Color;
            float     _BoardSize;
            fixed4    _DotColor;
            float     _DotSpacing;
            float     _DotRadius;
            float     _VignetteStrength;
            float     _VignetteRadius;
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

            fixed4 frag(v2f IN) : SV_Target
            {
                // --- Clip (UI masking) ---
                half4 texSample = tex2D(_MainTex, IN.texcoord);
                float clipAlpha = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                clip(clipAlpha - 0.001);

                // --- Background fill ---
                fixed4 col = IN.color;

                // --- Pixel-accurate dot grid ---
                // Convert UV to pixel position in board space
                float2 px = IN.texcoord * _BoardSize;

                // Nearest grid point (modulo wraps into a cell)
                float2 cellPx   = fmod(px, _DotSpacing);              // 0..spacing
                float2 toCenter = cellPx - 0.5 * _DotSpacing;         // centered on grid node
                float  dist     = length(toCenter);

                // Anti-aliased dot
                float dotMask = 1.0 - smoothstep(_DotRadius - 0.8, _DotRadius + 0.8, dist);

                col = lerp(col, _DotColor, dotMask * _DotColor.a);

                // --- Radial vignette ---
                float2 centered  = IN.texcoord - 0.5;                  // -0.5..0.5
                float  vigDist   = length(centered) / (_VignetteRadius * 0.5 + 0.001);
                float  vigMask   = smoothstep(0.4, 1.0, vigDist);
                col.rgb         *= 1.0 - vigMask * _VignetteStrength;

                col.a *= texSample.a * clipAlpha;
                return col;
            }
            ENDCG
        }
    }
}
