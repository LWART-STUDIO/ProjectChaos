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
       [SerializeField] private StateNode _passiveGrantState;
       private SyncVar<float> _elapsedTime = new SyncVar<float>(float.MaxValue);
       private PlayerInGameUI _playerInGameUI=>Service<UIService>.Instance.GetPlayerInGameUI();
       private Coroutine _timerCoroutine;
       
       public override void Enter(bool asServer){
            base.Enter(asServer);
            if(!asServer)
                return;
            if (Mathf.Approximately(_elapsedTime.value, float.MaxValue))
            {
                ResetTimer();
            }
            _timerCoroutine =StartCoroutine(ProcessTimer());
            PlayerHealth.onPlayerDie += OnPlayerDie;
            LevelManager.onLevelChanged += OnLevelChange;
            LevelManager.onPassiveGrant += OnPassiveGrant;
       }
       [ServerRpc(requireOwnership:false)]
       public void ResetTimer()
       {
           _elapsedTime.value = 0f;
       }

       private void OnLevelChange(int obj)
       {
           machine.SetState(_levelUpState);
       }
       private void OnPassiveGrant()
       {
           machine.SetState(_passiveGrantState);
       }

       public override void Exit(bool asServer){
           base.Exit(asServer);
           if(asServer)
               StopCoroutine(_timerCoroutine);
           PlayerHealth.onPlayerDie -= OnPlayerDie;
           LevelManager.onLevelChanged -= OnLevelChange;
           LevelManager.onPassiveGrant -= OnPassiveGrant;
       }

       private void OnDisable()
       {
           if(isServer)
               StopCoroutine(_timerCoroutine);
           PlayerHealth.onPlayerDie -= OnPlayerDie;
           LevelManager.onLevelChanged -= OnLevelChange;
           LevelManager.onPassiveGrant -= OnPassiveGrant;
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
          
           GameStatisticCollector.UpdateTime(_elapsedTime.value);
       }
       [ObserversRpc(runLocally: true)]
       private void UpdateTime()
       {
           
           _playerInGameUI.UpdateTimer(_elapsedTime);
       }
       

       private IEnumerator ProcessTimer()
       {
           while (true)
           {
               _elapsedTime.value +=Time.deltaTime;
               UpdateTime();
               UpdatePlayerTimer();
               yield return null;
           }
          

       }
    }
}
