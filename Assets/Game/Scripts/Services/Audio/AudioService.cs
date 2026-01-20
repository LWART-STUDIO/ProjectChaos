using System.Collections.Generic;
using Game.Scripts.Services.Pool;
using PurrNet;
using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public class AudioService : NetworkBehaviour, IService
    {
        [SerializeField] private List<AudioData> _audioData;
        [SerializeField] private GameObject _audioSourcePrefab;
        private  ObjectPool<AudioPlayInPlays> _audioPool;
        private List<AudioPlayInPlays> _currentAudios = new List<AudioPlayInPlays>();

        public void LocalAwake()
        {
            _audioPool = new ObjectPool<AudioPlayInPlays>(
                _audioSourcePrefab,OnSoundStartPlay,
                OnSoundEndPlay);
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
            if(!CanPlaySound(audioData))
                return;
            int semitone = 0;
            if (audioData.semitoneScale != null && audioData.semitoneScale.Length > 0)
            {
                semitone = audioData.semitoneScale[
                    Random.Range(0, audioData.semitoneScale.Length)
                ];
            }
            AudioPlayInPlays audioObject = _audioPool.Pull(position, Quaternion.identity);
            audioObject.AudioSource.clip = audioData.clips[Random.Range(0, audioData.clips.Count)];
            audioObject.AudioSource.pitch = PitchFromSemitones(audioData.basePitch, semitone);
            audioObject.AudioSource.volume = audioData.volume;
            audioObject.AudioSource.Play();
            float lifetime = audioObject.AudioSource.clip.length / Mathf.Abs(audioObject.AudioSource.pitch);
            audioObject.SetUp(name, lifetime);
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySoundInPlaceObserver(string name,
            Vector3 position)
        {
            AudioData audioData = SelectData(name);
            if (audioData == null)
                return;
            if(!CanPlaySound(audioData))
                return;
            int semitone = 0;
            if (audioData.semitoneScale != null && audioData.semitoneScale.Length > 0)
            {
                semitone = audioData.semitoneScale[
                    Random.Range(0, audioData.semitoneScale.Length)
                ];
            }
            AudioPlayInPlays audioObject = _audioPool.Pull(position, Quaternion.identity);
            audioObject.AudioSource.clip = audioData.clips[Random.Range(0, audioData.clips.Count)];
            
            audioObject.AudioSource.pitch = PitchFromSemitones(audioData.basePitch, semitone);
            audioObject.AudioSource.volume = audioData.volume;
            audioObject.AudioSource.Play();
            float lifetime = audioObject.AudioSource.clip.length / Mathf.Abs(audioObject.AudioSource.pitch);
            audioObject.SetUp(name, lifetime);

        }

        public AudioData SelectData(string name)
        {
            return _audioData.Find(x => x.name == name);
        }
        public bool CanPlaySound(AudioData audioData)
        {
           AudioPlayInPlays lastSound = _currentAudios.FindLast(x => x.AudioName == audioData.name);
            if(lastSound==null||_currentAudios.Count == 0)
                return true;
            if(lastSound.StartTime>=audioData.soundsDelay)
                return true;
            return false;
        }

        private void OnSoundStartPlay(AudioPlayInPlays audioPlayInPlays)
        {
            _currentAudios.Add(audioPlayInPlays);
        }
        private void OnSoundEndPlay(AudioPlayInPlays audioPlayInPlays)
        {
            _currentAudios.Remove(audioPlayInPlays);
        }
        private float PitchFromSemitones(float basePitch, int semitones)
        {
     
       
            return basePitch * Mathf.Pow(1.059463f, semitones);
            // или Mathf.Pow(2f, semitones / 12f)
        }

        [System.Serializable]
        public class AudioData
        {
            public string name;
            public List<AudioClip> clips;
            public float volume=1f;
            public float basePitch=1f;
            public float soundsDelay=0f;
            public int[] semitoneScale = { 0 };

        }
    }
}
