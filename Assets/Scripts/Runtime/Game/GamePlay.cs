using System;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Data;
using Runtime.Data.Factory;
using Runtime.Game.Input;
using UnityEngine;
using Utils;

namespace Runtime.Game
{
    public class GamePlay : MonoBehaviour
    {
        public static float ScrollSpeed;

        public const int OffsetHeight = 4;

        [SerializeField] private UI.UIRoot UiRoot;
        
        [SerializeField] private NoteMaker _noteMaker;
        
        [SerializeField] private Transform noteParent;

        private Simfile _simfile;

        private ReactiveProperty<double> _timer = new();

        public static readonly ReactiveProperty<int> _combo = new();

        private Simfile _curSimfile;

        private ReactiveProperty<float> _curBpm = new();

        private void Start()
        {
            Application.targetFrameRate = 144;
            
            _combo.Where(x => x > 0).Subscribe(x =>
            {
                UiRoot.textCombo.SetText(x.ToString());
            });

            _timer.Subscribe(time =>
            {
                UiRoot.textTimer.SetText(time.ToString("F3"));
            });

            _curBpm.Subscribe(bpm =>
            {
                UiRoot.textBpm.SetText(bpm.ToString());
            });

            InputSystem.Instance.SetInputAction(KeyCode.F5, StartSong);
        }

        private void StartSong()
        {
            _combo.Value = 0;
            
            // 임시로
            _curSimfile = FileLoader.FileLoad("Shugoku no Medley Chozetsugikou BosoKumiKyoku");

            noteParent.transform.position = new Vector3(0, OffsetHeight, 0);
            
            UiRoot.textTitle.SetText(_curSimfile.Title);
            
            UiRoot.textArtist.SetText(_curSimfile.Artist);

            _noteMaker.SetSimfile(_curSimfile, Difficulty.Challenge).InstantiateNote();

            _curSimfile.ConvertData();

            _timer.Value = _curSimfile.Offset - OffsetHeight;
            
            ScrollSpeed = -_noteMaker.GetScrollSpeed(0) * Time.deltaTime;

            AudioManager.Instance.SetMusic(_curSimfile.MusicAudioClip);
            
            Play().Forget();
        }
        
        private async UniTaskVoid Play()
        {
            // 노트가 다 만들어지고 Audio파일이 모두 로딩될 때 까지 대기
            await UniTask.WaitUntil(() => _noteMaker.IsNoteCreated && AudioManager.Instance.LoadAudio());

            _curBpm.Value = _curSimfile.BPM.Dequeue().bpm;

            while (true)
            {
                if (_timer.Value >= 0 && !AudioManager.Instance.IsPlayingMusic)
                {
                    AudioManager.Instance.PlayMusic();
                }

                if (_curSimfile.BPM.Count > 0)
                {
                    int time = _curSimfile.BPM.Peek().seconds;

                    int integerTimer = (int)(_timer.Value * 1000);
                
                    if (Math.Abs(integerTimer - time) < 5)
                    {
                        _curBpm.Value = _curSimfile.BPM.Dequeue().bpm;
                    }
                }
            
                if (_timer.Value >= - _curSimfile.Offset - OffsetHeight * Time.deltaTime)
                {
                    noteParent.Translate(0, -_noteMaker.GetScrollSpeed(_curBpm.Value) * Time.deltaTime, 0);
                }

                _timer.Value += Time.deltaTime;
                
                await UniTask.Yield();
            }
        }
    }
}