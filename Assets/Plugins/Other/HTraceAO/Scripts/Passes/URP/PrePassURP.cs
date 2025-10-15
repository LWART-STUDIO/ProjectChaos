//pipelinedefine
#define H_URP

using System;
using HTraceAO.Scripts.Data.Private;
using HTraceAO.Scripts.Extensions.CameraHistorySystem;
using HTraceAO.Scripts.Globals;
using HTraceAO.Scripts.Passes.Shared;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_2023_3_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#else
using UnityEngine.Experimental.Rendering.RenderGraphModule;
#endif


namespace HTraceAO.Scripts.Passes.URP
{
	internal class PrePassURP : ScriptableRenderPass
	{
		private static Vector4 s_HRenderScalePrevious = Vector4.one;

		private struct HistoryCameraData : ICameraHistoryData
		{
			private int hash;
			public Matrix4x4 previousViewProjMatrix;
			public Matrix4x4 previousInvViewProjMatrix;

			public int GetHash() => hash;
			public void SetHash(int hashIn) => this.hash = hashIn;
		}

		private static readonly CameraHistorySystem<HistoryCameraData> CameraHistorySystem = new CameraHistorySystem<HistoryCameraData>();
		private static int s_FrameCount = 0;

		#region --------------------------- Non Render Graph ---------------------------

		private        ScriptableRenderer _renderer;

		protected internal void Initialize(ScriptableRenderer renderer)
		{
			_renderer    = renderer;
		}

#if UNITY_2023_3_OR_NEWER
		[System.Obsolete]
#endif
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			Camera camera = renderingData.cameraData.camera;

			CameraHistorySystem.UpdateCameraHistoryIndex(camera.GetHashCode());
			CameraHistorySystem.UpdateCameraHistoryData();
		}

#if UNITY_2023_3_OR_NEWER
		[System.Obsolete]
#endif
		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
		}

#if UNITY_2023_3_OR_NEWER
		[System.Obsolete]
#endif
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var cmd = CommandBufferPool.Get(HNames.HTRACE_PRE_PASS_NAME);

			Camera camera = renderingData.cameraData.camera;

			cmd.SetGlobalVector(HShaderParams.AmbientOcclusionParam, new Vector4(1.0f, 0.0f, 0.0f, HSettings.GeneralSettings.DirectLightOcclusion));
			var cameraDepthTexture = Shader.GetGlobalTexture(HShaderParams.g_CameraDepthTexture);
			var cameraNormalsTexture = Shader.GetGlobalTexture(HShaderParams.g_CameraNormalsTexture);
			if (cameraDepthTexture == null)
				Shader.SetGlobalTexture(HShaderParams.g_CameraDepthTexture, HRenderer.EmptyTexture);
			if (cameraNormalsTexture == null)
				Shader.SetGlobalTexture(HShaderParams.g_CameraNormalsTexture, HRenderer.EmptyTexture);

			// -------------- HRenderScale -----------------
			// Unity's _RTHandleScale in URP always (1,1,1,1)? We need to overwrite it anyway...
			cmd.SetGlobalVector(HShaderParams.HRenderScalePrevious, s_HRenderScalePrevious);
			//s_HRenderScalePrevious = new Vector4(RTHandles.rtHandleProperties.rtHandleScale.x, RTHandles.rtHandleProperties.rtHandleScale.y, 1 / RTHandles.rtHandleProperties.rtHandleScale.x, 1 / RTHandles.rtHandleProperties.rtHandleScale.y); //we don't needed it more
			cmd.SetGlobalVector(HShaderParams.HRenderScale, s_HRenderScalePrevious);

			// -------------- Matrix -----------------

			Matrix4x4 projMatrix        = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true); //renderingData.cameraData.GetGPUProjectionMatrix();
			Matrix4x4 viewMatrix        = renderingData.cameraData.GetViewMatrix();
			Matrix4x4 viewProjMatrix    = projMatrix * viewMatrix;
			Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
			{
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_VP, viewProjMatrix);
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_I_VP, invViewProjMatrix);
				ref var previousViewProjMatrix = ref CameraHistorySystem.GetCameraData().previousViewProjMatrix;
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_PREV_VP, previousViewProjMatrix);
				ref var previousInvViewProjMatrix = ref CameraHistorySystem.GetCameraData().previousInvViewProjMatrix;
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_PREV_I_VP, previousInvViewProjMatrix);

				previousViewProjMatrix = viewProjMatrix;
				previousInvViewProjMatrix = invViewProjMatrix;
				CameraHistorySystem.GetCameraData().SetHash(camera.GetHashCode());

				// HistoryCameraData currentData = CameraHistorySystem.GetCameraData();
				// currentData.previousViewProjMatrix = viewProjMatrix;
				// currentData.previousInvViewProjMatrix = invViewProjMatrix;
				// CameraHistorySystem.SetCameraData(currentData);
			}


			// -------------- Other -----------------
			cmd.SetGlobalInt(HShaderParams.FrameCount, s_FrameCount);
			s_FrameCount++;
			//	Unity's blue noise is unreliable, so we'll use ours in all pipelines
			HBlueNoise.SetTextures(cmd);

			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}

		#endregion --------------------------- Non Render Graph ---------------------------

		#region --------------------------- Render Graph ---------------------------

