// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/AvatarVR/VictorStandardShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
		_ClippingMask("ClippingMask", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.5
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        // Do not render points this close to the current camera
        _rCutoff ("rCutoff", Float) = 0.3
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        // ------------------------------------------------------------------
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBaseWrapper //vertBase
            #pragma fragment fragBaseWrapper //fragBase
            #include "UnityStandardCoreForward.cginc"

            float _rCutoff;
			sampler2D _ClippingMask;

            struct VertexOutputForwardBaseWrapper
			{
			    VertexOutputForwardBase a;
			    float head : PSIZE;
				float2 uv : TEXCOORD8;
			};

            VertexOutputForwardBaseWrapper vertBaseWrapper(VertexInput v)
			{
			    VertexOutputForwardBase temp = vertBase(v);
			    VertexOutputForwardBaseWrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			half4 fragBaseWrapper(VertexOutputForwardBaseWrapper i) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;

				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return fragBase(i.a);
			}

            ENDCG
        }

        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        // ------------------------------------------------------------------
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }

            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAddWrapper //vertAdd
            #pragma fragment fragAddWrapper //fragAdd
            #include "UnityStandardCoreForward.cginc"

            float _rCutoff;
			sampler2D _ClippingMask;

			struct VertexOutputForwardAddWrapper
			{
			    VertexOutputForwardAdd a;
			    float head : PSIZE;
				float2 uv : TEXCOORD8;
			};
			
			VertexOutputForwardAddWrapper vertAddWrapper(VertexInput v)
			{
			    VertexOutputForwardAdd temp = vertAdd(v);
			    VertexOutputForwardAddWrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			half4 fragAddWrapper(VertexOutputForwardAddWrapper i) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;
				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}							    
			    return fragAdd(i.a);
			}

            ENDCG
        }

        // ------------------------------------------------------------------
        //  Shadow rendering pass
        // ------------------------------------------------------------------
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster //vertShadowCasterWrapper
            #pragma fragment fragShadowCaster //fragShadowCasterWrapper
            #include "UnityStandardShadow.cginc"

            // WE DO NOT APPLY rCutoff HERE, as we want to keep the casted shadows even if we are not rendering

            float _rCutoff;
			sampler2D _ClippingMask;
			
			void vertShadowCasterWrapper(VertexInput v, 
										 out float4 opos : SV_POSITION,
										 out float head : PSIZE,
										 out float2 uv : TEXCOORD8
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
									   , out VertexOutputShadowCaster o
#endif
#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
									   , out VertexOutputStereoShadowCaster os
#endif
			)
			{
			    vertShadowCaster(v, 
				                 opos
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
							   , o
#endif
#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
							   , os
#endif
				);
				
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				uv = v.uv0;
			}

			half4 fragShadowCasterWrapper(UNITY_POSITION(vpos),
			                              float head : PSIZE,
										  float2 uv : TEXCOORD8
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
										, VertexOutputShadowCaster i
#endif
			) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, uv).x;
				if (head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return fragShadowCaster(vpos
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
									  , i
#endif
				);
			}

			ENDCG
        }

        // ------------------------------------------------------------------
        //  Deferred pass
        // ------------------------------------------------------------------
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferredWrapper //vertDeferred
            #pragma fragment fragDeferredWrapper //fragDeferred
            #include "UnityStandardCore.cginc"

            float _rCutoff;
			sampler2D _ClippingMask;

			struct VertexOutputDeferredWrapper
			{
			    VertexOutputDeferred a;
			    float head : PSIZE;
				float2 uv : TEXCOORD8;
			};
			
			VertexOutputDeferredWrapper vertDeferredWrapper(VertexInput v)
			{
			    VertexOutputDeferred temp = vertDeferred(v);
			    VertexOutputDeferredWrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			void fragDeferredWrapper(VertexOutputDeferredWrapper i,
							         out half4 outGBuffer0 : SV_Target0,
                                     out half4 outGBuffer1 : SV_Target1,
                                     out half4 outGBuffer2 : SV_Target2,
                                     out half4 outEmission : SV_Target3 // RT3: emission (rgb), --unused-- (a)
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                                   , out half4 outShadowMask : SV_Target4 // RT4: shadowmask (rgba)
#endif
			)
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;
				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    fragDeferred(i.a,
			    			 outGBuffer0,
			    			 outGBuffer1,
			    			 outGBuffer2,
			    			 outEmission
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                            , outShadowMask
#endif
			    );
			}

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        // ------------------------------------------------------------------
        Pass
        {
            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #pragma vertex vert_meta //vert_meta_wrapper
            #pragma fragment frag_meta //frag_meta_wrapper
            #include "UnityStandardMeta.cginc"

            // WE DO NOT APPLY rCutoff HERE, as this pass it not used during regular rendering.

            float _rCutoff;
			sampler2D _ClippingMask;

			struct v2f_meta_wrapper
			{
			    v2f_meta a;
			    float head : PSIZE;
				float2 uv : TEXCOORD5;
			};

			v2f_meta_wrapper vert_meta_wrapper(VertexInput v)
			{
				v2f_meta temp = vert_meta(v);
			    v2f_meta_wrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			half4 frag_meta_wrapper(v2f_meta_wrapper i) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;
				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return frag_meta(i.a);
			}

            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        // ------------------------------------------------------------------
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex vertBaseWrapper //vertBase
            #pragma fragment fragBaseWrapper //fragBase
            #include "UnityStandardCoreForward.cginc"

            float _rCutoff;
			sampler2D _ClippingMask;

            struct VertexOutputForwardBaseWrapper
			{
			    VertexOutputForwardBase a;
			    float head : PSIZE;
				float2 uv : TEXCOORD8;
			};

            VertexOutputForwardBaseWrapper vertBaseWrapper(VertexInput v)
			{
			    VertexOutputForwardBase temp = vertBase(v);
			    VertexOutputForwardBaseWrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			half4 fragBaseWrapper(VertexOutputForwardBaseWrapper i) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;
				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return fragBase(i.a);
			}

            ENDCG
        }

        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        // ------------------------------------------------------------------
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }

            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAddWrapper //vertAdd
            #pragma fragment fragAddWrapper //fragAdd
            #include "UnityStandardCoreForward.cginc"

            float _rCutoff;
			sampler2D _ClippingMask;

			struct VertexOutputForwardAddWrapper
			{
			    VertexOutputForwardAdd a;
			    float head : PSIZE;
				float2 uv : TEXCOORD8;
			};
			
			VertexOutputForwardAddWrapper vertAddWrapper(VertexInput v)
			{
			    VertexOutputForwardAdd temp = vertAdd(v);
			    VertexOutputForwardAddWrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			half4 fragAddWrapper(VertexOutputForwardAddWrapper i) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;
				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return fragAdd(i.a);
			}

            ENDCG
        }

        // ------------------------------------------------------------------
        //  Shadow rendering pass
        // ------------------------------------------------------------------
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster //vertShadowCasterWrapper
            #pragma fragment fragShadowCaster //fragShadowCasterWrapper
            #include "UnityStandardShadow.cginc"

            // WE DO NOT APPLY rCutoff HERE, as we want to keep the casted shadows even if we are not rendering

            float _rCutoff;
			sampler2D _ClippingMask;
			
			void vertShadowCasterWrapper(VertexInput v, 
										 out float4 opos : SV_POSITION,
										 out float head : PSIZE,
										 out float2 uv : TEXCOORD8
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
									   , out VertexOutputShadowCaster o
#endif
#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
									   , out VertexOutputStereoShadowCaster os
#endif
			)
			{
			    vertShadowCaster(v, 
				                 opos
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
							   , o
#endif
#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
							   , os
#endif
				);
				
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				uv = v.uv0;
			}

			half4 fragShadowCasterWrapper(UNITY_POSITION(vpos),
			                              float head : PSIZE,
										  float2 uv : TEXCOORD8
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
										, VertexOutputShadowCaster i
#endif
			) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, uv).x;
				if (head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return fragShadowCaster(vpos
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
									  , i
#endif
				);
			}

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        // ------------------------------------------------------------------
        Pass
        {
            Name "META"
            Tags { "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #pragma vertex vert_meta //vert_meta_wrapper
            #pragma fragment frag_meta //frag_meta_wrapper
            #include "UnityStandardMeta.cginc"

            // WE DO NOT APPLY rCutoff HERE, as this pass it not used during regular rendering.

            float _rCutoff;
			sampler2D _ClippingMask;

			struct v2f_meta_wrapper
			{
			    v2f_meta a;
			    float head : PSIZE;
				float2 uv : TEXCOORD5;
			};

			v2f_meta_wrapper vert_meta_wrapper(VertexInput v)
			{
				v2f_meta temp = vert_meta(v);
			    v2f_meta_wrapper o;
			    o.a = temp;
			    float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
			    o.head = length(posWorld.xyz - _WorldSpaceCameraPos.xyz) < _rCutoff;
				o.uv = v.uv0;
			    return o;
			}

			half4 frag_meta_wrapper(v2f_meta_wrapper i) : SV_Target
			{
				float clippingMask = tex2D(_ClippingMask, i.uv).x;
				if (i.head > 0.0f && clippingMask > 0.5f)
				{
					discard;
				}
			    return frag_meta(i.a);
			}

            ENDCG
        }
    }

    FallBack "VertexLit"
    CustomEditor "VictorStandardShaderGUI"
}
