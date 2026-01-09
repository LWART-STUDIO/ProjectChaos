using Game.Scripts.Client.Logic.Player;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateRunning : StateNode
    {
       [SerializeField] private StateNode _lostState;
       [SerializeField] private StateNode _levelUpState;
       
       public override void Enter(bool asServer){
            base.Enter(asServer);
            if(!asServer)
                return;
            PlayerHealth.onPlayerDie += OnPlayerDie;
            LevelManager.onLevelChanged += OnLevelChange;
       }

       private void OnLevelChange(int obj)
       {
           machine.SetState(_levelUpState);
       }

       public override void Exit(bool asServer){
           base.Exit(asServer);
           PlayerHealth.onPlayerDie -= OnPlayerDie;
           LevelManager.onLevelChanged -= OnLevelChange;
       }

       private void OnPlayerDie(PlayerID playerID)
       {
           foreach (var player in PlayerHealth.AllPlayers.Values)
               if(player.Health>0)
                   return;
           machine.SetState(_lostState);
       }
    }
}
