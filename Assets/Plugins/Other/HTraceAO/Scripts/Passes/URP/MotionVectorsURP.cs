//pipelinedefine
#define H_URP

using System;
using HTraceAO.Scripts.Data.Private;
using HTraceAO.Scripts.Extensions;
using HTraceAO.Scripts.Globals;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_2023_3_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#else
using UnityEngine.Experimental.Rendering.RenderGraphModule;
#endif


namespace HTraceAO.Scripts.Passes.URP
{
	internal class MotionVectorsURP : ScriptableRenderPass
	{
		// Texture names
		const string _ObjectMotionVectorsColorURP    = "_ObjectMotionVectorsColorURP";
		const string _ObjectMotionVectorsDepthURP    = "_ObjectMotionVectorsDepthURP";
		const string _CustomCameraMotionVectorsURP_0 = "_CustomCameraMotionVectorsURP_0";
		const string _CustomCameraMotionVectorsURP_1 = "_CustomCameraMotionVectorsURP_1";
		
		private static readonly int ObjectMotionVectorsColor      = Shader.PropertyToID("_ObjectMotionVectors");
		private static readonly int ObjectMotionVectorsDepth = Shader.PropertyToID("_ObjectMotionVectorsDepth");
		private static readonly int BiasOffset               = Shader.PropertyToID("_BiasOffset");

		// Textures
		internal static RTHandle[] CustomCameraMotionVectorsURP = new RTHandle[2];
		internal static RTHandle   ObjectMotionVectorsColorURP;
		internal static RTHandle   ObjectMotionVectorsDepthURP;
		
		// Materials
		private static Material CameraMotionVectorsMaterial_URP;

		
		#region --------------------------- Non Render Graph ---------------------------
		
		private        ScriptableRenderer _renderer;
		private static int _historyCameraIndex;

		protected internal void Initialize(ScriptableRenderer renderer)
		{
			_renderer    = renderer;
		}

#if UNITY_2023_3_OR_NEWER
		[System.Obsolete]
#endif
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			SetupShared(renderingData.cameraData.camera, renderingData.cameraData.renderScale, renderingData.cameraData.cameraTargetDescriptor);
		}
		
#if UNITY_2023_3_OR_NEWER
		[System.Obsolete]
#endif
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			if (HSettings.GeneralSettings.AmbientOcclusionMode == AmbientOcclusionMode.GTAO && HSettings.GTAOSettings.SampleCountTemporal > 1
			    || HSettings.GeneralSettings.AmbientOcclusionMode == AmbientOcclusionMode.RTAO && HSettings.RTAOSettings.SampleCountTemporal > 1)
				ConfigureInput(ScriptableRenderPassInput.Motion);

		}

#if UNITY_2023_3_OR_NEWER
		[System.Obsolete]
#endif
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get(HNames.HTRACE_MV_PASS_NAME);
			
			Camera camera = renderingData.cameraData.camera;
			

			RenderMotionVectorsNonRenderGraph(cmd, camera, ref renderingData, ref context);
			
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}

		private void RenderMotionVectorsNonRenderGraph(CommandBuffer cmd, Camera camera, ref RenderingData renderingData, ref ScriptableRenderContext context)
		{
			void RenderObjectsMotionVectors(ref RenderingData renderingData, ref ScriptableRenderContext context)
			{
#if UNITY_2023_3_OR_NEWER
				if (camera.cameraType == CameraType.SceneView)
					return;
#endif

				CoreUtils.SetRenderTarget(cmd, ObjectMotionVectorsColorURP.rt, ClearFlag.All, Color.clear);
				CoreUtils.SetRenderTarget(cmd, ObjectMotionVectorsDepthURP.rt, ClearFlag.All, Color.clear);
#if UNITY_2023_1_OR_NEWER
				// We'll write not only to our own Color, but also to our own Depth target to use it later (in Camera MV) to compose per-object mv
				CoreUtils.SetRenderTarget(cmd, ObjectMotionVectorsColorURP.rt, ObjectMotionVectorsDepthURP.rt);
#else
			// Prior to 2023 camera motion vectors are rendered directly on objects, so we write to both motion mask and motion vectors via MRT
			RenderTargetIdentifier[] motionVectorsMRT = { CustomCameraMotionVectorsURP[0].rt, CustomCameraMotionVectorsURP[1].rt,};
			CoreUtils.SetRenderTarget(cmd, motionVectorsMRT, ObjectMotionVectorsDepthURP.rt);

#endif // UNITY_2023_1_OR_NEWER

				CullingResults cullingResults = renderingData.cullResults;

				ShaderTagId[] tags
#if UNITY_2023_1_OR_NEWER
					= {new ShaderTagId("MotionVectors")};
#else
				= {new ShaderTagId("Meta")};
				// If somethingis wrong with our custom shader we can always use the standard one instead
				// Material ObjectMotionVectorsMaterial = new Material(Shader.Find("Hidden/Universal Render Pipeline/ObjectMotionVectors"));
				Material ObjectMotionVectorsMaterial = new Material(Shader.Find($"Hidden/{HNames.ASSET_NAME}/ObjectMotionVectorsURP"));
#endif // UNITY_2023_1_OR_NEWER

				var renderList = new UnityEngine.Rendering.RendererUtils.RendererListDesc(tags, cullingResults, camera)
				{
					rendererConfiguration = PerObjectData.MotionVectors,
					renderQueueRange      = RenderQueueRange.opaque,
					sortingCriteria       = SortingCriteria.CommonOpaque,
					layerMask             = camera.cullingMask,
					overrideMaterial
#if UNITY_2023_1_OR_NEWER
						= null,
#else
					= ObjectMotionVectorsMaterial,
#endif //UNITY_2023_1_OR_NEWER
				};

				CoreUtils.DrawRendererList(context, cmd, context.CreateRendererList(renderList));

#if !UNITY_2023_1_OR_NEWER
			// Prior to 2023 camera motion vectors are rendered directly on objects, so we will finish mv calculation here and won't execute camera mv
			cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, CustomCameraMotionVectorsURP[0].rt);
			cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionMask, CustomCameraMotionVectorsURP[1].rt);
