using Cysharp.Threading.Tasks;
using R3;
using Runtime.Data;
using Runtime.Data.Factory;
using UnityEngine;
using Utils;

namespace Runtime.Game
{
    public class GamePlay : MonoBehaviour
    {
        public static float ScrollSpeed;

        public const int OffsetHeight = 4;

        [SerializeField] private UI.UIRoot UiRoot;
        
        [SerializeField] private NoteMaker _noteMaker;
        
        [SerializeField] private Transform noteParent;

        private Simfile _simfile;

        private ReactiveProperty<double> _timer = new();

        public static readonly ReactiveProperty<int> _combo = new();
        
        public static Simfile CurrentSimfile { get; private set; }

        private void Start()
        {
            var d = Disposable.CreateBuilder();
            
            Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.F5)).Subscribe(_ =>
                {
                    StartSong();
                }).AddTo(ref d);
            
            d.RegisterTo(destroyCancellationToken);;

            _combo.Where(x => x > 0).Subscribe(x =>
            {
                UiRoot.textCombo.SetText(x.ToString());
            });

            _timer.Subscribe(time =>
            {
                UiRoot.textTimer.SetText(time.ToString("F3"));
            });
        }

        private void StartSong()
        {
            _combo.Value = 0;
            
            // 임시로
            CurrentSimfile = FileLoader.FileLoad("Jounetsu Fun Fanfare");

            noteParent.transform.position = new Vector3(0, OffsetHeight, 0);
            
            UiRoot.textTitle.SetText(CurrentSimfile.TitleTranslit);
            
            UiRoot.textArtist.SetText(CurrentSimfile.ArtistTranslit);

            _noteMaker.SetSimfile(CurrentSimfile, Difficulty.Challenge).InstantiateNote();

            _timer.Value = CurrentSimfile.Offset - OffsetHeight;
            
            ScrollSpeed = -_noteMaker.ScrollSpeed * Time.deltaTime;
            
            Play().Forget();
        }
        
        private async UniTaskVoid Play()
        {
            await UniTask.WaitUntil(() => _noteMaker.IsNoteCreated);
            
            while (true)
            {
                if (_timer.Value >= 0 && !AudioManager.Instance.IsPlayingMusic)
                {
                    AudioManager.Instance.PlayMusic();
                }
            
                if (_timer.Value >= -CurrentSimfile.Offset)
                {
                    noteParent.Translate(0, -_noteMaker.ScrollSpeed * Time.deltaTime, 0);
                }

                _timer.Value += Time.deltaTime;
                
                await UniTask.Yield();
            }
        }
    }
}