using UnityEngine;

namespace Runtime.Object.Note
{
    public class LongNote : MonoBehaviour
    {
        [SerializeField] private LineRenderer _renderer;

        public LongNote SetPosition(Vector3 start, Vector3 end, float width = 0.5f)
        {
            if (_renderer == null) return null;

            _renderer.SetPositions(new []{ start, end });
            
            return this;
        }
    }
}