using System;
using System.Collections;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Client.UI.Game.PlayerUI;
using Game.Scripts.Services.UI;
using PurrNet;
using PurrNet.StateMachine;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Game
{
    public class GameStateRunning : StateNode
    {
       [SerializeField] private StateNode _lostState;
       [SerializeField] private StateNode _levelUpState;
       private float _elapsedTime = float.MaxValue;
       private PlayerInGameUI _playerInGameUI=>Service<UIService>.Instance.GetPlayerInGameUI();
       private Coroutine _timerCoroutine;
       
       public override void Enter(bool asServer){
            base.Enter(asServer);
            if(!asServer)
                return;
            if (Mathf.Approximately(_elapsedTime, float.MaxValue))
            {
                ResetTimer();
            }
            _timerCoroutine =StartCoroutine(ProcessTimer());
            PlayerHealth.onPlayerDie += OnPlayerDie;
            LevelManager.onLevelChanged += OnLevelChange;
       }
       [ObserversRpc(runLocally: true)]
       public void ResetTimer()
       {
           _elapsedTime = 0f;
       }

       private void OnLevelChange(int obj)
       {
           machine.SetState(_levelUpState);
       }

       public override void Exit(bool asServer){
           base.Exit(asServer);
           if(asServer)
               StopCoroutine(_timerCoroutine);
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

       [ObserversRpc(runLocally: true)]
       private void UpdatePlayerTimer()
       {
           _playerInGameUI.UpdateTimer(_elapsedTime);
           GameStatisticCollector.UpdateTime(_elapsedTime);
       }
        [ObserversRpc(runLocally: true)]
       private void UpdateTime()
       {
           _elapsedTime +=Time.deltaTime;
       }

       private IEnumerator ProcessTimer()
       {
           while (true)
           {
               UpdateTime();
               UpdatePlayerTimer();
               yield return null;
           }
          

       }
    }
}
