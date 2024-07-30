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

        private ReactiveProperty<float> _longNoteComboInterval = new();

        private CancellationTokenSource _token;

        private bool _longNoteStart, _longNoteEnd;
        
        private Vector3 previousStartPos;
        
        private Vector3 previousEndPos;

        public LongNote SetPosition(Transform tr, Vector3 start, Vector3 end, float width = 0.5f)
        {
            if (_renderer == null) return null;

            _renderer.SetPositions(new[] { start, end });

            trJudgeLine = tr;

            _start = start;

            _end = end;
            
            trStart.transform.localPosition = _start;

            trEnd.transform.localPosition = _end;

            GamePlay.CurrentBpm.Subscribe(bpm =>
            {
                _longNoteComboInterval.Value = 60 / GamePlay.CurrentBpm.Value / 4;
            });

            return this;
        }

        private void Start()
        {
            previousStartPos = trStart.transform.position;
            
            previousEndPos = trEnd.transform.position;
        }

        private void Update()
        {
            if (trJudgeLine == null) return;
    
            float deltaTime = Time.deltaTime; // 프레임 간 경과 시간
    
            // 현재 위치 계산
            Vector3 currentStartPos = trStart.transform.position;
            Vector3 currentEndPos = trEnd.transform.position;

            // 이동 거리 계산
            float startYPos = currentStartPos.y - GamePlay.ScrollSpeed * deltaTime;
            float endYPos = currentEndPos.y - GamePlay.ScrollSpeed * deltaTime;

            // 보간하여 중간 위치 계산
            Vector3 interpolatedStartPos = Vector3.Lerp(previousStartPos, currentStartPos, 0.5f);
            Vector3 interpolatedEndPos = Vector3.Lerp(previousEndPos, currentEndPos, 0.5f);

            // 판정
            if ((Math.Abs(startYPos - trJudgeLine.transform.position.y) < 0.16f || 
                 Math.Abs(interpolatedStartPos.y - trJudgeLine.transform.position.y) < 0.16f) && !_longNoteStart)
            {
                _bStartCalculate = true;
    
                _token = new CancellationTokenSource();
        
                CalculateLongNoteCombo().Forget();
    
                _longNoteStart = true;
            }
    
            if ((Math.Abs(endYPos - trJudgeLine.transform.position.y) < 0.16f || 
                 Math.Abs(interpolatedEndPos.y - trJudgeLine.transform.position.y) < 0.16f) && !_longNoteEnd)
            {
                _bStartCalculate = false;
        
                _token?.Cancel();
    
                _longNoteEnd = true;
            }

            // 현재 위치를 이전 위치로 업데이트
            previousStartPos = currentStartPos;
            previousEndPos = currentEndPos;
        }


        private async UniTaskVoid CalculateLongNoteCombo()
        {
            try
            {
                while (_bStartCalculate && !_token.IsCancellationRequested)
                {
                    GamePlay.Instance.UpdateCombo();
                    
                    await UniTask.WaitForSeconds(_longNoteComboInterval.Value, cancellationToken: _token.Token);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}