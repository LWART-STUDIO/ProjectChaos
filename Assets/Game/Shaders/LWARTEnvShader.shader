// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

Shader "LWART/EnvShader"
{
	Properties
	{
		[Enum(Front, 2, Back, 1, Both, 0)] _Cull ("Render Face", Float) = 2.0
		[TCP2ToggleNoKeyword] _ZWrite ("Depth Write", Float) = 1.0
		[HideInInspector] _RenderingMode ("rendering mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("blending source", Float) = 1.0
		[HideInInspector] _DstBlend ("blending destination", Float) = 0.0
		[TCP2Separator]

		// Injection Point: 'Properties/Start'
		[TCP2HeaderHelp(Base)]
		_Color ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		[MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		[TCP2Header(Ramp Shading)]
		
		_RampThreshold ("Threshold", Range(0.01,1)) = 0.5
		_RampSmoothing ("Smoothing", Range(0.001,1)) = 0.5
		[TCP2Separator]
		
		// Injection Point: 'Properties/End'

		[ToggleOff(_RECEIVE_SHADOWS_OFF)] _ReceiveShadowsOff ("Receive Shadows", Float) = 1

		// Avoid compile error if the properties are ending with a drawer
		[HideInInspector] __dummy__ ("unused", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType"="Opaque"
			// Injection Point: 'SubShader/Tags'
		}

		// Injection Point: 'SubShader/Shader States'

		HLSLINCLUDE
		#define fixed half
		#define fixed2 half2
		#define fixed3 half3
		#define fixed4 half4

		#if UNITY_VERSION >= 202020
			#define URP_10_OR_NEWER
		#endif
		#if UNITY_VERSION >= 202120
			#define URP_12_OR_NEWER
		#endif
		#if UNITY_VERSION >= 202220
			#define URP_14_OR_NEWER
		#endif

		// Texture/Sampler abstraction
		#define TCP2_TEX2D_WITH_SAMPLER(tex)						TEXTURE2D(tex); SAMPLER(sampler##tex)
		#define TCP2_TEX2D_NO_SAMPLER(tex)							TEXTURE2D(tex)
		#define TCP2_TEX2D_SAMPLE(tex, samplertex, coord)			SAMPLE_TEXTURE2D(tex, sampler##samplertex, coord)
		#define TCP2_TEX2D_SAMPLE_LOD(tex, samplertex, coord, lod)	SAMPLE_TEXTURE2D_LOD(tex, sampler##samplertex, coord, lod)

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

		// Injection Point: 'Include Files'

		// Uniforms

		// Shader Properties
		TCP2_TEX2D_WITH_SAMPLER(_BaseMap);
		// Injection Point: 'Variables/Outside CBuffer'

		CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			float4 _BaseMap_ST;
			fixed4 _Color;
			float _RampThreshold;
			float _RampSmoothing;
			fixed4 _SColor;
			fixed4 _HColor;
			// Injection Point: 'Variables/Inside CBuffer'
		CBUFFER_END

		#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_DOTS_INSTANCING_ENABLED)
			#define unity_ObjectToWorld UNITY_MATRIX_M
			#define unity_WorldToObject UNITY_MATRIX_I_M
		#endif

		// Built-in renderer (CG) to SRP (HLSL) bindings
		#define UnityObjectToClipPos TransformObjectToHClip
		#define _WorldSpaceLightPos0 _MainLightPosition
		
		// Injection Point: 'Functions'

		ENDHLSL

		Pass
		{
			Name "Main"
			Tags
			{
				"LightMode"="UniversalForward"
				// Injection Point: 'Main Pass/Tags'
			}
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
			// Injection Point: 'Main Pass/Shader States'

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 3.0
			// Injection Point: 'Main Pass/Pragma'

			// -------------------------------------
			// Material keywords
			#pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Universal Render Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			// -------------------------------------

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			#pragma vertex Vertex
			#pragma fragment Fragment

			//--------------------------------------
			// Toony Colors Pro 2 keywords
		#pragma shader_feature_local _ _ALPHAPREMULTIPLY_ON

			// vertex input
			struct Attributes
			{
				float4 vertex       : POSITION;
				float3 normal       : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				// Injection Point: 'Main Pass/Attributes'
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// vertex output / fragment input
			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
				float3 normal         : NORMAL;
				float4 worldPosAndFog : TEXCOORD0;
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord    : TEXCOORD1; // compute shadow coord per-vertex for the main light
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				half3 vertexLights : TEXCOORD2;
			#endif
				float2 pack0 : TEXCOORD3; /* pack0.xy = texcoord0 */
				// Injection Point: 'Main Pass/Varyings'
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				// Injection Point: 'Main Pass/Vertex Shader/Start'

				// Texture Coordinates
				output.pack0.xy.xy = input.texcoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				output.shadowCoord = GetShadowCoord(vertexInput);
			#endif

				VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normal);
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				// Vertex lighting
				output.vertexLights = VertexLighting(vertexInput.positionWS, vertexNormalInput.normalWS);
			#endif

				// world position
				output.worldPosAndFog = float4(vertexInput.positionWS.xyz, 0);

				// normal
				output.normal = normalize(vertexNormalInput.normalWS);

				// clip position
				output.positionCS = vertexInput.positionCS;

				// Injection Point: 'Main Pass/Vertex Shader/End'

				return output;
			}

			half4 Fragment(Varyings input
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				// Injection Point: 'Main Pass/Fragment Shader/Start'

				float3 positionWS = input.worldPosAndFog.xyz;
				float3 normalWS = normalize(input.normal);

				// Shader Properties Sampling
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack0.xy).rgba );
				float4 __mainColor = ( _Color.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __ambientIntensity = ( 1.0 );
				float __rampThreshold = ( _RampThreshold );
				float __rampSmoothing = ( _RampSmoothing );
				float3 __shadowColor = ( _SColor.rgb );
				float3 __highlightColor = ( _HColor.rgb );

				// main texture
				half3 albedo = __albedo.rgb;
				half alpha = __alpha;

				half3 emission = half3(0,0,0);
				
				albedo *= __mainColor.rgb;

				// main light: direction, color, distanceAttenuation, shadowAttenuation
			#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord = input.shadowCoord;
			#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
			#else
				float4 shadowCoord = float4(0, 0, 0, 0);
			#endif

			#if defined(URP_10_OR_NEWER)
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
					half4 shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
				#elif !defined (LIGHTMAP_ON)
					half4 shadowMask = unity_ProbesOcclusion;
				#else
					half4 shadowMask = half4(1, 1, 1, 1);
				#endif

				Light mainLight = GetMainLight(shadowCoord, positionWS, shadowMask);
			#else
				Light mainLight = GetMainLight(shadowCoord);
			#endif

				// ambient or lightmap
				// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
				// are also defined in case you want to sample some terms per-vertex.
				half3 bakedGI = SampleSH(normalWS);
				half occlusion = 1;

				half3 indirectDiffuse = bakedGI;
				indirectDiffuse *= occlusion * albedo * __ambientIntensity;

				half3 lightDir = mainLight.direction;
				half3 lightColor = mainLight.color.rgb;

				half atten = mainLight.shadowAttenuation * mainLight.distanceAttenuation;

				half ndl = dot(normalWS, lightDir);
				half3 ramp;
				
				// Wrapped Lighting
				ndl = ndl * 0.5 + 0.5;
				
				half rampThreshold = __rampThreshold;
				half rampSmooth = __rampSmoothing * 0.5;
				ndl = saturate(ndl);
				ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

				// apply attenuation
				ramp *= atten;

				half3 color = half3(0,0,0);
				half3 accumulatedRamp = ramp * max(lightColor.r, max(lightColor.g, lightColor.b));
				half3 accumulatedColors = ramp * lightColor.rgb;

				// Additional lights loop
			#ifdef _ADDITIONAL_LIGHTS
				uint pixelLightCount = GetAdditionalLightsCount();

				LIGHT_LOOP_BEGIN(pixelLightCount)
				{
					#if defined(URP_10_OR_NEWER)
						Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
					#else
						Light light = GetAdditionalLight(lightIndex, positionWS);
					#endif
					half atten = light.shadowAttenuation * light.distanceAttenuation;

					#if defined(_LIGHT_LAYERS)
						half3 lightDir = half3(0, 1, 0);
						half3 lightColor = half3(0, 0, 0);
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						{
							lightColor = light.color.rgb;
							lightDir = light.direction;
						}
					#else
						half3 lightColor = light.color.rgb;
						half3 lightDir = light.direction;
					#endif

					half ndl = dot(normalWS, lightDir);
					half3 ramp;
					
					// Wrapped Lighting
					ndl = ndl * 0.5 + 0.5;
					
					ndl = saturate(ndl);
					ramp = smoothstep(rampThreshold - rampSmooth, rampThreshold + rampSmooth, ndl);

					// apply attenuation (shadowmaps & point/spot lights attenuation)
					ramp *= atten;

					accumulatedRamp += ramp * max(lightColor.r, max(lightColor.g, lightColor.b));
					accumulatedColors += ramp * lightColor.rgb;

				}
				LIGHT_LOOP_END
			#endif
			#ifdef _ADDITIONAL_LIGHTS_VERTEX
				color += input.vertexLights * albedo;
			#endif

				accumulatedRamp = saturate(accumulatedRamp);
				half3 shadowColor = (1 - accumulatedRamp.rgb) * __shadowColor;
				accumulatedRamp = accumulatedColors.rgb * __highlightColor + shadowColor;
				color += albedo * accumulatedRamp;

				// apply ambient
				color += indirectDiffuse;

				// Premultiply blending
				#if defined(_ALPHAPREMULTIPLY_ON)
					color.rgb *= alpha;
				#endif

				color += emission;

				// Injection Point: 'Main Pass/Fragment Shader/End'

				return half4(color, alpha);
			}
			ENDHLSL
		}

		// Depth & Shadow Caster Passes
		HLSLINCLUDE

		#if defined(SHADOW_CASTER_PASS) || defined(DEPTH_ONLY_PASS)

			#define fixed half
			#define fixed2 half2
			#define fixed3 half3
			#define fixed4 half4

			float3 _LightDirection;
			float3 _LightPosition;

			struct Attributes
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float4 texcoord0 : TEXCOORD0;
				// Injection Point: 'Depth + Shadow Caster Pass/Attributes'
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS     : SV_POSITION;
			#if defined(DEPTH_NORMALS_PASS)
				float3 normalWS : TEXCOORD0;
			#endif
				float2 pack0 : TEXCOORD1; /* pack0.xy = texcoord0 */
				// Injection Point: 'Depth + Shadow Caster Pass/Varyings'
			#if defined(DEPTH_ONLY_PASS)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			#endif
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normal);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif
				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}

			Varyings ShadowDepthPassVertex(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				#if defined(DEPTH_ONLY_PASS)
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				#endif

				// Injection Point: 'Depth + Shadow Caster Pass/Vertex Shader/Start'

				// Texture Coordinates
				output.pack0.xy.xy = input.texcoord0.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;

				#if defined(DEPTH_ONLY_PASS)
					output.positionCS = TransformObjectToHClip(input.vertex.xyz);
					#if defined(DEPTH_NORMALS_PASS)
						float3 normalWS = TransformObjectToWorldNormal(input.normal);
						output.normalWS = normalWS; // already normalized in TransformObjectToWorldNormal
					#endif
				#elif defined(SHADOW_CASTER_PASS)
					output.positionCS = GetShadowPositionHClip(input);
				#else
					output.positionCS = float4(0,0,0,0);
				#endif

				// Injection Point: 'Depth + Shadow Caster Pass/Vertex Shader/End'

				return output;
			}

			half4 ShadowDepthPassFragment(
				Varyings input
	#if defined(DEPTH_NORMALS_PASS) && defined(_WRITE_RENDERING_LAYERS)
				, out float4 outRenderingLayers : SV_Target1
	#endif
			) : SV_TARGET
			{
				#if defined(DEPTH_ONLY_PASS)
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				#endif

				// Injection Point: 'Depth + Shadow Caster Pass/Fragment Shader/Start'

				// Shader Properties Sampling
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack0.xy).rgba );
				float4 __mainColor = ( _Color.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );

				half3 albedo = half3(1,1,1);
				half alpha = __alpha;
				half3 emission = half3(0,0,0);

				// Injection Point: 'Depth + Shadow Caster Pass/Fragment Shader/End'

				#if defined(DEPTH_NORMALS_PASS)
					#if defined(_WRITE_RENDERING_LAYERS)
						uint meshRenderingLayers = GetMeshRenderingLayer();
						outRenderingLayers = float4(EncodeMeshRenderingLayer(meshRenderingLayers), 0, 0, 0);
					#endif

					#if defined(URP_12_OR_NEWER)
						return float4(input.normalWS.xyz, 0.0);
					#else
						return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS, true)), 0.0, 0.0);
					#endif
				#endif

				return 0;
			}

		#endif
		ENDHLSL

		Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
				// Injection Point: 'Shadow Caster Pass/Tags'
			}

			ZWrite On
			ZTest LEqual
			// Injection Point: 'Shadow Caster Pass/Shader States'

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0
			// Injection Point: 'Shadow Caster Pass/Pragma'

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile SHADOW_CASTER_PASS

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags
			{
				"LightMode" = "DepthOnly"
				// Injection Point: 'Depth Pass/Tags'
			}

			ZWrite On
			ColorMask 0
			Cull [_Cull]
			// Injection Point: 'Depth Pass/Shader States'

			HLSLPROGRAM

			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0
			// Injection Point: 'Depth Pass/Pragma'

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile DEPTH_ONLY_PASS

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			ENDHLSL
		}

		Pass
		{
			Name "DepthNormals"
			Tags
			{
				"LightMode" = "DepthNormals"
				// Injection Point: 'Depth Normals Pass/Tags'
			}

			ZWrite On
			// Injection Point: 'Depth Normals Pass/Shader States'

			HLSLPROGRAM
			#pragma exclude_renderers gles gles3 glcore
			#pragma target 2.0
			// Injection Point: 'Depth Normals Pass/Pragma'

			#pragma multi_compile_instancing

			// using simple #define doesn't work, we have to use this instead
			#pragma multi_compile DEPTH_ONLY_PASS
			#pragma multi_compile DEPTH_NORMALS_PASS

			#pragma vertex ShadowDepthPassVertex
			#pragma fragment ShadowDepthPassFragment

			ENDHLSL
		}

	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.9.10";unity:"6000.0.58f2";tmplt:"SG2_Template_URP";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","UNITY_2019_4","UNITY_2020_1","UNITY_2021_1","UNITY_2021_2","UNITY_2022_2","AUTO_TRANSPARENT_BLENDING","SUBSURFACE_AMB_COLOR","WRAPPED_LIGHTING_HALF","ENABLE_DEPTH_NORMALS_PASS","TEMPLATE_LWRP"];flags:list[];flags_extra:dict[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",GPU_INSTANCING_MAX_COUNT_VALUE="50"];shaderProperties:list[,sp(name:"Main Color";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:4;chan:"RGBA";prop:"_Color";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"5c97d97c-03b8-40ad-b3d7-d360fdd8e2af";op:Multiply;lbl:"Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False)];customTextures:list[];codeInjection:codeInjection(injectedFiles:list[];mark:True);matLayers:list[]) */
/* TCP_HASH 471b9a2fe4516029b0c3594311f61972 */
