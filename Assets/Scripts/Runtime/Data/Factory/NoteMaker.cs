using System.Collections.Generic;
using System.Linq;
using Runtime.Object.Note;
using UnityEngine;

namespace Runtime.Data.Factory
{
    public class NoteMaker : MonoBehaviour
    {
        private Simfile _simfile;

        private Difficulty _difficulty;

        [SerializeField] private List<float> noteXPos = new();

        [SerializeField] private Transform trNoteParent;

        [SerializeField] private Transform trJudgeLine;

        private float _speed = 30;

        /// <summary>
        /// 1마디 움직이는 속도 (두 마디 사이의 거리 / 4분음표 4개 지나가는 시간 ( 60 / bpm * 4 )
        /// </summary>

        private float _scrollSpeed;
        
        public float GetScrollSpeed(float bpm)
        {
            return _lineDistance / (60 / bpm * 4);
        }
        
        public bool IsNoteCreated { get; private set; }

        // 4분음표 사이의 간격 (1초 : 60bpm기준)
        private float _bpmRatio;

        private float _lineDistance;

        private (float?, float?) _lineCompare;

        private bool _bStart;

        private List<(int line, float yPos)> _longNoteStack = new();
        
        public NoteMaker SetSimfile(Simfile simfile, Difficulty difficulty)
        {
            _simfile = simfile;

            _difficulty = difficulty;

            return this;
        }

        public void InstantiateNote()
        {
            _longNoteStack.Clear();

            if (_simfile == null) return;

            foreach (var measure in _simfile.NoteDatas[_difficulty].NoteInMeasure)
            {
                //_bpmRatio = 60 / 259f;
                
                int beatCount = measure.Value.Count;

                // beatCount : 이 마디에 최소 박자단위가 몇인지 (ex. 4 => 4박만으로 이루어져있다)
                // minYSpace : 이 마디안에서 노트 하나와 하나 사이의 간격
                float minYSpace = 1f / beatCount;

                List<int> beatContains = new();
                for (int i = 4; i <= beatCount; i++)
                {
                    if (beatCount % i == 0 && i != 6) beatContains.Add(i);
                }
                
                if (beatContains.Count <= 0) beatContains.Add(4);

                for (int i = 0; i < beatCount; i++)
                {
                    string line = measure.Value[i];

                    if (line == "00000") continue;

                    // 현재 라인에 노트 색상 지정
                    Color noteColor = CheckCurLineColor(beatContains, i);

                    bool bPlayTick = true;

                    for (int j = 0; j < 5; j++)
                    {
                        if (line[j] - '0' == 0 || line[j] == 'M') continue;
                        
                        // 현재 노트의 좌표 계산
                        float yPos = measure.Key + minYSpace * i;
                        
                        if (line[j] - '0' == 2)
                        {
                            _longNoteStack.Add((j, yPos));
                            continue;
                        }
                        
                        if (line[j] - '0' == 3)
                        {
                            var data = _longNoteStack.Find(x => x.line == j);
                            if (!data.Equals(default))
                            {
                                _longNoteStack.Remove(data);
                                var longNotePrefab = Resources.Load<GameObject>("Prefab/LongNote");
                                
                                Vector3 start = new Vector3(0, 0, 0);
                                Vector3 end = new Vector3(0, (yPos - data.yPos) * _speed, 0);

                                var longObj = Instantiate(longNotePrefab, trNoteParent);

                                longObj.transform.localPosition = new Vector3(noteXPos[j], data.yPos * _speed, 0);
                                
                                if (longObj.TryGetComponent<LongNote>(out var longNote))
                                {
                                    longNote.SetPosition(trJudgeLine, start, end);
                                }
                            }

                            continue;
                        }

                        // 좌표를 Vector3로 저장, 저장할 때는 현재 배속을 곱해줘서 노트 사이의 간격을 조절
                        Vector3 position = new Vector3(noteXPos[j], yPos * _speed, 0);

                        //=== 오브젝트 생성
                        var obj = Resources.Load<GameObject>("Prefab/Note");

                        var noteObj = Instantiate(obj, trNoteParent);

                        noteObj.transform.localPosition = position;

                        if (noteObj.TryGetComponent<Note>(out var note))
                        {
                            note.SetJudgeLine(trJudgeLine);

                            note.SetNoteColor(noteColor);

                            note.SetTickSound(bPlayTick);
                        }

                        note.gameObject.name = $"Note_{measure.Key}_{i}";

                        //=== 오브젝트 생성 끝

                        // 같은 라인의 노트는 한 노트만 Tick을 재생하도록
                        bPlayTick = false;
                    }
                }

                // 마디선 생성
                var lineObj = Instantiate(Resources.Load<GameObject>("Prefab/Line"), trNoteParent);

                // if (lineObj.transform.TryGetComponent<Object.Line.Line>(out var lineComponent))
                // {
                //     lineComponent.SetLineNum(measure.Key);
                //     
                //     // 마디선 실제 시간 계산
                //     double realTime = -_simfile.Offset + _bpmRatio * 4 * measure.Key;
                //     
                //     lineComponent.SetLineSeconds(realTime);
                // }

                // 마디선의 Y좌표 계산, 마디선은 해당 마디의 첫번째 노트와 좌표가 똑같아야 된다.
                float lineY = measure.Key * _speed;
                

                switch (measure.Key)
                {
                    // 좌표 사이의 거리를 계산해서 60bpm일 때 1을 기준으로 속도를 계산하기 위해 데이터 저장
                    case 0 when _lineCompare.Item1 == null:
                        _lineCompare.Item1 = lineY;
                        break;
                    case 1 when _lineCompare.Item2 == null:
                        _lineCompare.Item2 = lineY;
                        break;
                }

                // 위에서 계산된 좌표 삽입
                lineObj.transform.localPosition = new Vector3(trJudgeLine.position.x, lineY, 0);
            }

            // 마디선과 마디선 사이의 간격
            _lineDistance = _lineCompare.Item2.Value - _lineCompare.Item1.Value;
            
            IsNoteCreated = true;
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