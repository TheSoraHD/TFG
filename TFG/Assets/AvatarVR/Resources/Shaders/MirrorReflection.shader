// original source from: http://wiki.unity3d.com/index.php/MirrorReflection4

Shader "Custom/AvatarVR/Mirror/MirrorReflection"
{
    Properties
    {
        _MainTex ("_MainTex", 2D) = "white" {}
        _ReflectionTexLeft ("_ReflectionTexLeft", 2D) = "white" {}
        _ReflectionTexRight ("_ReflectionTexRight", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass 
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

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
            };

            float4 _MainTex_ST;
            sampler2D _MainTex;
            sampler2D _ReflectionTexLeft;
            sampler2D _ReflectionTexRight;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.refl = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 refl;
                if (unity_StereoEyeIndex == 0) 
                {
                	refl = tex2Dproj(_ReflectionTexLeft, UNITY_PROJ_COORD(i.refl));
                }
                else 
                {
                	refl = tex2Dproj(_ReflectionTexRight, UNITY_PROJ_COORD(i.refl));
                }
                return tex * refl;
            }

            ENDCG
        }
    }
}
