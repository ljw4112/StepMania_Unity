using System;
using System.Collections.Generic;
using R3;
using Runtime.Data;
using Runtime.Object;
using UnityEngine;

namespace Runtime.Loader
{
    public class NoteMaker : MonoBehaviour
    {
        private Simfile _simfile;

        private Difficulty _difficulty;

        [SerializeField] private List<float> noteXPos = new();

        [SerializeField] private Transform trNoteParent;

        [SerializeField] private Transform noteBar;

        private List<GameObject> _noteList = new();

        private float _speed = 5;

        private const float _offsetY = 3;

        private float _bpmRatio;

        private float _lineDistance;

        private (float?, float?) _lineCompare;

        [SerializeField] private double _timer;
        
        [SerializeField] private AudioSource _audioSource;

        private bool _bStart;

        public NoteMaker SetSimfile(Simfile simfile, Difficulty difficulty)
        {
            _simfile = simfile;

            _difficulty = difficulty;

            _bpmRatio = 191 / 60f;

            return this;
        }
        
        public void InstantiateNote()
        {
            _noteList.Clear();
            
            if (_simfile == null) return;

            foreach (var measure in _simfile.NoteDatas[_difficulty].NoteInMeasure)
            {
                int beatCount = measure.Value.Count;
                
                float minYSpace = _bpmRatio / beatCount;

                for (int i = 0; i < measure.Value.Count; i++)
                {
                    string line = measure.Value[i];
                    
                    for (int j = 0; j < 5; j++)
                    {
                        if (line[j] - '0' != 1) continue;
                        
                        float yPos = _offsetY * measure.Key + (measure.Key + minYSpace * i);
                        
                        Vector3 position = new Vector3(noteXPos[j], yPos * _speed, 0);

                        var obj = Resources.Load<GameObject>("Prefab/CubeParent");

                        var cubeObj = Instantiate(obj, position, Quaternion.identity, trNoteParent);

                        var cube = cubeObj.transform.GetComponent<Cube>();
                        
                        _noteList.Add(cubeObj);
                        
                        cube.SetNoteBar(noteBar);
                    }
                }

                var lineObj = Instantiate(Resources.Load<GameObject>("Prefab/Line"), trNoteParent);

                float lineY = (_offsetY * measure.Key + measure.Key) * _speed;

                if (measure.Key == 0 && _lineCompare.Item1 == null) _lineCompare.Item1 = lineY;
                else if (measure.Key == 1 && _lineCompare.Item2 == null) _lineCompare.Item2 = lineY;
                
                lineObj.transform.position = new Vector3(0, lineY, 0);
            }

            _lineDistance = _lineCompare.Item2.Value - _lineCompare.Item1.Value;
        }

        private bool bUpdate;
        public void Move()
        {
            _audioSource.Play();
            _audioSource.Stop();
            
            bUpdate = true;

            _timer = _simfile.Offset;
        }

        private void Update()
        {
            if (!bUpdate) return;

            if (_timer >= 0 && !_bStart)
            {
                _audioSource.Play();
                _bStart = true;
            }

            if (_timer >= -_simfile.Offset)
            {
                float speed = _lineDistance / (60 / 191f * 4);
            
                trNoteParent.Translate(0, -speed * Time.deltaTime, 0);
            }
            
            _timer += Time.deltaTime;
        }
    }
}