// Toony Colors Pro+Mobile 2
// (c) 2014-2023 Jean Moreno

Shader "LWART/EnvShaderWithowShadow"
{
	Properties
	{
		[Enum(Front, 2, Back, 1, Both, 0)] _Cull ("Render Face", Float) = 2.0
		[TCP2ToggleNoKeyword] _ZWrite ("Depth Write", Float) = 1.0
		[HideInInspector] _RenderingMode ("rendering mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("blending source", Float) = 1.0
		[HideInInspector] _DstBlend ("blending destination", Float) = 0.0
		[TCP2Separator]

		//================================
		// Injected Code for 'Properties/Start'
		[TCP2Header(Dissolve)]
		[NoScaleOffset] _MaskNoiseMap ("Mask Noise Map", 2D) = "white" {}
		[HDR] _MaskEdgeColor ("Mask Edge Color", Color) = (1,1,1,1)
		_AlphaClipThreshold ("Alpha Clip Threshold", Range(0.0, 1.0)) = 0.5
		_EdgeThreshold ("Edge Threshold", Range(0.0, 1.0)) = 0.5
		[ToggleUI] _InvertDissolveEffect ("Invert Dissolve Effect", Float) = 0.0
		[TCP2Separator]
		//================================

		[TCP2HeaderHelp(Base)]
		_Color ("Color", Color) = (1,1,1,1)
		[TCP2ColorNoAlpha] _HColor ("Highlight Color", Color) = (0.75,0.75,0.75,1)
		[TCP2ColorNoAlpha] _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
		[MainTexture] _BaseMap ("Albedo", 2D) = "white" {}
		[TCP2Separator]

		// Injection Point: 'Properties/End'

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

		//================================
		// Injected Code for 'Include Files'
		#include "Assets/Plugins/VFX/Amazing Assets/Dynamic Radial Masks/Shaders/CGINC/HeightField/DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global.cginc"
		TCP2_TEX2D_WITH_SAMPLER(_MaskNoiseMap); // Объявляем текстуру вне CBUFFER
		//================================

		// Uniforms

		// Shader Properties
		TCP2_TEX2D_WITH_SAMPLER(_BaseMap);
		//================================
		// Injected Code for 'Variables/Outside CBuffer'
		// Переменные из свойств объявляем здесь, т.к. TCP2 может не помещать все в CBUFFER автоматически в этом случае
		fixed4 _MaskEdgeColor;
		float _AlphaClipThreshold;
		float _InvertDissolveEffect;
		float _EdgeThreshold;
		//================================

		CBUFFER_START(UnityPerMaterial)
			
			// Shader Properties
			float4 _BaseMap_ST;
			fixed4 _Color;
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
				//================================
				// Injected Code for 'Main Pass/Tags'
				"RenderType" = "TransparentCutout"
				//================================

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
			//================================
			// Injected Code for 'Main Pass/Pragma'
			#pragma shader_feature_local _ALPHATEST_ON
			// Не используем ToggleOff, просто Float, поэтому не добавляем _INVERTDISSOLVE_ON
			//================================

			// -------------------------------------
			// Material keywords
			#pragma multi_compile _RECEIVE_SHADOWS_OFF

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
				//================================
				// Injected Code for 'Main Pass/Varyings'
				// Не нужно добавлять worldPos, т.к. используем input.worldPosAndFog.xyz
				//================================

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

				//================================
				// Injected Code for 'Main Pass/Fragment Shader/Start'
				// Сэмплируем шумовую карту
				half4 noiseSample = TCP2_TEX2D_SAMPLE(_MaskNoiseMap, _MaskNoiseMap, input.pack0.xy);
				half noiseValue = noiseSample.r;
				
				// Вычисляем маску диссольва DRM
				float dissolveMask = DynamicRadialMasks_HeightField_1_Advanced_Normalized_ID1_Global(input.worldPosAndFog.xyz, noiseValue);
				
				// Применяем инверсию, если нужно (используем Float, а не keyword)
				if (_InvertDissolveEffect > 0.5) {
				    dissolveMask = 1.0 - dissolveMask;
				}
				//================================

				float3 positionWS = input.worldPosAndFog.xyz;
				float3 normalWS = normalize(input.normal);

				// Shader Properties Sampling
				float4 __albedo = ( TCP2_TEX2D_SAMPLE(_BaseMap, _BaseMap, input.pack0.xy).rgba );
				float4 __mainColor = ( _Color.rgba );
				float __alpha = ( __albedo.a * __mainColor.a );
				float __ambientIntensity = ( 1.0 );
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

				half atten = mainLight.shadowAttenuation;

				half ndl = dot(normalWS, lightDir);
				half3 ramp;
				
				ndl = saturate(ndl);
				ramp = float3(1, 1, 1);

				// apply attenuation
				ramp *= atten;

				// highlight/shadow colors
				ramp = lerp(__shadowColor, __highlightColor, ramp);
				
				// output color
				half3 color = half3(0,0,0);
				color += albedo * ramp;

				// Additional lights loop

				// apply ambient
				color += indirectDiffuse;

				// Premultiply blending
				#if defined(_ALPHAPREMULTIPLY_ON)
					color.rgb *= alpha;
				#endif

				color += emission;

				//================================
				// Injected Code for 'Main Pass/Fragment Shader/End'
				// Проверяем маску диссольва против порога (как в рабочем шейдере)
				clip(dissolveMask - _AlphaClipThreshold);
				
				// Добавляем цвет края диссольва к эмиссии (как в рабочем шейдере)
				// Используем _RampSmoothing как ширину края
				half3 dissolveEdgeMask = saturate(1.0 - abs(dissolveMask - _AlphaClipThreshold) / max(0.001, _EdgeThreshold)); // __rampSmoothing из TCP2
				//color-=emission;
				emission += dissolveEdgeMask * _MaskEdgeColor.rgb;
				color+=emission;
				//================================

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

				return 0;
			}

		#endif
		ENDHLSL

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

	}

	FallBack "Hidden/InternalErrorShader"
	CustomEditor "ToonyColorsPro.ShaderGenerator.MaterialInspector_SG2"
}

