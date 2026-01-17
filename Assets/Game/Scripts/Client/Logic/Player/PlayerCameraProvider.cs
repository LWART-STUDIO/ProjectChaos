using PurrNet;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player
{
    public class PlayerCameraProvider : NetworkBehaviour
    {
        [SerializeField] private Camera _camera;
        public static Camera LocalCamera { get; private set; }

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            if(!isOwner)
                return;
            LocalCamera=_camera;
        }
    }
}
