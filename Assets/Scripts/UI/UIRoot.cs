using TMPro;
using UnityEngine;

namespace UI
{
    public class UIRoot : MonoBehaviour
    {
        [Header("Combo / Timer")]
        public TextMeshProUGUI textCombo;
        public TextMeshProUGUI textTimer;

        [Header("Song Information")] 
        public TextMeshProUGUI textTitle;
        public TextMeshProUGUI textArtist;
    }
}