Shader "UI/RoundedCorner"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0,0.5)) = 0.1
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
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
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            float _Radius;
            sampler2D _MainTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;

                // 计算当前像素在UV空间的位置（0~1）
                float2 uv = IN.texcoord;
                float2 aspect = float2(_ScreenParams.x / _ScreenParams.y, 1); // 可选，处理非方形图片

                // 计算四个角的距离，取最小值
                float2 bottomLeft = uv;
                float2 bottomRight = float2(1 - uv.x, uv.y);
                float2 topLeft = float2(uv.x, 1 - uv.y);
                float2 topRight = float2(1 - uv.x, 1 - uv.y);

                float minDist = min(
                    min(length(bottomLeft), length(bottomRight)),
                    min(length(topLeft), length(topRight))
                );

                // 圆角裁剪：距离小于半径则显示，否则丢弃
                clip(minDist - _Radius);
                // 如果想做抗锯齿边缘，可以用 smoothstep 替代 clip
                // color.a *= smoothstep(_Radius - 0.01, _Radius, minDist);

                return color;
            }
            ENDCG
        }
    }
}