/* TCP_DATA u config(ver:"2.9.10";unity:"6000.3.2f1";tmplt:"SG2_Template_URP";features:list["UNITY_5_4","UNITY_5_5","UNITY_5_6","UNITY_2017_1","UNITY_2018_1","UNITY_2018_2","UNITY_2018_3","UNITY_2019_1","UNITY_2019_2","UNITY_2019_3","UNITY_2019_4","UNITY_2020_1","UNITY_2021_1","UNITY_2021_2","UNITY_2022_2","AUTO_TRANSPARENT_BLENDING","SUBSURFACE_AMB_COLOR","DISABLE_SHADOW_CASTING","DISABLE_SHADOW_RECEIVING","SHADOW_COLOR_MAIN_DIR","DISABLE_ADDITIONAL_LIGHTS","TEMPLATE_LWRP","NO_RAMP_UNLIT"];flags:list[];flags_extra:dict[];keywords:dict[RENDER_TYPE="Opaque",RampTextureDrawer="[TCP2Gradient]",RampTextureLabel="Ramp Texture",SHADER_TARGET="3.0",GPU_INSTANCING_MAX_COUNT_VALUE="50"];shaderProperties:list[,sp(name:"Main Color";imps:list[imp_mp_color(def:RGBA(1, 1, 1, 1);hdr:False;cc:4;chan:"RGBA";prop:"_Color";md:"";gbv:False;custom:False;refs:"";pnlock:False;guid:"5c97d97c-03b8-40ad-b3d7-d360fdd8e2af";op:Multiply;lbl:"Color";gpu_inst:False;dots_inst:False;locked:False;impl_index:0)];layers:list[];unlocked:list[];layer_blend:dict[];custom_blend:dict[];clones:dict[];isClone:False)];customTextures:list[];codeInjection:codeInjection(injectedFiles:list[injectedFile(guid:"3fca7c847ce8b7d41aa1016d217af658";filename:"DMC_Injection";injectedPoints:list[injectedPoint(name:"Properties/Start";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Properties";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Include Files";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Include";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Variables/Outside CBuffer";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Variables";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Main Pass/Tags";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Main Pass Tags";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Main Pass/Pragma";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Main Pass Pragma";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Main Pass/Varyings";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Main Pass Varyings";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Main Pass/Fragment Shader/Start";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Fragment Shader Start";program:Fragment;shaderProperties:list[]),injectedPoint(name:"Main Pass/Fragment Shader/End";enabled:True;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Fragment Shader End";program:Fragment;shaderProperties:list[]),injectedPoint(name:"Depth + Shadow Caster Pass/Varyings";enabled:False;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Shadow Caster Pass Varyings";program:Undefined;shaderProperties:list[]),injectedPoint(name:"Depth + Shadow Caster Pass/Vertex Shader/Start";enabled:False;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Shadow Caster Pass Vertex Shader Start";program:Vertex;shaderProperties:list[]),injectedPoint(name:"Depth + Shadow Caster Pass/Fragment Shader/Start";enabled:False;replace:False;displayName:__NULL__;blockName:"DMC Dissolve Shadow Caster Pass Fragment Shader Start";program:Fragment;shaderProperties:list[])])];mark:True);matLayers:list[]) */
/* TCP_HASH a7530e26afe9e70ae1d79c012aaa864e */
