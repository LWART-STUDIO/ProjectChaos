using Game.Scripts.Client.Logic.Player;
using PurrNet;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Client.Logic.Enemy
{
    public class EnemyHealthBar : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _target;        // Ссылка на врага
        [SerializeField] private Image _fillImage;         // Заполняемая часть полоски
        [SerializeField] private Canvas _canvas;          // Canvas World Space

        [Header("Offset")]
        [SerializeField] private Vector3 _offset = new Vector3(0, 2f, 0); // над головой врага

        private Camera _mainCamera;
        [SerializeField] private EnemyHealth _enemyHealth;

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            if(PlayerHealth.AllPlayers.ContainsKey(networkManager.localPlayer))
                _mainCamera = PlayerHealth.AllPlayers[networkManager.localPlayer].transform.GetComponentInChildren<Camera>();
        }
        

        private void LateUpdate()
        {
            if (_target == null || _fillImage == null || _enemyHealth == null||_mainCamera == null)
                return;

            // 1️⃣ позиция над врагом
            transform.position = _target.position + _offset;

            // 2️⃣ поворачиваем к камере
            Vector3 dir = transform.position - _mainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);

            // 3️⃣ обновляем полоску
            _fillImage.fillAmount = Mathf.Clamp01(_enemyHealth.Health / _enemyHealth.MaxHealth);
        }

  
    }
}