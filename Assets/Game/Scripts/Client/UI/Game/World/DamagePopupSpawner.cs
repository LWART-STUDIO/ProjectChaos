using UnityEngine;

namespace Game.Scripts.Client.UI.Game.World
{
    public class DamagePopupSpawner : MonoBehaviour
    {
        public static DamagePopupSpawner Instance { get; private set; }

        [SerializeField] private DamagePopup _popupPrefab;

        private void Awake()
        {
            Instance = this;
        }

        public void Spawn(float damage, Vector3 position)
        {
            DamagePopup popup = Instantiate(_popupPrefab);
            popup.Init(damage, position);
        }
    }
}