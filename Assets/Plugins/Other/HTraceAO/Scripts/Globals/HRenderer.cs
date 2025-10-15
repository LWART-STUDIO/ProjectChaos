//pipelinedefine
#define H_URP

using System;
using System.Collections.Generic;
using System.Reflection;
using HTraceAO.Scripts.Data.Private;
using UnityEngine;
using UnityEngine.Rendering;
using HTraceAO.Scripts.Infrastructure.URP;
using UnityEditor;
using UnityEngine.Rendering.Universal;

namespace HTraceAO.Scripts.Globals
{
	public enum HRenderPipeline
	{
		None,
		BIRP,
		URP,
		HDRP
	}

	public static class HRenderer
	{
		static HRenderPipeline s_CurrentHRenderPipeline = HRenderPipeline.None;

		public static HRenderPipeline CurrentHRenderPipeline
		{
			get
			{
				if (s_CurrentHRenderPipeline == HRenderPipeline.None)
				{
					s_CurrentHRenderPipeline = GetRenderPipeline();
				}

				return s_CurrentHRenderPipeline;
			}
		}

		private static HRenderPipeline GetRenderPipeline()
		{
			if (GraphicsSettings.currentRenderPipeline)
			{
				if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
					return HRenderPipeline.HDRP;
				else
					return HRenderPipeline.URP;
			}

			return HRenderPipeline.BIRP;
		}

		public static bool SupportsInlineRayTracing
		{
			get
			{
#if UNITY_2023_1_OR_NEWER
				return SystemInfo.supportsInlineRayTracing;
#else
				return false;
#endif
			}
		}

		public static bool SupportsRayTracing
		{
			get
			{
#if UNITY_2023_1_OR_NEWER // TODO: revert this to 2019 when raytracing issue in 2022 is resolved
				if (SystemInfo.supportsRayTracing == false)
					return false;


				return true;
#else
				return false;
#endif
			}
		}

		public static bool RayTracingExecutionCheck
		{
			get
			{
				if (HSettings.GeneralSettings.AmbientOcclusionMode == AmbientOcclusionMode.RTAO)
				{
					if (HSettings.RTAOSettings.AlphaCutout == AlphaCutout.Evaluate)
					{
						return SupportsRayTracing;
					}
					else
					{
						return SupportsInlineRayTracing;
					}
				}

				return true;
			}
		}

		public static bool RenderGraphEnabled
		{
			get
			{
#if UNITY_2023_3_OR_NEWER
				return GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>().enableRenderCompatibilityMode == false;
#endif
				return false;
			}
		}


		public static int TextureXrSlices
		{
			get
			{
				if (Application.isPlaying == false)
					return 1;


				return 1;
			}
		}

		static RenderTexture emptyTexture;
		public static RenderTexture EmptyTexture
		{
			get
			{
				if (emptyTexture == null)
				{
					emptyTexture                   = new RenderTexture(1, 1, 0);
					emptyTexture.enableRandomWrite = true;
					emptyTexture.dimension         = TextureDimension.Tex2D;
					emptyTexture.format            = RenderTextureFormat.ARGBFloat;
					emptyTexture.Create();
				}

				return emptyTexture;
			}
		}
		
		
		private static Mesh _fullscreenTriangle;
		public static Mesh FullscreenTriangle
		{
			get
			{
				if (_fullscreenTriangle != null)
					return _fullscreenTriangle;

				_fullscreenTriangle = new Mesh { name = "Fullscreen Triangle" };

				// Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
				// this directly in the vertex shader using vertex ids :(
				_fullscreenTriangle.SetVertices(new List<Vector3>
				{
					new Vector3(-1f, -1f, 0f),
					new Vector3(-1f, 3f,  0f),
					new Vector3( 3f, -1f, 0f)
				});
				_fullscreenTriangle.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
				_fullscreenTriangle.UploadMeshData(false);

				return _fullscreenTriangle;
			}
		}
	}
}
