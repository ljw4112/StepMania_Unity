using Runtime.Metronome;
using UnityEngine;

namespace Runtime.Data
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private AudioSource _musicAudioSource;
        
        [SerializeField] private AudioClip _musicAudioClip;

        private AudioSource _tickAudioSource;

        private AudioClip _tickAudioClip;

        private const int AudioSourceLength = 5;

        protected override void Awake()
        {
            base.Awake();
            
            _tickAudioSource = gameObject.AddComponent<AudioSource>();
            
            _musicAudioSource ??= gameObject.AddComponent<AudioSource>();

            _musicAudioSource.playOnAwake = false;

            _musicAudioClip.LoadAudioData();

            _tickAudioClip ??= Resources.Load<AudioClip>("Audio/assist_tick");

            _tickAudioClip.LoadAudioData();
            
            Sync.Instance.Init(_musicAudioSource, _musicAudioClip);
        }
        
        public void PlayTick()
        {
            _tickAudioSource.PlayOneShot(_tickAudioClip);
        }

        public void PlayMusic()
        {
            _musicAudioSource.clip = _musicAudioClip; 
            
            _musicAudioSource.Play();
        }

    }
}