using CompassNavigatorPro;
using PurrNet;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.Client.Logic.Player
{
    public class PlayerTag : NetworkBehaviour
    {
        [SerializeField] private CompassProPOI _compassProPOI;
        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            if (!asServer)
                return;
           
            UpdatePoiVisibility();
        }
        

       [ObserversRpc(runLocally: true)]
        private void UpdatePoiVisibility()
        {
            // Только локальный клиент решает, показывать ли этот POI
            if (isOwner)
            {
                // Я — владелец: НЕ показываю свой POI
                _compassProPOI.enabled = false;
            }
            else
            {
                int id = Random.Range(0, 1000000);
                _compassProPOI.id = id;
                // Это чужой игрок — показываю его POI
                _compassProPOI.enabled = true;
            }
        }
    }
}
