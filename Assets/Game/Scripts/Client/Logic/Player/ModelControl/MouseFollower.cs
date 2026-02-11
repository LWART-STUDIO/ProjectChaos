using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.ModelControl
{
    public class MouseFollower : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private Transform _planeOrigin;
        [SerializeField] private float _planeDistance = 1.5f;
        [SerializeField] private float _smooth = 10f;

        [Header("Screen clamp")]
        [SerializeField] private float _screenPadding = 20f; // отступ от краёв экрана

        private Plane plane;

        void Start()
        {
            if (!_cam)
                _cam = Camera.main;

            plane = new Plane(
                -_cam.transform.forward,
                _planeOrigin.position + _cam.transform.forward * _planeDistance
            );
        }

        void Update()
        {
            Vector3 mousePos = Input.mousePosition;

            // 🔒 Кламп по экрану
            mousePos.x = Mathf.Clamp(mousePos.x, _screenPadding, Screen.width - _screenPadding);
            mousePos.y = Mathf.Clamp(mousePos.y, _screenPadding, Screen.height - _screenPadding);

            Ray ray = _cam.ScreenPointToRay(mousePos);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 targetPos = ray.GetPoint(enter);

                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    Time.deltaTime * _smooth
                );
            }
        }
    }
}