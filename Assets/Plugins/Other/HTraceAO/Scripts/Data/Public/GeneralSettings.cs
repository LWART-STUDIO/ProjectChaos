//pipelinedefine
#define H_URP

using System;
using HTraceAO.Scripts.Data.Private;
using HTraceAO.Scripts.Extensions;
using HTraceAO.Scripts.Globals;
using UnityEngine;

namespace HTraceAO.Scripts.Data.Public
{
	[Serializable]
	public class GeneralSettings
	{
		
		[SerializeField]
		public HBuffer HBuffer = HBuffer.Multi;
		
		[SerializeField]
		private AmbientOcclusionMode _ambientOcclusionMode = AmbientOcclusionMode.GTAO;
		
		public AmbientOcclusionMode AmbientOcclusionMode
		{
			get
			{
				if ((HSettings.RTAOSettings.AlphaCutout == AlphaCutout.Evaluate && HRenderer.SupportsRayTracing == false) ||
				    (HSettings.RTAOSettings.AlphaCutout == AlphaCutout.DepthTest && HRenderer.SupportsInlineRayTracing == false))
				{
					switch (_ambientOcclusionMode)
					{
						case AmbientOcclusionMode.SSAO:
						case AmbientOcclusionMode.GTAO:
							return _ambientOcclusionMode;
						case AmbientOcclusionMode.RTAO:
							return AmbientOcclusionMode.GTAO;
					}	
				}

				return _ambientOcclusionMode;
			}
			set { _ambientOcclusionMode = value; }
		}

		[SerializeField]
		private float _intensity = 1f;
		/// <summary>
		/// 
		/// </summary>
		/// <value>[0.1;4.0]</value>
		[HExtensions.HRangeAttribute(0.1f,4.0f)]
		public float Intensity
		{
			get => _intensity;    
			set
			{
				if (Mathf.Abs(value - _intensity) < Mathf.Epsilon)
					return;

				_intensity = HExtensions.Clamp(value, typeof(GeneralSettings), nameof(GeneralSettings.Intensity));
			}
		}
		
		[SerializeField]
		private float _directLightOcclusion = 0f;
		/// <summary>
		/// 
		/// </summary>
		/// <value>[0.0;1.0]</value>
		[HExtensions.HRangeAttribute(0.0f,1.0f)]
		public float DirectLightOcclusion
		{
			get => _directLightOcclusion;    
			set
			{
				if (Mathf.Abs(value - _directLightOcclusion) < Mathf.Epsilon)
					return;

				_directLightOcclusion = HExtensions.Clamp(value, typeof(GeneralSettings), nameof(GeneralSettings.DirectLightOcclusion));
			}
		}
	}
}