#endif // UNITY_2023_1_OR_NEWER
			}

			void RenderCameraMotionVectors()
			{
#if UNITY_2023_1_OR_NEWER

				float DepthBiasOffset = 0;
#if UNITY_2023_1_OR_NEWER
				DepthBiasOffset = 0.00099f;
#endif // UNITY_2023_1_OR_NEWER
#if UNITY_6000_0_OR_NEWER
				DepthBiasOffset = 0;
#endif // UNITY_6000_0_OR_NEWER

				// Target target[0] is set as a Depth Buffer, just because this method requires Depth, but we don't care for it in the fullscreen pass
				RenderTargetIdentifier[] motionVectorsMRT = { CustomCameraMotionVectorsURP[0], CustomCameraMotionVectorsURP[1]};
				CoreUtils.SetRenderTarget(cmd, motionVectorsMRT, motionVectorsMRT[0]);

				CameraMotionVectorsMaterial_URP.SetTexture(ObjectMotionVectorsColor, ObjectMotionVectorsColorURP);
				CameraMotionVectorsMaterial_URP.SetTexture(ObjectMotionVectorsDepth, ObjectMotionVectorsDepthURP);
				CameraMotionVectorsMaterial_URP.SetFloat(BiasOffset, DepthBiasOffset);

				cmd.DrawProcedural(Matrix4x4.identity, CameraMotionVectorsMaterial_URP, 0, MeshTopology.Triangles, 3, 1);

				// This restores color camera color target (.SetRenderTarget can be used for Forward + any Depth Priming, but doesn't work in Deferred)
#pragma warning disable CS0618
				ConfigureTarget(_renderer.cameraColorTargetHandle);
#pragma warning restore CS0618

				cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, CustomCameraMotionVectorsURP[0]);
				cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionMask, CustomCameraMotionVectorsURP[1]);
#endif // UNITY_2023_1_OR_NEWER
			}

			RenderObjectsMotionVectors(ref renderingData, ref context);
			RenderCameraMotionVectors();
		}

		#endregion --------------------------- Non Render Graph ---------------------------

		#region --------------------------- Render Graph ---------------------------
		
