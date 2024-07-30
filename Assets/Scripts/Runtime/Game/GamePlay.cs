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
        [SerializeField] private string _playSongName;

        [SerializeField] private Difficulty _difficulty;
        
        [Range(1, 100)]
        public int fFont_Size;
        [Range(0, 1)]
        public float Red, Green, Blue;

        private float deltaTime = 0.0f;
        
        public static float ScrollSpeed;

        private float _offsetHeight;

        [SerializeField] private Camera _mainCamera;

        [SerializeField] private UI.UIRoot UiRoot;
        
        [SerializeField] private NoteMaker _noteMaker;
        
        [SerializeField] private Transform noteParent;

        [SerializeField] private Transform _judgeLine;

        private Simfile _simfile;

        private ReactiveProperty<double> _timer = new();

        public static readonly ReactiveProperty<int> _combo = new();

        private Simfile _curSimfile;

        public static readonly ReactiveProperty<float> CurrentBpm = new();

        [SerializeField] private float _height;

        private double _startTime, _requireTime;

        private void Start()
        {
            _combo.Where(x => x > 0).Subscribe(x =>
            {
                UiRoot.textCombo.SetText(x.ToString());
            });

            _timer.Subscribe(time =>
            {
                UiRoot.textTimer.SetText(time.ToString("F3"));
            });

            CurrentBpm.Subscribe(bpm =>
            {
                UiRoot.textBpm.SetText(bpm.ToString());
            });

            InputSystem.Instance.SetInputAction(KeyCode.F5, StartSong);
        }

        private void StartSong()
        {
            if (string.IsNullOrEmpty(_playSongName) || _difficulty == Difficulty.None)
            {
                Debug.LogError("Please check play song name or difficulty");
                return;
            }
            
            _combo.Value = 0;
            
            // 임시로
            _curSimfile = FileLoader.FileLoad(_playSongName);

            SetNoteParentPosition();
            
            UiRoot.textTitle.SetText(_curSimfile.Title);
            
            UiRoot.textArtist.SetText(_curSimfile.Artist);

            _noteMaker.SetSimfile(_curSimfile, _difficulty).InstantiateNote();

            _curSimfile.ConvertData();

            _timer.Value = GetStartTime() - 3;
            
            ScrollSpeed = -_noteMaker.GetScrollSpeed(0) * Time.deltaTime;

            AudioManager.Instance.SetMusic(_curSimfile.MusicAudioClip);
            
            Play().Forget();

            return;

            void SetNoteParentPosition()
            {
                // 카메라 뷰포트 좌표에서 (0.5, 1.0)은 화면의 상단 중앙을 의미합니다.
                Vector3 screenPosition = new Vector3(0.5f, 1.0f, _mainCamera.nearClipPlane);

                // 스크린 좌표를 월드 좌표로 변환합니다.
                Vector3 worldPosition = _mainCamera.ViewportToWorldPoint(screenPosition);

                // targetObject의 y 위치를 조정하여 카메라 영역 바로 위에 오도록 합니다.
                worldPosition.y += _height / 2;

                // targetObject의 위치를 설정합니다.
                noteParent.transform.position = worldPosition;

                _offsetHeight = noteParent.transform.position.y;
            }

            double GetStartTime()
            {
                double translateAmount = Math.Abs(_noteMaker.GetScrollSpeed(_curSimfile.BPM.Peek().bpm));
                double distance = Math.Abs(noteParent.transform.position.y - _judgeLine.position.y);
                _requireTime = distance / translateAmount;
                
                Debug.Log($"{distance} / {translateAmount} = {_requireTime}");

                _startTime = _curSimfile.Offset - _requireTime;
                
                return _startTime;
            }
        }
        
        private async UniTaskVoid Play()
        {
            // 노트가 다 만들어지고 Audio파일이 모두 로딩될 때 까지 대기
            await UniTask.WaitUntil(() => _noteMaker.IsNoteCreated && AudioManager.Instance.LoadAudio());

            CurrentBpm.Value = _curSimfile.BPM.Dequeue().bpm;

            bool isMultipleBPMChange = _curSimfile.BPM.Count > 1;

            while (true)
            {
                deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
                
                if (_timer.Value >= 0 && !AudioManager.Instance.IsPlayingMusic)
                {
                    AudioManager.Instance.PlayMusic();
                }

                if (_curSimfile.BPM.Count > 0 && isMultipleBPMChange)
                {
                    int time = _curSimfile.BPM.Peek().seconds;

                    int integerTimer = (int)(_timer.Value * 1000);

                    if (Math.Abs(integerTimer - time) < 5)
                    {
                        CurrentBpm.Value = _curSimfile.BPM.Dequeue().bpm;
                    }
                }
            
                float translateAmount = -_noteMaker.GetScrollSpeed(CurrentBpm.Value) * Time.deltaTime;
                
                if (_timer.Value >= -_curSimfile.Offset - _requireTime)
                {
                    noteParent.position += new Vector3(0, translateAmount, 0);
                }

                _timer.Value += Time.deltaTime;

                await UniTask.DelayFrame(1);
            }
        }
        
        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 0.02f);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / fFont_Size;
            style.normal.textColor = new Color(Red, Green, Blue, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = $"{msec:0.0} ms ({fps:0.} fps)";
            GUI.Label(rect, text, style);
        }
    }
}