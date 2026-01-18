using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public class AudioService : NetworkBehaviour, IService
    {
        [SerializeField] private List<AudioData> _audioData;
        [SerializeField] private AudioSource _audioSourcePrefab;

        public void LocalAwake()
        {

        }

        public void LocalStart()
        {

        }

        public void LocalUpdate(float deltaTime)
        {

        }

        public void PlaySoundInPlace(string name,
            Vector3 position,
            bool asObserver)
        {
           
            if (asObserver)
            {
                PlaySoundInPlaceObserver(name, position);
                return;
            }
            AudioData audioData = SelectData(name);
            if (audioData == null)
                return;

            AudioSource audioSource = Instantiate(_audioSourcePrefab, position, Quaternion.identity);
            audioSource.clip = audioData.clips[Random.Range(0, audioData.clips.Count)];
            audioSource.pitch = Random.Range(audioData.pitchStart, audioData.pitchEnd);
            audioSource.volume = audioData.volume;
            audioSource.Play();
            float lifetime = audioSource.clip.length / Mathf.Abs(audioSource.pitch);
            Destroy(audioSource.gameObject, lifetime);
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySoundInPlaceObserver(string name,
            Vector3 position)
        {
            AudioData audioData = SelectData(name);
            if (audioData == null)
                return;

            AudioSource audioSource = Instantiate(_audioSourcePrefab, position, Quaternion.identity);
            audioSource.clip = audioData.clips[Random.Range(0, audioData.clips.Count)];
            audioSource.pitch = Random.Range(audioData.pitchStart, audioData.pitchEnd);
            audioSource.volume = audioData.volume;
            audioSource.Play();
            float lifetime = audioSource.clip.length / Mathf.Abs(audioSource.pitch);
            Destroy(audioSource.gameObject, lifetime);

        }

        public AudioData SelectData(string name)
        {
            return _audioData.Find(x => x.name == name);
        }

        [System.Serializable]
        public class AudioData
        {
            public string name;
            public List<AudioClip> clips;
            public float volume;
            public float pitchStart;
            public float pitchEnd;

        }
    }
}
