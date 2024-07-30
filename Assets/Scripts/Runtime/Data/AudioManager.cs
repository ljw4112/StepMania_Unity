using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
            
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
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

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (Instance != null)
                {
                    DestroyImmediate(gameObject);
                }
            }
        }
#endif
    }
}