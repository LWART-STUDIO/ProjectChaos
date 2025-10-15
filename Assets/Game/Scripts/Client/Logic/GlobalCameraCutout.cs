using UnityEngine;

namespace Game.Scripts.Client.Logic
{
    [ExecuteAlways]
    public class GlobalCameraCutout : MonoBehaviour
    {
        public Camera targetCamera;
        public float cutoutRadius = 5f;

        void Update()
        {
            if (!targetCamera) targetCamera = Camera.main;
            if (!targetCamera) return;

            Shader.SetGlobalVector("_CameraPos", targetCamera.transform.position);
            Shader.SetGlobalFloat("_CutoutRadius", cutoutRadius);
        }
    }
}