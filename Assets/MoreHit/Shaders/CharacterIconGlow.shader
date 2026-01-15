Shader "UI/IconGlowEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // グローエフェクト設定
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.5
        _GlowSize ("Glow Size", Range(0, 0.1)) = 0.03
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
        
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
            Name "Default"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
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
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowSize;
            float _PulseSpeed;
            float _PulseAmount;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // メインテクスチャ
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // パルスエフェクト
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseAmount + 1.0;
                
                // グローエフェクト（エッジ検出）
                float2 glowOffset = _GlowSize * pulse;
                
                // 周囲のサンプリング（8方向）
                half4 glow = half4(0, 0, 0, 0);
                float samples = 8.0;
                
                for(float i = 0; i < samples; i++)
                {
                    float angle = (i / samples) * 6.28318; // 2 * PI
                    float2 offset = float2(cos(angle), sin(angle)) * glowOffset;
                    glow += tex2D(_MainTex, IN.texcoord + offset);
                }
                
                glow /= samples;
                glow.rgb = _GlowColor.rgb * _GlowIntensity;
                glow.a *= _GlowColor.a;
                
                // グローとメインテクスチャの合成
                color.rgb = lerp(glow.rgb, color.rgb, color.a);
                color.a = max(color.a, glow.a * 0.5);
                
                // クリッピング
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
                
                return color;
            }
            ENDCG
        }
    }
}