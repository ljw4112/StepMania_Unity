using System;
using R3;
using Runtime.Data;
using Runtime.Loader;
using UnityEngine;

namespace Runtime.Object.Note
{
    public class Note : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _noteRenderer;
        
        private Transform _judgeLine;

        private bool _canPlayTickSound;

        private bool _playTickSound;

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
            if (!_canPlayTickSound) return;
            
            var d = Disposable.CreateBuilder();
            
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (_playTickSound) return;
                
                if (_judgeLine == null) return;
            
                float scrolledYPos = transform.position.y - NoteMaker.ScrollSpeed;
                
                if (Mathf.Abs(_judgeLine.position.y - scrolledYPos) > 0.5f) return;
                
                AudioManager.Instance.PlayTick();

                _playTickSound = true;

            }).AddTo(ref d);
            
            d.RegisterTo(destroyCancellationToken);
        }
    }
}