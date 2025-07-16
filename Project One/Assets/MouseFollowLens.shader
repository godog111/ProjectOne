Shader "Custom/UI_LensEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _LensSize ("Lens Size", Range(0.01, 0.5)) = 0.2
        _MaxDistortion ("Max Distortion", Range(0, 1)) = 0.5
        _Magnification ("Magnification", Range(1, 5)) = 2
        _Smoothness ("Smoothness", Range(0.01, 1)) = 0.3
        _MousePos ("Mouse Position", Vector) = (0,0,0,0)
        
        // UI必要属性
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 texcoord  : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _LensSize;
            float _MaxDistortion;
            float _Magnification;
            float _Smoothness;
            float2 _MousePos;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 screenUV = IN.texcoord;
                float2 delta = screenUV - _MousePos;
                float distance = length(delta);
                
                float lensFactor = saturate(1 - (distance / _LensSize));
                float smoothFactor = smoothstep(0, 1, lensFactor);
                float distortion = _MaxDistortion * smoothFactor;
                
                float2 distortedUV = screenUV;
                if (distance < _LensSize)
                {
                    float2 lensUV = _MousePos + delta * (1 - distortion) / _Magnification;
                    distortedUV = lerp(screenUV, lensUV, smoothFactor);
                }
                
                half4 color = tex2D(_MainTex, distortedUV) * IN.color;
                return color;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}