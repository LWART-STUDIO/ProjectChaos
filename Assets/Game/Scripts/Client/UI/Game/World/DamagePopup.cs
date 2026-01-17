using Game.Scripts.Client.Logic.Player;
using PurrNet;
using TMPro;
using UnityEngine;

namespace Game.Scripts.Client.UI.Game.World
{
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _lifetime = 1f;
        [SerializeField] private float _floatSpeed = 1.5f;
        [SerializeField] private float _randomOffset = 0.3f;

        private Camera _camera;
        private float _timer;

        public void Init(float damage, Vector3 worldPosition)
        {
            _camera = PlayerCameraProvider.LocalCamera;
            _text.text = Mathf.Abs(damage).ToString("0");

            Vector3 random = new Vector3(
                Random.Range(-_randomOffset, _randomOffset),
                Random.Range(0f, _randomOffset),
                Random.Range(-_randomOffset, _randomOffset)
            );

            transform.position = worldPosition + random;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // движение вверх
            transform.position += Vector3.up * (_floatSpeed * Time.deltaTime);

            // поворот к камере
            if (_camera != null)
            {
                Vector3 dir = transform.position - _camera.transform.position;
                transform.rotation = Quaternion.LookRotation(dir);
            }

            if (_timer >= _lifetime)
                Destroy(gameObject);
        }
    }
}