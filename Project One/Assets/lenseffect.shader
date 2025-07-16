Shader "Custom/TextBlockLensEffect_Pro"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Distortion ("Distortion", Range(0, 2)) = 0.5
        _Radius ("Effect Radius", Range(0, 1)) = 0.3
        _MousePos ("Mouse Position (UV Space)", Vector) = (0.5,0.5,0,0)
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "DisableBatching"="True" // 防止批处理破坏坐标计算
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 localPos : TEXCOORD1; // 新增局部坐标
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Distortion;
            float _Radius;
            float4 _MousePos;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // 关键改进：记录物体空间坐标
                o.localPos = v.vertex.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算鼠标距离（基于文本块局部坐标）
                float2 mouseDir = _MousePos.xy - float2(
                    (i.localPos.x + 0.5), 
                    (i.localPos.y + 0.5)
                );
                float mouseDist = length(mouseDir);

                // 全局透镜变形
                if (mouseDist < _Radius)
                {
                    float2 dir = normalize(mouseDir);
                    float falloff = 1.0 - saturate(mouseDist / _Radius);
                    float2 distortedUV = i.uv + dir * falloff * _Distortion * 0.1;
                    return tex2D(_MainTex, distortedUV);
                }
                
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}