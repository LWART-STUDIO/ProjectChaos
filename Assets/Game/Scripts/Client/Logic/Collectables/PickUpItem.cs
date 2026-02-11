using Game.Scripts.Client.Logic.Colectables;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services;
using Game.Scripts.Services.Audio;
using PurrNet;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Collectables
{
    public class PickUpItem : NetworkBehaviour
    {
        [SerializeField] private CollectableType _type;
        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            if (!asServer) return;
           
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                CollectObject(playerHealth.transform);
            }
        }
        [ServerRpc(requireOwnership: false)]
        private void CollectObject(Transform player)
        {
            switch (_type)
            {
                case CollectableType.Magnet:
                    ExpOrbManager.MagnetAllOrbsToPlayer(player);
                    break;
            }
            DestroyObject();
           
        }

        [ObserversRpc(runLocally: true)]
        public void DestroyObject()
        {
            switch (_type)
            {
                case CollectableType.Magnet:
                    AudioService.instance.PlaySoundInPlace("Magnet",transform.position);
                    break;
            }
           
           Destroy(gameObject);
        }
        [System.Serializable]
        public enum CollectableType
        {
            None = 0,
            Magnet = 1,
        }
    }
}
