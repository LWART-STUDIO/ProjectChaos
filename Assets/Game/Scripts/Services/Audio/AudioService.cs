using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Services.Pool;
using PurrNet;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Services.Audio
{
    public class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioListener _audioListener;
        [SerializeField] private AudioSource _musicA;
        [SerializeField] private AudioSource _musicB;
        [SerializeField] private List<AudioData> _audioDataSFX;
        [SerializeField] private List<AudioData> _audioDataMusic;
        [SerializeField] private GameObject _audioSourcePrefab;
        [SerializeField] private float _musicFadeDuration = 1.5f;
        private  ObjectPool<AudioPlayInPlays> _audioPool;
        private List<AudioPlayInPlays> _currentAudios = new List<AudioPlayInPlays>();
        public static AudioService instance = null;
        private AudioSource _currentMusic;
        private AudioSource _nextMusic;
        private Coroutine _musicFadeCoroutine;
        private string _currentMusicName;


        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            _musicA.loop = true;
            _musicB.loop = true;
            
            _musicA.playOnAwake = false;
            _musicB.playOnAwake = false;
            
            _currentMusic = _musicA;
            _nextMusic = _musicB;
            
            _audioPool = new ObjectPool<AudioPlayInPlays>(
                _audioSourcePrefab,OnSoundStartPlay,
                OnSoundEndPlay);

        }

        public void PlayMusicCrossfade(string musicName)
        {
            if (_currentMusicName == musicName)
                return; // уже играет

            AudioData audioData = SelectDataMusic(musicName);
            if (audioData == null || audioData.clips.Count == 0)
                return;

            _currentMusicName = musicName;

            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            _musicFadeCoroutine = StartCoroutine(
                CrossfadeMusic(audioData)
            );
        }
        private IEnumerator CrossfadeMusic(AudioData newMusic)
        {
            // --- Настройка нового трека ---
            int semitone = 0;
            if (newMusic.semitoneScale != null && newMusic.semitoneScale.Length > 0)
            {
                semitone = newMusic.semitoneScale[
                    Random.Range(0, newMusic.semitoneScale.Length)
                ];
            }

            _nextMusic.clip = newMusic.clips[
                Random.Range(0, newMusic.clips.Count)
            ];
            _nextMusic.pitch = PitchFromSemitones(newMusic.basePitch, semitone);
            _nextMusic.volume = 0f;
            _nextMusic.Play();

            float time = 0f;
            float startVolume = _currentMusic.volume;
            float targetVolume = newMusic.volume;

            // --- Сам crossfade ---
            while (time < _musicFadeDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / _musicFadeDuration;

                _currentMusic.volume = Mathf.Lerp(startVolume, 0.2f, t);
                _nextMusic.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            // --- Финал ---
            _currentMusic.Stop();
            _currentMusic.volume = 0f;
            _nextMusic.volume = targetVolume;

            // меняем местами
            (_currentMusic, _nextMusic) = (_nextMusic, _currentMusic);
        }
        public void StopMusic(float fadeTime = 1f)
        {
            if (_musicFadeCoroutine != null)
                StopCoroutine(_musicFadeCoroutine);

            StartCoroutine(FadeOutMusic(fadeTime));
        }
        private IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = _currentMusic.volume;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                _currentMusic.volume = Mathf.Lerp(startVolume, 0f, time / duration);
                yield return null;
            }

            _currentMusic.Stop();
        }



        public void PlaySoundInPlace(string name,
            Vector3 position)
        {
            
            AudioData audioData = SelectDataSFX(name);
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
            Debug.Log($"Sound was played{name}"); 
            float lifetime = audioObject.AudioSource.clip.length / Mathf.Abs(audioObject.AudioSource.pitch);
            audioObject.SetUp(name, lifetime);
        }
        

        public AudioData SelectDataSFX(string name)
        {
            return _audioDataSFX.Find(x => x.name == name);
          
        }
        public AudioData SelectDataMusic(string name)
        {
            return _audioDataMusic.Find(x => x.name == name);
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

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
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

        public void AttachAudioListenerToObject(Transform objectToAttach)
        {
            _audioListener.transform.SetParent(objectToAttach);
            _audioListener.transform.localPosition = Vector3.zero;
            _audioListener.transform.localRotation = Quaternion.identity;
        }

        public void DetachAudioListener()
        {
            _audioListener.transform.SetParent(this.transform);
            _audioListener.transform.localPosition = Vector3.zero;
            _audioListener.transform.localRotation = Quaternion.identity;
        }
    }
}
