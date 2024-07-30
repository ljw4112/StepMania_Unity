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

        float deltaTime = 0.0f;
        
        public static float ScrollSpeed;

        public const int OffsetHeight = 4;

        [SerializeField] private UI.UIRoot UiRoot;
        
        [SerializeField] private NoteMaker _noteMaker;
        
        [SerializeField] private Transform noteParent;

        private Simfile _simfile;

        private ReactiveProperty<double> _timer = new();

        public static readonly ReactiveProperty<int> _combo = new();

        private Simfile _curSimfile;

        public static readonly ReactiveProperty<float> CurrentBpm = new();

        private void Start()
        {
            Application.targetFrameRate = 60;
            
            QualitySettings.vSyncCount = 0;
            
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

            noteParent.transform.position = new Vector3(0, OffsetHeight, 0);
            
            UiRoot.textTitle.SetText(_curSimfile.Title);
            
            UiRoot.textArtist.SetText(_curSimfile.Artist);

            _noteMaker.SetSimfile(_curSimfile, _difficulty).InstantiateNote();

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
                
                if (_timer.Value >= -_curSimfile.Offset - OffsetHeight * Time.deltaTime)
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