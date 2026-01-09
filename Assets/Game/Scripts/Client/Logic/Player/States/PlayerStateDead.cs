using System.Collections.Generic;
using PurrNet.StateMachine;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Player.States
{
    public class PlayerStateDead : StateNode
    {
        [SerializeField] private StateNode _aliveState;
        [SerializeField] private float _reviveDistance = 2f;
        [SerializeField] private float _reviveTime = 3f;
        [SerializeField] private GameObject _graphics;
        [SerializeField] private List<MonoBehaviour> _components;

        private float _reviveProgress;
        public override void Enter(bool asServer)
        {
            base.Enter(asServer);
            if(asServer)
                return;
            //_graphics?.SetActive(true);
            ToggleComponents(false);
            
        }

        public override void Exit(bool asServer)
        {
            base.Exit(asServer);
            if(asServer)
                return;
            //_graphics?.SetActive(false);
            ToggleComponents(true);
        }

        private void ToggleComponents(bool enabled)
        {
            if (!isOwner)
                return;
            foreach(var component in _components)
                component.enabled = enabled;
        }

        public override void StateUpdate(bool asServer)
        {
            base.StateUpdate(asServer);
            if(!isOwner || asServer)
                return;
            
            bool beingRevived = false;
            foreach (var player in PlayerHealth.AllPlayers.Values)
            {
                if(player.isOwner)
                    continue;

                if (Vector3.Distance(player.transform.position, transform.position) > _reviveDistance)
                    continue;
                beingRevived = true;
                _reviveProgress += Time.deltaTime;
                
            }
            if(!beingRevived)
                _reviveProgress = 0f;
            if (_reviveProgress >= _reviveTime) 
                machine.SetState(_aliveState);
        }
    }
}
