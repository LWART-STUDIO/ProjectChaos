using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RadiantGI.Universal {

    [ExecuteInEditMode]
    public class RadiantShadowMap : MonoBehaviour {

        internal static Camera captureCameraRef;
        internal static RenderTexture colorsRef;
        internal static RenderTexture worldPosRef;
        internal static RenderTexture normalsRef;
        internal static Vector3 clipDirRef;

        internal static class ShaderParams {
            public static int RadiantShadowMapColors = Shader.PropertyToID("_RadiantShadowMapColors");
            public static int RadiantShadowMapNormals = Shader.PropertyToID("_RadiantShadowMapNormals");
            public static int RadiantShadowMapWorldPos = Shader.PropertyToID("_RadiantShadowMapWorldPos");
            public static int RadiantShadowMapDepth = Shader.PropertyToID("_RadiantShadowMapDepth");
            public static int RadiantWorldToShadowMap = Shader.PropertyToID("_RadiantWorldToShadowMap");
            public static int ClipToWorld = Shader.PropertyToID("_ClipToWorld");
            public static int ClipDir = Shader.PropertyToID("_ClipDir");
            public static int FarClipPlane = Shader.PropertyToID("_FarClipPlane");
        }

        public enum ShadowMapResolution {
            [InspectorName("64")]
            _64,
            [InspectorName("128")]
            _128,
            [InspectorName("256")]
            _256,
            [InspectorName("512")]
            _512,
            [InspectorName("1024")]
            _1024,
            [InspectorName("2048")]
            _2048
        }

        const string RADIANT_GO_NAME = "RadiantGI Capture Camera";

        public static bool installed;

        public Transform target;

        [Tooltip("The capture extents around target")]
        public float targetCaptureSize = 25;
        public ShadowMapResolution resolution = ShadowMapResolution._512;
        Light thisLight;
        public Camera captureCamera;
        Material captureMat;
        Quaternion lastRotation;
        Vector3 lastTargetPos;
        float lastCaptureSize;
        [NonSerialized]
        public RenderTexture rtColors, rtWorldPos, rtNormals;

        bool needShoot;

        void OnEnable () {
            thisLight = GetComponent<Light>();
            if (thisLight == null || thisLight.type != LightType.Directional) {
                Debug.LogError("Radiant Shadow Map script must be added to a directional light!");
                return;
            }
            SetupCamera();
            lastTargetPos = new Vector3(float.MaxValue, 0, 0);
            installed = true;
        }

        private void OnValidate () {
            targetCaptureSize = Mathf.Max(targetCaptureSize, 5);
        }

        private void OnDestroy () {
            Remove();
        }

        private void Remove () {
            installed = false;
            if (captureCamera != null && RADIANT_GO_NAME.Equals(captureCamera.name)) {
                DestroyImmediate(captureCamera.gameObject);
            }
            DestroyRT(rtColors);
            DestroyRT(rtWorldPos);
            DestroyRT(rtNormals);
        }

        void SetupCamera () {
            if (captureCamera == null) {
                captureCamera = GetComponentInChildren<Camera>();
                if (captureCamera == null) {
                    GameObject camGO = Instantiate(Resources.Load<GameObject>("RadiantGI/CaptureCamera"));
                    camGO.name = RADIANT_GO_NAME;
                    camGO.transform.SetParent(transform, false);
                    captureCamera = camGO.GetComponent<Camera>();
                }
            }
            captureCamera.forceIntoRenderTexture = true;

            var urpData = captureCamera.GetComponent<UniversalAdditionalCameraData>();
            if (urpData == null) urpData = captureCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            urpData.requiresDepthTexture = true;
            urpData.renderPostProcessing = false;
            urpData.renderShadows = false;
            urpData.requiresColorTexture = false;
            urpData.allowXRRendering = false;
            AssignCaptureRenderer(urpData);
        }

        bool CheckCompatiblePipelineArchitecture () {
            UniversalRenderPipelineAsset pipe = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            if (pipe == null) return false;
            bool isCompatible = true;
#if UNITY_2022_2_OR_NEWER
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12) {
                for (int i = 0; i < pipe.m_RendererDataList.Length; i++) {
                    UniversalRendererData rendererData = pipe.m_RendererDataList[i] as UniversalRendererData;
                    if (rendererData != null) {
                        if (rendererData.renderingMode == RenderingMode.ForwardPlus) { isCompatible = false; break; }
#if UNITY_6000_1_OR_NEWER
                        else if (rendererData.renderingMode == RenderingMode.DeferredPlus) { isCompatible = false; break; }
#endif
                    }
                }
            }
#endif
            return isCompatible;
        }

        const string CAPTURE_RENDERER_RES_PATH = "RadiantGI/CaptureCameraRenderer";
        UniversalRendererData captureRendererData;
        void AssignCaptureRenderer (UniversalAdditionalCameraData camData) {
            UniversalRenderPipelineAsset pipe = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            if (pipe == null) return;

            bool skipCustomRenderer = !CheckCompatiblePipelineArchitecture();
            if (skipCustomRenderer) {
                camData.SetRenderer(-1);
                return;
            }

            if (captureRendererData == null) {
                captureRendererData = Resources.Load<UniversalRendererData>(CAPTURE_RENDERER_RES_PATH);
                if (captureRendererData == null) {
                    Debug.LogError("Radiant Capture renderer asset not found at Resources/" + CAPTURE_RENDERER_RES_PATH);
                    camData.SetRenderer(-1);
                    return;
                }
            }

            int rendererIndex = -1;
            for (int k = 0; k < pipe.m_RendererDataList.Length; k++) {
                if (pipe.m_RendererDataList[k] == captureRendererData) { rendererIndex = k; break; }
            }
            if (rendererIndex < 0) {
                rendererIndex = pipe.m_RendererDataList.Length;
                System.Array.Resize<ScriptableRendererData>(ref pipe.m_RendererDataList, rendererIndex + 1);
                pipe.m_RendererDataList[rendererIndex] = captureRendererData;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(pipe);
#endif
            }
            camData.SetRenderer(rendererIndex);
        }

        private void LateUpdate () {
            if (thisLight == null) {
                Remove();
                return;
            }

            if (target == null) {
                target = FindTarget();
                if (target == null) return;
            }

            if (captureCamera == null) {
                SetupCamera();
                if (captureCamera == null) return;
            }

            Quaternion rotation = transform.rotation;
            if (lastCaptureSize != targetCaptureSize || lastRotation != rotation || (lastTargetPos - target.position).sqrMagnitude > 25) {
                needShoot = true;
            }

            int desiredSize = 1 << ((int)resolution + 6);
            if (rtColors == null || rtNormals == null || rtWorldPos == null || rtColors.width != desiredSize) {
                DestroyRT(rtColors);
                DestroyRT(rtNormals);
                DestroyRT(rtWorldPos);
                if (rtColors == null) {
                    RenderTextureDescriptor rtDesc = new RenderTextureDescriptor(desiredSize, desiredSize, RenderTextureFormat.ARGBHalf, 24);
                    rtDesc.msaaSamples = 1;
                    rtDesc.useMipMap = false;
                    // create rsm color target
                    rtColors = new RenderTexture(rtDesc);
                    rtColors.Create();
                    // create rsm normals target
                    rtDesc.depthBufferBits = 0;
                    rtNormals = new RenderTexture(rtDesc);
                    rtNormals.Create();
                    // create rsm world pos target
                    rtWorldPos = new RenderTexture(rtDesc);
                    rtWorldPos.Create();
                }
                // Let URP manage targets via SubmitRenderRequest; keep targetTexture null
                captureCamera.targetTexture = null;
                needShoot = true;
            }

            if (needShoot) {
                // Make sure indirect light intensity > 0
                VolumeStack volume = VolumeManager.instance.stack;
                if (volume == null) return;
                RadiantGlobalIllumination radiant = volume.GetComponent<RadiantGlobalIllumination>();
                if (radiant == null || radiant.indirectIntensity.value <= 0) return;

                // Make sure the directional light is illuminating the scene
                if (thisLight.intensity <= 0 || thisLight.transform.forward.y > 0) {
                    Shader.SetGlobalTexture(ShaderParams.RadiantShadowMapColors, Texture2D.blackTexture);
                    return;
                }

                needShoot = false;

                lastRotation = transform.rotation;
                lastTargetPos = target.position;
                lastCaptureSize = targetCaptureSize;
                float farClipPlane = captureCamera.farClipPlane;
                Vector3 targetPosition = target != null ? target.transform.position : Vector3.zero;
                captureCamera.transform.localRotation = Quaternion.identity;
                captureCamera.transform.position = targetPosition - transform.forward * (farClipPlane * 0.5f);
                captureCamera.orthographicSize = targetCaptureSize;
                captureCameraRef = captureCamera;
                colorsRef = rtColors;
                worldPosRef = rtWorldPos;
                normalsRef = rtNormals;
                clipDirRef = transform.forward;

                RenderToTextureWithURP(captureCamera, rtColors);

                Shader.SetGlobalMatrix(ShaderParams.RadiantWorldToShadowMap, captureCamera.projectionMatrix * captureCamera.worldToCameraMatrix);
            }
        }



        void RenderToTextureWithURP (Camera cam, RenderTexture destination) {
            var request = new UniversalRenderPipeline.SingleCameraRequest {
                destination = destination,
                mipLevel = 0,
                face = CubemapFace.Unknown,
                slice = 0
            };
            var prev = cam.targetTexture;
            cam.targetTexture = destination;
            if (RenderPipeline.SupportsRenderRequest(cam, request)) {
                RenderPipeline.SubmitRenderRequest(cam, request);
            }
            else {
                cam.Render();
            }
            cam.targetTexture = prev;
        }

        Transform FindTarget () {
            Camera cam = Camera.main;
            if (cam != null) return cam.transform;
            return null;
        }

        void DestroyRT (RenderTexture rt) {
            if (rt == null) return;
            rt.Release();
            DestroyImmediate(rt);
        }

    }

}
