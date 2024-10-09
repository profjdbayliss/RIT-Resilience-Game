Shader "UI/DottedOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 1
        _DotSpeed ("Dot Speed", Float) = 1
        _DotSize ("Dot Size", Float) = 0.1
        _DotSpacing ("Dot Spacing", Float) = 0.2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _DotSpeed;
            float _DotSize;
            float _DotSpacing;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                float2 uv = IN.texcoord;
                float outlineWidth = _OutlineWidth / 100;

                float left = uv.x;
                float right = 1.0 - uv.x;
                float bottom = uv.y;
                float top = 1.0 - uv.y;

                float edgeDist = min(min(left, right), min(bottom, top));

                // Determine if we're on the outline
                float isOutline = step(edgeDist, outlineWidth);

                float perimDist = 0.0;

                // Calculate perimDist to increase continuously clockwise
                if (bottom <= outlineWidth && bottom <= left && bottom <= right)
                {
                    // Bottom edge (left to right)
                    perimDist = uv.x;
                }
                else if (right <= outlineWidth && right <= top && right <= bottom)
                {
                    // Right edge (bottom to top)
                    perimDist = 1.0 + uv.y;
                }
                else if (top <= outlineWidth && top <= left && top <= right)
                {
                    // Top edge (right to left)
                    perimDist = 2.0 + (1.0 - uv.x);
                }
                else if (left <= outlineWidth && left <= top && left <= bottom)
                {
                    // Left edge (top to bottom)
                    perimDist = 3.0 + (1.0 - uv.y);
                }

                // Total perimeter length is 4 units
                float perimeterLength = 4.0;

                // Adjust perimDist by time and speed
                float speed = _DotSpeed * _Time.y;
                float adjustedPerimDist = perimDist + speed;

                // Wrap around the perimeter
                adjustedPerimDist = fmod(adjustedPerimDist, perimeterLength);

                float dotPattern = frac(adjustedPerimDist / _DotSpacing);
                dotPattern = step(dotPattern, _DotSize);

                color = lerp(color, _OutlineColor, isOutline * dotPattern);

                return color;
            }
            ENDCG
        }
    }
}
