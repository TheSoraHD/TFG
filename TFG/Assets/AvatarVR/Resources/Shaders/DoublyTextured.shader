Shader "Custom/AvatarVR/Mirror/DoublyTextured"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture (RGBA)", 2D) = "black" {}
		_SecondaryTex ("Overlay Texture (RGBA)", 2D) = "black" {}
		[Toggle(FLIP_MAIN)]
        _FlipMainTex ("Flip Main Texture Horizontally", Float) = 0
        [Toggle(FLIP_SECONDARY)]
        _FlipSecondaryTex ("Flip Secondary Texture Horizontally", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature FLIP_MAIN
            #pragma shader_feature FLIP_SECONDARY

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_SecondaryTex : TEXCOORD1;
            };

            fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _SecondaryTex;
			float4 _MainTex_ST;
			float4 _SecondaryTex_ST;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_SecondaryTex = TRANSFORM_TEX(v.uv, _SecondaryTex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef FLIP_MAIN
	                float2 mainFlippedUV = float2(i.uv_MainTex);
	                mainFlippedUV.x = 1.0 - mainFlippedUV.x;
					float4 mainTex = tex2D(_MainTex, mainFlippedUV);
                #else
					float4 mainTex = tex2D(_MainTex, i.uv_MainTex);
                #endif

                #ifdef FLIP_SECONDARY
                	float2 secondaryFlipped_uv = float2(i.uv_SecondaryTex);
	                secondaryFlipped_uv.x = 1.0 - secondaryFlipped_uv.x;
                	float4 overlayTex = tex2D(_SecondaryTex, secondaryFlipped_uv);
                #else
					float4 overlayTex = tex2D(_SecondaryTex, i.uv_SecondaryTex);
				#endif

				float overlayAlpha = overlayTex.a;
				float mainAlpha = (1.0f - overlayTex.a) * mainTex.a;
				float backgroundAlpha = (1.0f - overlayTex.a) * (1.0f - mainTex.a) * _Color.a;

				float3 finalColor = overlayAlpha * overlayTex.rgb + mainAlpha * mainTex.rgb + backgroundAlpha * _Color;
				float finalAlpha = min(1.0, overlayTex.a+mainTex.a+_Color.a);

				return fixed4(finalColor.r, finalColor.g, finalColor.b, finalAlpha);
            }

            ENDCG
        }
    }
}