using TMPro;
using UnityEngine;

namespace Runtime.Object.Line
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private Transform trJudgeLine;
        
        [SerializeField] private TextMeshPro _lineNum;

        [SerializeField] private TextMeshPro _lineSeconds;

        public Line SetLineNum(int num)
        {
            _lineNum.SetText(num.ToString());
            return this;
        }

        public Line SetLineSeconds(double time)
        {
            _lineSeconds.SetText(time.ToString("F3"));
            return this;
        }
    }
}