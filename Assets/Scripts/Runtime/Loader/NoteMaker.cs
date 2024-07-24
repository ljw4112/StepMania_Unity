using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Runtime.Data;
using Runtime.Object.Note;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Loader
{
    public class NoteMaker : MonoBehaviour
    {
        [SerializeField] private Text _text;
        
        public static float ScrollSpeed;

        private Simfile _simfile;

        private Difficulty _difficulty;

        [SerializeField] private List<float> noteXPos = new();

        [SerializeField] private Transform trNoteParent;

        [SerializeField] private Transform trJudgeLine;

        private List<GameObject> _noteList = new();

        private float _speed = 25;

        private float _scrollSpeed;

        private const float _offsetY = 3;

        private float _bpmRatio;

        private float _lineDistance;

        private (float?, float?) _lineCompare;

        [SerializeField] private double _timer;

        private bool _bStart;

        public NoteMaker SetSimfile(Simfile simfile, Difficulty difficulty)
        {
            _simfile = simfile;

            _difficulty = difficulty;

            _bpmRatio = 60 / 180f;

            return this;
        }

        public void InstantiateNote()
        {
            _noteList.Clear();

            if (_simfile == null) return;

            foreach (var measure in _simfile.NoteDatas[_difficulty].NoteInMeasure)
            {
                int beatCount = measure.Value.Count;

                // beatCount : 이 마디에 최소 박자단위가 몇인지 (ex. 4 => 4박만으로 이루어져있다)
                // _bpmRatio : 60기준을 1로 했을 때 bpm 비율, 60보다 빨라질수록 해당값은 작아진다.
                // minYSpace : 이 마디안에서 노트 하나와 하나 사이의 간격
                float minYSpace = 1f / beatCount;

                List<int> beatContains = new();
                for (int i = 4; i <= beatCount; i++)
                {
                    if (beatCount % i == 0 && i != 6) beatContains.Add(i);
                }

                for (int i = 0; i < beatCount; i++)
                {
                    string line = measure.Value[i];

                    if (line == "00000") continue;

                    // 현재 라인에 노트 색상 지정
                    Color noteColor = CheckCurLineColor(beatContains, i);

                    bool bPlayTick = true;

                    for (int j = 0; j < 5; j++)
                    {
                        // 일단 기본노트(1)이 아니면 넘어감
                        if (line[j] - '0' != 1) continue;

                        // 현재 노트의 좌표 계산
                        float yPos = (measure.Key + minYSpace * i);

                        // 좌표를 Vector3로 저장, 저장할 때는 현재 배속을 곱해줘서 노트 사이의 간격을 조절
                        Vector3 position = new Vector3(noteXPos[j], yPos * _speed, 0);

                        //=== 오브젝트 생성
                        var obj = Resources.Load<GameObject>("Prefab/Note");

                        var cubeObj = Instantiate(obj, position, Quaternion.identity, trNoteParent);

                        if (cubeObj.TryGetComponent<Note>(out var note))
                        {
                            note.SetJudgeLine(trJudgeLine);

                            note.SetNoteColor(noteColor);

                            note.SetTickSound(bPlayTick);
                        }

                        note.gameObject.name = $"Note_{measure.Key}_{i}";

                        _noteList.Add(cubeObj);
                        //=== 오브젝트 생성 끝

                        // 같은 라인의 노트는 한 노트만 Tick을 재생하도록
                        bPlayTick = false;
                    }
                }

                // 마디선 생성
                var lineObj = Instantiate(Resources.Load<GameObject>("Prefab/Line"), trNoteParent);

                if (lineObj.transform.TryGetComponent<Object.Line.Line>(out var lineComponent))
                {
                    lineComponent.SetLineNum(measure.Key);
                }

                // 마디선의 Y좌표 계산, 마디선은 해당 마디의 첫번째 노트와 좌표가 똑같아야 된다.
                float lineY = measure.Key * _speed;

                // 좌표 사이의 거리를 계산해서 60bpm일 때 1을 기준으로 속도를 계산하기 위해 데이터 저장
                if (measure.Key == 0 && _lineCompare.Item1 == null) _lineCompare.Item1 = lineY;
                else if (measure.Key == 1 && _lineCompare.Item2 == null) _lineCompare.Item2 = lineY;

                // 위에서 계산된 좌표 삽입
                lineObj.transform.position = new Vector3(0, lineY, 0);
            }

            // 마디선과 마디선 사이의 간격
            _lineDistance = _lineCompare.Item2.Value - _lineCompare.Item1.Value;
            
            _scrollSpeed = _lineDistance / (_bpmRatio * 4);
        }

        private bool bUpdate;

        public void Move()
        {
            bUpdate = true;
            
            _timer = _simfile.Offset;
        }
        
        private void Update()
        {
            if (!bUpdate) return;

            if (_timer >= 0 && !_bStart)
            {
                AudioManager.Instance.PlayMusic();
                
                _bStart = true;
            }
            
            if (_timer >= -_simfile.Offset)
            {
                trNoteParent.Translate(0, -_scrollSpeed * Time.deltaTime, 0);

                ScrollSpeed = -_scrollSpeed * Time.deltaTime;
            }

            _timer += Time.deltaTime;

            _text.text = _timer.ToString(CultureInfo.InvariantCulture);
        }

        private Color CheckCurLineColor(IEnumerable<int> beats, int line)
        {
            var pattern = Patterns(beats.Max());

            line = Mathf.Clamp(line % pattern.Count, 0, pattern.Count - 1);

            return NoteColor.BitColor[pattern[line]];

            List<int> Patterns(int maxBeat)
            {
                return maxBeat switch
                {
                    4 => new List<int> { 4 },
                    8 => new List<int> { 4, 8 },
                    12 => new List<int> { 4, 12, 12 },
                    16 => new List<int> { 4, 16, 8, 16 },
                    24 => new List<int> { 4, 24, 12, 8, 12, 24 },
                    32 => new List<int> { 4, 32, 16, 32, 8, 32, 16, 32 },
                    48 => new List<int> { 4, 48, 24, 16, 12, 48, 8, 48, 12, 16, 24, 48 },
                    64 => new List<int> { 4, 64, 32, 64, 16, 64, 32, 64, 8, 64, 32, 64, 16, 64, 32, 64 },
                    192 => new List<int>
                    {
                        4, 192, 64, 192, 48, 192, 32, 192, 24, 192, 16, 192, 12, 192, 8, 192, 12, 192, 16, 192, 24, 192,
                        32, 192, 48, 192, 64, 192
                    },
                    _ => null
                };
            }
        }
    }
}