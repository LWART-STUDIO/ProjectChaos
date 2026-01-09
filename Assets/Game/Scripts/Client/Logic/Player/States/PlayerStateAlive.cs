using PurrNet.StateMachine;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.States
{
    public class PlayerStateAlive : StateNode
    {
        [SerializeField] private GameObject _graphics;
        [SerializeField] private PlayerHealth _playerHealth;
        public override void Enter(bool asServer)
        {
            base.Enter(asServer);
            if(asServer)
                return;
           // _graphics?.SetActive(true);

           if (isOwner)
           {
               _playerHealth.onPlayerDieLocal += OnPlayerDie;
               _playerHealth.RestoreFullHealth();
           }
                
        }

        private void OnPlayerDie()
        {
            machine.Next();
        }

        public override void Exit(bool asServer)
        {
            base.Exit(asServer);
            if(asServer)
                return;
            _playerHealth.onPlayerDieLocal -= OnPlayerDie;
           // _graphics?.SetActive(false);
        }

    }
   
}
