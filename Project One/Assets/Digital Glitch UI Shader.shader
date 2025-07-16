Shader "UI/Kino/Glitch/Digital"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        
        // Glitch properties
        _Intensity ("Glitch Intensity", Range(0, 1)) = 0
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _TrashTex ("Trash Texture", 2D) = "white" {}
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
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            
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
            sampler2D _NoiseTex;
            sampler2D _TrashTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _Intensity;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                float4 glitch = tex2D(_NoiseTex, IN.texcoord);
                
                float thresh = 1.001 - _Intensity * 1.001;
                float w_d = step(thresh, pow(glitch.z, 2.5)); // displacement glitch
                float w_f = step(thresh, pow(glitch.w, 2.5)); // frame glitch
                float w_c = step(thresh, pow(glitch.z, 3.5)); // color glitch
                
                // Displacement
                float2 uv = frac(IN.texcoord + glitch.xy * w_d);
                float4 source = tex2D(_MainTex, uv);
                
                // Mix with trash frame
                float3 color = lerp(source, tex2D(_TrashTex, uv), w_f).rgb;
                
                // Shuffle color components
                float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.5);
                color = lerp(color, neg, w_c);
                
                half4 result = half4(color, source.a);
                result *= IN.color;
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (result.a - 0.001);
                #endif
                
                return result;
            }
            ENDCG
        }
    }
}