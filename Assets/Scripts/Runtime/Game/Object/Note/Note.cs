using R3;
using Runtime.Game;
using Runtime.Game.Object.ObjectPool;
using UnityEngine;

namespace Runtime.Object.Note
{
    public class Note : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _noteRenderer;
        
        private Transform _judgeLine;

        private bool _canPlayTickSound;

        private bool _isCalculated;

        public Note SetJudgeLine(Transform judgeLine)
        {
            _judgeLine = judgeLine;

            return this;
        }

        public Note SetNoteColor(Color color)
        {
            _noteRenderer.color = color;
            
            return this;
        }

        public Note SetTickSound(bool bTick)
        {
            _canPlayTickSound = bTick;
            
            return this;
        }

        private void Start()
        {
            var d = Disposable.CreateBuilder();
            
            Observable.EveryUpdate().Subscribe(_ =>
            {
                // float negDistance = _judgeLine.position.y - currentYPos;
                //
                // if (negDistance > 1f)
                // {
                //     NotePool.Instance.Pool(this);
                //     
                //     return;
                // }
                
                if (_isCalculated) return;
                
                if (_judgeLine == null) return;
                
                float currentYPos = transform.position.y - GamePlay.ScrollSpeed;
                
                float distance = Mathf.Abs(_judgeLine.position.y - currentYPos);
                
                // 0.3까지는 처리안함
                if (distance > 0.3) return;

                if (distance < 0.16f)
                {
                    GamePlay._combo.Value += 1;
                    
                    _isCalculated = true;
                }

            }).AddTo(ref d);
            
            d.RegisterTo(destroyCancellationToken);
        }
    }
}