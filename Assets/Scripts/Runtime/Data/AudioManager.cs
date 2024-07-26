using UnityEngine;

namespace Runtime.Data
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private AudioSource _musicAudioSource;
        
        [SerializeField] private AudioClip _musicAudioClip;

        private AudioSource _tickAudioSource;

        private AudioClip _tickAudioClip;

        public bool IsPlayingMusic;

        protected override void Awake()
        {
            base.Awake();
            
            _tickAudioSource = gameObject.AddComponent<AudioSource>();
            
            _musicAudioSource ??= gameObject.AddComponent<AudioSource>();

            _musicAudioSource.playOnAwake = false;

            _musicAudioClip.LoadAudioData();

            _tickAudioClip ??= Resources.Load<AudioClip>("Audio/assist_tick");

            _tickAudioClip.LoadAudioData();
        }
        
        public void PlayTick()
        {
            _tickAudioSource.PlayOneShot(_tickAudioClip);
        }

        public void PlayMusic()
        {
            if (_musicAudioClip == null || _musicAudioSource == null) return;
            
            _musicAudioSource.clip = _musicAudioClip; 
            
            _musicAudioSource.Play();

            IsPlayingMusic = true;
        }

    }
}