#if UNITY_2023_3_OR_NEWER
		private class PassData
		{
			public RendererListHandle  RendererListHandle;
			public UniversalCameraData UniversalCameraData;
		}

		public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
		{
			using (var builder = renderGraph.AddUnsafePass<PassData>(HNames.HTRACE_PRE_PASS_NAME, out var passData, new ProfilingSampler(HNames.HTRACE_PRE_PASS_NAME)))
			{
				UniversalResourceData  resourceData           = frameData.Get<UniversalResourceData>();
				UniversalCameraData    universalCameraData    = frameData.Get<UniversalCameraData>();
				UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
				UniversalLightData     lightData              = frameData.Get<UniversalLightData>();

				ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);

				builder.AllowGlobalStateModification(true);
				builder.AllowPassCulling(false);

				passData.UniversalCameraData  = universalCameraData;

				Camera camera = universalCameraData.camera;

				CameraHistorySystem.UpdateCameraHistoryIndex(camera.GetHashCode());
				CameraHistorySystem.UpdateCameraHistoryData();

				builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
			}
		}

		private static void ExecutePass(PassData data, UnsafeGraphContext rgContext)
		{
			var cmd = CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd);

			Camera camera = data.UniversalCameraData.camera;

			cmd.SetGlobalVector(HShaderParams.AmbientOcclusionParam, new Vector4(1.0f, 0.0f, 0.0f, HSettings.GeneralSettings.DirectLightOcclusion));

			var cameraDepthTexture = Shader.GetGlobalTexture(HShaderParams.g_CameraDepthTexture);
			var cameraNormalsTexture = Shader.GetGlobalTexture(HShaderParams.g_CameraNormalsTexture);
			if (cameraDepthTexture == null)
				Shader.SetGlobalTexture(HShaderParams.g_CameraDepthTexture, HRenderer.EmptyTexture);
			if (cameraNormalsTexture == null)
				Shader.SetGlobalTexture(HShaderParams.g_CameraNormalsTexture, HRenderer.EmptyTexture);


			// -------------- HRenderScale -----------------
			// Unity's _RTHandleScale in URP always (1,1,1,1)? We need to overwrite it anyway...
			cmd.SetGlobalVector(HShaderParams.HRenderScalePrevious, s_HRenderScalePrevious);
			//s_HRenderScalePrevious = new Vector4(RTHandles.rtHandleProperties.rtHandleScale.x, RTHandles.rtHandleProperties.rtHandleScale.y, 1 / RTHandles.rtHandleProperties.rtHandleScale.x, 1 / RTHandles.rtHandleProperties.rtHandleScale.y); //we don't needed it more
			cmd.SetGlobalVector(HShaderParams.HRenderScale, s_HRenderScalePrevious);

			// -------------- Matrix -----------------

			Matrix4x4 projMatrix        = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true); //renderingData.cameraData.GetGPUProjectionMatrix();
			Matrix4x4 viewMatrix        = data.UniversalCameraData.GetViewMatrix();
			Matrix4x4 viewProjMatrix    = projMatrix * viewMatrix;
			Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
			{
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_VP, viewProjMatrix);
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_I_VP, invViewProjMatrix);
				ref var previousViewProjMatrix = ref CameraHistorySystem.GetCameraData().previousViewProjMatrix;
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_PREV_VP, previousViewProjMatrix);
				ref var previousInvViewProjMatrix = ref CameraHistorySystem.GetCameraData().previousInvViewProjMatrix;
				cmd.SetGlobalMatrix(HShaderParams.H_MATRIX_PREV_I_VP, previousInvViewProjMatrix);

				previousViewProjMatrix = viewProjMatrix;
				previousInvViewProjMatrix = invViewProjMatrix;
				CameraHistorySystem.GetCameraData().SetHash(camera.GetHashCode());

				// HistoryCameraData currentData = CameraHistorySystem.GetCameraData();
				// currentData.previousViewProjMatrix = viewProjMatrix;
				// currentData.previousInvViewProjMatrix = invViewProjMatrix;
				// CameraHistorySystem.SetCameraData(currentData);
			}

			// -------------- Other -----------------
			cmd.SetGlobalInt(HShaderParams.FrameCount, s_FrameCount);
			s_FrameCount++;
			//	Unity's blue noise is unreliable, so we'll use ours in all pipelines
			HBlueNoise.SetTextures(cmd);
		}

#endif

		#endregion ---------------------------  Render Graph ---------------------------

		protected internal void Dispose()
		{
			s_FrameCount = 0;
		}
	}
}
