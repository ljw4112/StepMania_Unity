using UnityEngine;

namespace Runtime.Data
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private AudioSource[] _tickAudioSource;

        private AudioClip _tickAudioClip;

        private const int AudioSourceLength = 10;

        protected override void Awake()
        {
            base.Awake();

            _tickAudioSource = new AudioSource[AudioSourceLength];

            for (int i = 0; i < AudioSourceLength; i++)
            {
                _tickAudioSource[i] = gameObject.AddComponent<AudioSource>();
            }

            _tickAudioClip ??= Resources.Load<AudioClip>("Audio/assist_tick");
        }

        public void PlayTick()
        {
            for (int i = 0; i < AudioSourceLength; i++)
            {
                if (_tickAudioSource[i].isPlaying) continue;
                
                _tickAudioSource[i].PlayOneShot(_tickAudioClip);
            }
        }
    }
}