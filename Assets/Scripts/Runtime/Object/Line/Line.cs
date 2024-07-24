using TMPro;
using UnityEngine;

namespace Runtime.Object.Line
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _lineNum;

        public Line SetLineNum(int num)
        {
            _lineNum.SetText(num.ToString());
            return this;
        }
    }
}