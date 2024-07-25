using System;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Data;
using Runtime.Loader;
using UnityEngine;
using Utils;

namespace Runtime.Game
{
    public class GamePlay : MonoBehaviour
    {
        [SerializeField] private NoteMaker _noteMaker;
        
        [SerializeField] private Transform noteParent;

        private Simfile _simfile;

        private double _timer;

        private void Start()
        {
            
            
            var d = Disposable.CreateBuilder();
            
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.F5))
                .ThrottleFirst(TimeSpan.FromSeconds(5)).Subscribe(_ =>
                {
                    StartSong();
                }).AddTo(ref d);
            
            d.RegisterTo(destroyCancellationToken);;
        }

        private void StartSong()
        {
            // 임시로
            _simfile = FileLoader.FileLoad("Jounetsu Fun Fanfare");

            _noteMaker.SetSimfile(_simfile, Difficulty.Challenge).InstantiateNote();

            _timer = _simfile.Offset;
            
            Play().Forget();
        }
        
        private async UniTaskVoid Play()
        {
            await UniTask.WaitUntil(() => _noteMaker.IsNoteCreated);
            
            while (true)
            {
                if (_timer >= 0 && !AudioManager.Instance.IsPlayingMusic)
                {
                    AudioManager.Instance.PlayMusic();
                }
            
                if (_timer >= -_simfile.Offset)
                {
                    noteParent.Translate(0, -_noteMaker.ScrollSpeed * Time.deltaTime, 0);
                }

                _timer += Time.deltaTime;
                
                await UniTask.Yield();
            }
        }
    }
}