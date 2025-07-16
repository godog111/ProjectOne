Shader "UI/Kino/DigitalGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _TrashTex ("Trash Texture", 2D) = "white" {}
        _Intensity ("Glitch Intensity", Range(0, 1)) = 0
        _Color ("Tint", Color) = (1,1,1,1)
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
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _TrashTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _TrashTex_ST;
            fixed4 _Color;
            float _Intensity;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UI裁剪
                half4 color = tex2D(_MainTex, i.texcoord) * i.color;
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                // 原始故障效果
                float4 glitch = tex2D(_NoiseTex, i.texcoord);

                float thresh = 1.001 - _Intensity * 1.001;
                float w_d = step(thresh, pow(glitch.z, 2.5)); // displacement glitch
                float w_f = step(thresh, pow(glitch.w, 2.5)); // frame glitch
                float w_c = step(thresh, pow(glitch.z, 3.5)); // color glitch

                // Displacement.
                float2 uv = frac(i.texcoord + glitch.xy * w_d);
                float4 source = tex2D(_MainTex, uv) * i.color;

                // Mix with trash frame.
                float3 result = lerp(source, tex2D(_TrashTex, uv), w_f).rgb;

                // Shuffle color components.
                float3 neg = saturate(result.grb + (1 - dot(result, 1)) * 0.5);
                result = lerp(result, neg, w_c);

                // 保留UI的透明度
                return fixed4(result, source.a * color.a);
            }
            ENDCG
        }
    }
}