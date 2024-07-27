using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Data
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private AudioSource _musicAudioSource;
        
        private AudioClip _musicAudioClip;

        private AudioSource _tickAudioSource;

        private AudioClip _tickAudioClip;

        public bool IsPlayingMusic;

        protected override void Awake()
        {
            base.Awake();
            
            _tickAudioSource = gameObject.AddComponent<AudioSource>();
            
            _musicAudioSource ??= gameObject.AddComponent<AudioSource>();

            _musicAudioSource.playOnAwake = false;
        }
        
        public void PlayTick()
        {
            _tickAudioSource.PlayOneShot(_tickAudioClip);
        }

        public AudioManager SetMusic(AudioClip audioClip)
        {
            _musicAudioClip = audioClip;
            
            return this;
        }

        public bool LoadAudio() => _musicAudioClip != null && _musicAudioClip.LoadAudioData();

        public void PlayMusic()
        {
            if (_musicAudioClip == null || _musicAudioSource == null) return;
            
            _musicAudioSource.clip = _musicAudioClip; 
            
            _musicAudioSource.Play();

            IsPlayingMusic = true;
        }

        private void OnApplicationQuit()
        {
            if (Instance != null)
                Destroy(gameObject);
        }
    }
}