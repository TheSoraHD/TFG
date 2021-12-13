// original source from: http://wiki.unity3d.com/index.php/MirrorReflection4

Shader "Custom/AvatarVR/Mirror/DoublyTexturedMirrorReflection"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _ReflectionTexLeft ("Reflection Texture (Left)", 2D) = "white" {}
        _ReflectionTexRight ("Reflection Texture (Right)", 2D) = "white" {}
        _DisplayTex ("Display Texture", 2D) = "black" {}

        [Toggle(FLIP_DISPLAY)]
        _FlipDisplayTex ("Flip Display Texture Horizontally", Float) = 0
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

            #pragma shader_feature FLIP_DISPLAY

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 refl : TEXCOORD1;
                float2 disp : TEXCOORD2;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReflectionTexLeft;
            sampler2D _ReflectionTexRight;
            sampler2D _DisplayTex;
			float4 _DisplayTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.refl = ComputeScreenPos(o.pos);
                o.disp = TRANSFORM_TEX(v.uv, _DisplayTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 mainTex = tex2D(_MainTex, i.uv);                

                fixed4 refl;
                if (unity_StereoEyeIndex == 0) 
                {
                	refl = tex2Dproj(_ReflectionTexLeft, UNITY_PROJ_COORD(i.refl));
                }
                else 
                {
                	refl = tex2Dproj(_ReflectionTexRight, UNITY_PROJ_COORD(i.refl));
                }

				mainTex = mainTex * refl;

                #ifdef FLIP_DISPLAY
                	float2 displayFlipped_uv = float2(i.disp);
	                displayFlipped_uv.x = 1.0 - displayFlipped_uv.x;
                	float4 dispTex = tex2D(_DisplayTex, displayFlipped_uv);
                #else
					float4 dispTex = tex2D(_DisplayTex, i.disp);
				#endif

				float overlayAlpha = dispTex.a;
				float mainAlpha = (1.0f - dispTex.a) * mainTex.a;
				float backgroundAlpha = (1.0f - dispTex.a) * (1.0f - dispTex.a) * _Color.a;

				float3 finalColor = overlayAlpha * dispTex.rgb + mainAlpha * mainTex.rgb + backgroundAlpha * _Color;
				float finalAlpha = min(1.0, dispTex.a+mainTex.a+_Color.a);

				return fixed4(finalColor.r, finalColor.g, finalColor.b, finalAlpha);
            }

            ENDCG
        }
    }
}
