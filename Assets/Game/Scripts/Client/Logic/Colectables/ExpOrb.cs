using System.Collections.Generic;
using Game.Scripts.Client.Logic.Game;
using Game.Scripts.Client.Logic.Player;
using Game.Scripts.Services;
using PurrNet;
using Sisus.Init;
using UnityEngine;

namespace Game.Scripts.Client.Logic.Colectables
{
    public class ExpOrb : NetworkBehaviour
    {
        [SerializeField] private  GameObject _expRender;
        [SerializeField] private Collider _expCollider;
        
        private int _exp = 0;

        public void SetUpExp(int exp)
        {
            _exp = exp;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_exp == 0)
                return;
            if (other.TryGetComponent(out PlayerHealth player))
            {
                
                AddExp();
                PlaySound();
            }
        }

        [ObserversRpc(runLocally: true)]
        private void AddExp()
        {
            _expCollider.enabled = false;
            if (InstanceHandler.TryGetInstance(out LevelManager levelManager))
                levelManager.AddExp(_exp);
            _expRender.SetActive(false);
            Destroy(gameObject);
        }

        private void PlaySound()
        {
            Service<ServiceInitor>.Instance.AudioService.PlaySoundInPlace("ExpOrb",transform.position,true);
        }
    }
}