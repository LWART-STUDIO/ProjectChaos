using Systems.Services.SceneService;
using UnityEngine;

namespace Game.Scripts.Services.Scene
{
    public class LoadingScreen : MonoBehaviour
    {
        private float _currentValue;
        [SerializeField] private SlicedFilledImage _image;

        public void SetCurrentProgress(float progress)
        {
            _currentValue = progress;
        }

        public void Update()
        {
            if(!gameObject.activeInHierarchy)
                return;
            _image.fillAmount = _currentValue / 100f;
        }
    }
}
