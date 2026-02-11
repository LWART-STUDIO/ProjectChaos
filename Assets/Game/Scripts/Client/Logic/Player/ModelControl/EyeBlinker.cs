using System.Collections;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.ModelControl
{
    public class EyeBlinker : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [Header("Properties")]
        [SerializeField] private float _blinkInterval = 5.0f;
        [SerializeField] private float _blinkEyeCloseDuration = 0.06f;
        [SerializeField] private float _blinkEyeOpeningSeconds = 0.03f;
        [SerializeField] private float _blinkEyeClosingSeconds = 0.1f;
    
        private Coroutine _blinkingRoutine;
    
        private int blendShapeIndex;
    

        private void Awake()
        {
            blendShapeIndex = GetBlendShapeIndex("Fcl_EYE_Close");
        
        }

        private void OnEnable()
        {
            _blinkingRoutine = StartCoroutine(Blinking());
        }

        private void OnDisable()
        {
            StopCoroutine(_blinkingRoutine);
        }

        private int GetBlendShapeIndex(string blendShapeName)
        {
            Mesh mesh = _skinnedMeshRenderer.sharedMesh;
            blendShapeIndex = mesh.GetBlendShapeIndex(blendShapeName);
            return blendShapeIndex;
        }

        private IEnumerator Blinking()
        {
            while (true)
            {
                //Close
                yield return new WaitForSecondsRealtime(_blinkInterval);
                var value = 0f;
                var closeSpeed = 1f / _blinkEyeClosingSeconds;
                while (value < 1f)
                {
                    _skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value * 100);
                    value += Time.deltaTime * closeSpeed;
                    yield return null;
                }
                _skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, 100f);
            
                //Wait
                yield return new WaitForSecondsRealtime(_blinkEyeCloseDuration);
            
                //Open
                value = 1f;
                var openSpeed = 1f / _blinkEyeOpeningSeconds;
                while (value > 0)
                {
                    _skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, value * 100);
                    value -= Time.deltaTime * openSpeed;
                    yield return null;
                }
                _skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, 0);
            

            }
        }
        
    }
}