#if UNITY_2023_3_OR_NEWER
		private class PassData
		{
			public RendererListHandle  RendererListHandle;
			public TextureHandle       ColorTexture;
			public TextureHandle       DepthTexture;
			public TextureHandle       MotionVectorsTexture;
			public UniversalCameraData UniversalCameraData;
			public TextureHandle[]     CustomCameraMotionVectors = new TextureHandle[2];
			public TextureHandle       ObjectMotionVectorsColor;
			public TextureHandle       ObjectMotionVectorsDepth;
		}
		
		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			using (var builder = renderGraph.AddUnsafePass<PassData>(HNames.HTRACE_MV_PASS_NAME, out var passData, new ProfilingSampler(HNames.HTRACE_MV_PASS_NAME)))
			{
				UniversalResourceData  resourceData           = frameData.Get<UniversalResourceData>();
				UniversalCameraData    universalCameraData    = frameData.Get<UniversalCameraData>();
				UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
				UniversalLightData     lightData              = frameData.Get<UniversalLightData>();

				if (HSettings.GeneralSettings.AmbientOcclusionMode == AmbientOcclusionMode.GTAO && HSettings.GTAOSettings.SampleCountTemporal > 1
				    || HSettings.GeneralSettings.AmbientOcclusionMode == AmbientOcclusionMode.RTAO && HSettings.RTAOSettings.SampleCountTemporal > 1)
					ConfigureInput(ScriptableRenderPassInput.Motion);
				
				builder.AllowGlobalStateModification(true);
				builder.AllowPassCulling(false);
				
				TextureHandle colorTexture = universalRenderingData.renderingMode == RenderingMode.Deferred ? resourceData.activeColorTexture : resourceData.cameraColor;
				TextureHandle depthTexture = universalRenderingData.renderingMode == RenderingMode.Deferred ? resourceData.activeDepthTexture : resourceData.cameraDepth;
				TextureHandle mvTexture    = resourceData.motionVectorColor;
				builder.UseTexture(colorTexture, AccessFlags.Read);
				builder.UseTexture(depthTexture, AccessFlags.Read);
				builder.UseTexture(mvTexture, AccessFlags.Read);

				passData.ColorTexture         = colorTexture;
				passData.DepthTexture         = depthTexture;
				passData.MotionVectorsTexture = mvTexture;
				passData.UniversalCameraData  = universalCameraData;

				AddRendererList(renderGraph, universalCameraData, universalRenderingData, lightData, passData, builder);

				SetupShared(universalCameraData.camera, universalCameraData.renderScale, universalCameraData.cameraTargetDescriptor);
				ExtensionsURP.UseTexture(builder, renderGraph, CustomCameraMotionVectorsURP[0], ref passData.CustomCameraMotionVectors[0], AccessFlags.ReadWrite);
				ExtensionsURP.UseTexture(builder, renderGraph, CustomCameraMotionVectorsURP[1], ref passData.CustomCameraMotionVectors[1], AccessFlags.ReadWrite);
				ExtensionsURP.UseTexture(builder, renderGraph, ObjectMotionVectorsColorURP, ref passData.ObjectMotionVectorsColor, AccessFlags.ReadWrite);
				ExtensionsURP.UseTexture(builder, renderGraph, ObjectMotionVectorsDepthURP, ref passData.ObjectMotionVectorsDepth, AccessFlags.ReadWrite);

				builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
			}
		}
		
		private static void AddRendererList(RenderGraph renderGraph, UniversalCameraData universalCameraData, UniversalRenderingData universalRenderingData, UniversalLightData lightData, PassData passData, IUnsafeRenderGraphBuilder builder)
		{
			SortingCriteria   sortFlags        = universalCameraData.defaultOpaqueSortFlags;
			RenderQueueRange  renderQueueRange = RenderQueueRange.opaque;
			FilteringSettings filterSettings   = new FilteringSettings(renderQueueRange, ~0);

			// Redraw only objects that have their LightMode tag set to UniversalForward 
			ShaderTagId shadersToOverride = new ShaderTagId("MotionVectors");

			// Create drawing settings
			DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, universalRenderingData, universalCameraData, lightData, sortFlags);
			drawSettings.perObjectData = PerObjectData.MotionVectors;
			
			// Add the override material to the drawing settings
			//drawSettings.overrideMaterial = materialToUse;

			// Create the list of objects to draw
			var rendererListParameters = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);

			// Convert the list to a list handle that the render graph system can use
			passData.RendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
                
			// Set the render target as the color and depth textures of the active camera texture
			builder.UseRendererList(passData.RendererListHandle);
		}
		
		private static void ExecutePass(PassData data, UnsafeGraphContext rgContext)
		{
			var cmd = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);

			Camera camera = data.UniversalCameraData.camera;
			
			RenderMotionVectorsRenderGraph(cmd, data);
		}

		private static void RenderMotionVectorsRenderGraph(CommandBuffer cmd, PassData data)
		{
			void RenderObjectMotionVectors()
			{
				// OBSOLETE?
				// if (ObjectMotionVectorsColorURP.rt.width != RTHandles.rtHandleProperties.currentViewportSize.x) // Switch from Scene view to Game view for RenderGraph only case
				// {
				// 	cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, data.MotionVectorsTexture);
				// 	cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionMask, ObjectMotionVectorsColorURP.rt);
				// 	return;
				// }

				cmd.SetRenderTarget(data.ObjectMotionVectorsColor, data.DepthTexture);
				cmd.ClearRenderTarget(false, true, Color.black);

				cmd.DrawRendererList(data.RendererListHandle);
				cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, data.MotionVectorsTexture);
				cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionMask, data.ObjectMotionVectorsColor);
			}

			void RenderCameraMotionVectors()
			{
				// Render Graph + Game View - no need to render camera mv, as they are already available to us in this combination
				if (data.UniversalCameraData.cameraType == CameraType.Game)
				{
					return;
				}

				float DepthBiasOffset = 0;

				// Target target[0] is set as a Depth Buffer, just because this method requires Depth, but we don't care for it in the fullscreen pass
				RenderTargetIdentifier[] motionVectorsMRT = { data.CustomCameraMotionVectors[0], data.CustomCameraMotionVectors[1],};
				CoreUtils.SetRenderTarget(cmd, motionVectorsMRT, motionVectorsMRT[0]);

				CameraMotionVectorsMaterial_URP.SetTexture(ObjectMotionVectorsColor, data.ObjectMotionVectorsColor);
				CameraMotionVectorsMaterial_URP.SetTexture(ObjectMotionVectorsDepth, data.ObjectMotionVectorsDepth);
				CameraMotionVectorsMaterial_URP.SetFloat(BiasOffset, DepthBiasOffset);

				cmd.DrawProcedural(Matrix4x4.identity, CameraMotionVectorsMaterial_URP, 0, MeshTopology.Triangles, 3, 1);

				// This restores color camera color target (.SetRenderTarget can be used for Forward + any Depth Priming, but doesn't work in Deferred)
				cmd.SetRenderTarget(data.ColorTexture);

				cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionVectors, data.CustomCameraMotionVectors[0]);
				cmd.SetGlobalTexture(HShaderParams.g_HTraceMotionMask, data.CustomCameraMotionVectors[1]);
			}

			RenderObjectMotionVectors();
			RenderCameraMotionVectors();
		}
