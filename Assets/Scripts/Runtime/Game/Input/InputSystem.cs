using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Runtime.Game.Input
{
    
    /// <summary>
    /// 현재 인게임의 모든 Input을 관리하는 클래스
    /// </summary>
    public class InputSystem : MonoSingleton<InputSystem>
    {
        private Dictionary<KeyCode, Action> _keyActions = new();

        public InputSystem SetInputAction(KeyCode code, Action callback)
        {
            _keyActions[code] = callback;

            return this;
        }

        private void Start()
        {
            var d = Disposable.CreateBuilder();
        
            Observable.EveryUpdate()
                .Where(_ => _keyActions.Keys.Any(UnityEngine.Input.GetKeyDown))
                .Subscribe(_ =>
                {
                    foreach (var key in _keyActions.Keys.Where(UnityEngine.Input.GetKeyDown))
                    {
                        _keyActions[key]?.Invoke();
                    }
                }).AddTo(ref d);
        
            d.RegisterTo(this.GetCancellationTokenOnDestroy());
        }
    }
}