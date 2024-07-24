using Cysharp.Threading.Tasks;
using Runtime.Data;
using UnityEngine;
using Utils;

namespace Runtime.Metronome
{
    public class Sync : MonoSingleton<Sync>
    {
        private Simfile _simfile;

        private const double stdBPM = 60;
        
        private double _offsetForSample, _oneBeatTime, _nextSample, _bitPerSec, _bitPerSample, _barPerSec;

        private AudioClip _clip;

        private AudioSource _source;
        
        public bool SongStart { get; set; }
        
        public void Init(AudioSource source, AudioClip clip)
        {
            _source = source;

            _clip = clip;

            _simfile = FileLoader.FileLoad("Tsukitourou");

            _offsetForSample = clip.frequency * _simfile.Offset;

            _oneBeatTime = (stdBPM / 191);

            _nextSample = _oneBeatTime * clip.frequency - _offsetForSample;

            _bitPerSec = stdBPM / (8 * 191);

            _bitPerSample = _bitPerSec * clip.frequency - _offsetForSample;
            
            _barPerSec = _oneBeatTime * 4;
        }

        private void Update()
        {
            if (SongStart && _source.timeSamples >= _nextSample)
            {
                //PlayTick().Forget();
            }
        }

        private async UniTaskVoid PlayTick()
        {
            AudioManager.Instance.PlayTick();

            double beatPerSample = _oneBeatTime + _clip.frequency;

            _nextSample += beatPerSample;

            await UniTask.Yield();
        }
    }
}