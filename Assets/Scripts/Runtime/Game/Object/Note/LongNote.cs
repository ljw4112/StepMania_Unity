using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Runtime.Game;
using UnityEngine;

namespace Runtime.Object.Note
{
    public class LongNote : MonoBehaviour
    {
        [SerializeField] private LineRenderer _renderer;

        [SerializeField] private Transform trStart;
        
        [SerializeField] private Transform trEnd;

        private Transform trJudgeLine;

        private Vector3 _start, _end;

        private bool _bStartCalculate;

        private float _longNoteComboInterval;

        private CancellationTokenSource _token;

        private bool _longNoteStart, _longNoteEnd;

        public LongNote SetPosition(Transform tr, Vector3 start, Vector3 end, float width = 0.5f)
        {
            if (_renderer == null) return null;

            _renderer.SetPositions(new[] { start, end });

            trJudgeLine = tr;

            _start = start;

            _end = end;
            
            trStart.transform.localPosition = _start;

            trEnd.transform.localPosition = _end;

            _longNoteComboInterval = 60 / 180f / 4;

            return this;
        }

        private void Start()
        {
            var d = Disposable.CreateBuilder();

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (trJudgeLine == null) return;
                
                float startYPos = trStart.transform.position.y - GamePlay.ScrollSpeed;
                
                float endYPos = trEnd.transform.position.y - GamePlay.ScrollSpeed;

                if (Math.Abs(startYPos - trJudgeLine.transform.position.y) < 0.16f && !_longNoteStart)
                {
                    _bStartCalculate = true;

                    _token = new CancellationTokenSource();
                    
                    CalculateLongNoteCombo().Forget();

                    _longNoteStart = true;
                }

                if (Math.Abs(endYPos - trJudgeLine.transform.position.y) < 0.16f && !_longNoteEnd)
                {
                    _bStartCalculate = false;
                    
                    _token.Cancel();

                    _longNoteEnd = true;
                }

            }).AddTo(ref d);

            d.RegisterTo(destroyCancellationToken);
        }

        private async UniTaskVoid CalculateLongNoteCombo()
        {
            try
            {
                while (_bStartCalculate && !_token.IsCancellationRequested)
                {
                    GamePlay._combo.Value += 1;
                    
                    await UniTask.WaitForSeconds(_longNoteComboInterval, cancellationToken: _token.Token);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}