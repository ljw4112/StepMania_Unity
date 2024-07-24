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

        private void Start()
        {
            var d = Disposable.CreateBuilder();
            
            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (_judgeLine == null) return;

                float scrolledYPos = transform.position.y - NoteMaker.ScrollSpeed;
                
                if (Mathf.Abs(_judgeLine.position.y - scrolledYPos) > 0.16f) return;
                
                AudioManager.Instance.PlayTick();
            }).AddTo(ref d);

            d.RegisterTo(destroyCancellationToken);
        }
    }
}