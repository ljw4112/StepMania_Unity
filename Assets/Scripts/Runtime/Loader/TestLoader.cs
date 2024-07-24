using Runtime.Data;
using Runtime.Metronome;
using UnityEngine;
using Utils;

namespace Runtime.Loader
{
    public class TestLoader : MonoBehaviour
    {
        [SerializeField] private NoteMaker _maker;
        
        private void Start()
        {
            Simfile file = FileLoader.FileLoad("Jounetsu Fun Fanfare");
            
            _maker.SetSimfile(file, Difficulty.Challenge).InstantiateNote();
            
            _maker.Move();
        }
    }
}