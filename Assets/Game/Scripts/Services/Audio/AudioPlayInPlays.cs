using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Services.Pool;
using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public class AudioPlayInPlays : MonoBehaviour,IPoolable<AudioPlayInPlays>
    {
        [SerializeField] private AudioSource _audioSource;
       
        private Action<AudioPlayInPlays> returnToPool;
        private string _audioName;
        private float _lifeTime;
        private float _startTime;
        
        public string AudioName => _audioName;
        public AudioSource AudioSource => _audioSource;
        public float StartTime => _startTime;

        public void SetUp(string audioName, float lifeTime)
        {
            _audioName = audioName;
            _lifeTime = lifeTime;
            _startTime = Time.time;
            StartCoroutine(WaitAndReturnToPool());
        }
        public void Initialize(Action<AudioPlayInPlays> returnAction)
        {
            this.returnToPool = returnAction;
        }
        private void OnDisable()
        {
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            returnToPool?.Invoke(this);
        }

        private IEnumerator WaitAndReturnToPool()
        {
            yield return new WaitForSeconds(_lifeTime);
            gameObject.SetActive(false);
        }
    }
}
