Shader "MoreHit/RedMistEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 0.2, 0.2, 1)
        _NoiseScale ("Noise Scale", Float) = 2.0
        _Speed ("Animation Speed", Float) = 1.0
        _Intensity ("Effect Intensity", Range(0, 2)) = 1.0
        _EdgeFade ("Edge Fade", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _NoiseScale;
            float _Speed;
            float _Intensity;
            float _EdgeFade;

            // シンプルなノイズ関数
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // パーリンノイズ風の関数
            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;
                
                // 中心からの距離を計算（円形フェード用）
                float2 center = float2(0.5, 0.5);
                float distFromCenter = distance(uv, center);
                
                // ノイズレイヤー1（粗い動き）
                float2 noiseUV1 = uv * _NoiseScale + float2(time * 0.1, time * 0.05);
                float noise1 = smoothNoise(noiseUV1);
                
                // ノイズレイヤー2（細かい動き）
                float2 noiseUV2 = uv * _NoiseScale * 2.0 + float2(time * 0.15, -time * 0.08);
                float noise2 = smoothNoise(noiseUV2);
                
                // ノイズレイヤー3（揺らめき効果）
                float2 noiseUV3 = uv * _NoiseScale * 0.5 + float2(-time * 0.08, time * 0.12);
                float noise3 = smoothNoise(noiseUV3);
                
                // ノイズを合成
                float combinedNoise = (noise1 * 0.5 + noise2 * 0.3 + noise3 * 0.2);
                
                // エッジフェード（中心から外側に向けてフェード）
                float edgeFade = 1.0 - smoothstep(0.2, 0.5, distFromCenter);
                edgeFade *= (1.0 - smoothstep(0.0, _EdgeFade, distFromCenter));
                
                // 脈動効果
                float pulse = (sin(time * 2.0) * 0.5 + 0.5) * 0.3 + 0.7;
                
                // 最終的なアルファ値
                float alpha = combinedNoise * edgeFade * _Intensity * pulse;
                alpha = saturate(alpha);
                
                // メインテクスチャをサンプリング
                fixed4 tex = tex2D(_MainTex, uv);
                
                // 赤いもやもや色を適用
                fixed4 finalColor = _Color * i.color;
                finalColor.a *= alpha * tex.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}