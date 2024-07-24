using Runtime.Data;
using UnityEngine;
using Utils;

namespace Runtime.Loader
{
    public class TestLoader : MonoBehaviour
    {
        [SerializeField] private NoteMaker _maker;
        
        private void Start()
        {
            Simfile file = FileLoader.FileLoad("Tsukitourou");
            
            _maker.SetSimfile(file, Difficulty.Medium).InstantiateNote();
            
            //_maker.Move();
        }
    }
}