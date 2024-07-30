using System;
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
        
        private Vector3 previousPos;

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
            previousPos = transform.position;
        }

        private void Update()
        {
            if (_isCalculated) return;
    
            if (_judgeLine == null) return;
    
            float deltaTime = Time.deltaTime;
            Vector3 currentPos = transform.position;
            float currentYPos = currentPos.y - GamePlay.ScrollSpeed * deltaTime;

            // 이동 거리 계산
            float distance = Mathf.Abs(_judgeLine.position.y - currentYPos);

            if (distance > 0.35f)
            {
                // 현재 프레임의 판정이 아닌 경우 보간 판정을 시도
                int steps = 5; // 체크할 중간 점의 수
                for (int i = 1; i <= steps; i++)
                {
                    float t = (float)i / steps;
                    Vector3 interpolatedPos = Vector3.Lerp(previousPos, currentPos, t);
                    float interpolatedYPos = interpolatedPos.y - GamePlay.ScrollSpeed * deltaTime;
                    float interpolatedDistance = Mathf.Abs(_judgeLine.position.y - interpolatedYPos);

                    if (interpolatedDistance < 0.18f)
                    {
                        GamePlay.Instance.UpdateCombo();
                        _isCalculated = true;
                        break;
                    }
                }

                if (_isCalculated) return; // 보간 판정이 성공하면 함수 종료
            }
            else if (distance < 0.18f)
            {
                GamePlay.Instance.UpdateCombo();
                _isCalculated = true;
            }

            // 현재 위치를 이전 위치로 업데이트
            previousPos = currentPos;
        }
    }
}