#endif
		
		#endregion ---------------------------  Render Graph ---------------------------

		#region --------------------------- Share ---------------------------

		private static void SetupShared(Camera camera, float renderScale, RenderTextureDescriptor desc)
		{
			if (CameraMotionVectorsMaterial_URP == null) CameraMotionVectorsMaterial_URP = new Material(Shader.Find($"Hidden/{HNames.ASSET_NAME}/CameraMotionVectorsURP"));

			int width  = (int)(camera.scaledPixelWidth * renderScale);
			int height = (int)(camera.scaledPixelHeight * renderScale);

			if (desc.width != width || desc.height != height)
				desc = new RenderTextureDescriptor(width, height);

			desc.depthBufferBits    = 0; // Color and depth cannot be combined in RTHandles
			desc.stencilFormat      = GraphicsFormat.None;
			desc.depthStencilFormat = GraphicsFormat.None;
			desc.msaaSamples        = 1;
			desc.bindMS             = false;
			desc.enableRandomWrite  = true;

			RenderTextureDescriptor depthDesc = desc;
			//depthDesc.depthBufferBits = 32;
			depthDesc.graphicsFormat  = GraphicsFormat.R16_SFloat;

			ExtensionsURP.ReAllocateIfNeeded(_CustomCameraMotionVectorsURP_0, ref CustomCameraMotionVectorsURP[0], ref desc, graphicsFormat: GraphicsFormat.R16G16_SFloat);
			ExtensionsURP.ReAllocateIfNeeded(_CustomCameraMotionVectorsURP_1, ref CustomCameraMotionVectorsURP[1], ref desc, graphicsFormat: GraphicsFormat.R8_SNorm);
			ExtensionsURP.ReAllocateIfNeeded(_ObjectMotionVectorsColorURP, ref ObjectMotionVectorsColorURP, ref desc, graphicsFormat: GraphicsFormat.R16G16_SFloat);
			ExtensionsURP.ReAllocateIfNeeded(_ObjectMotionVectorsDepthURP, ref ObjectMotionVectorsDepthURP, ref depthDesc, graphicsFormat: GraphicsFormat.R16_SFloat);
		}

		protected internal void Dispose()
		{
			CustomCameraMotionVectorsURP[0]?.Release();
			CustomCameraMotionVectorsURP[1]?.Release();
			ObjectMotionVectorsColorURP?.Release();
			ObjectMotionVectorsDepthURP?.Release();
		}

		#endregion --------------------------- Share ---------------------------
	}
}
