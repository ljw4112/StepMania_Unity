using System.Collections.Generic;
using Runtime.Object.Note;
using UnityEngine;

namespace Runtime.Game.Object.ObjectPool
{
    public class NotePool : MonoSingleton<NotePool>
    {
        private Stack<Note> _notePool = new();

        public Note Get(Transform parent)
        {
            if (_notePool.Count <= 0)
                return null;

            Note note = _notePool.Pop();
            
            note.gameObject.SetActive(true);
            
            note.transform.SetParent(parent);

            return note;
        }

        public void Pool(Note note)
        {
            note.transform.SetParent(transform);
            
            note.gameObject.SetActive(false);
            
            _notePool.Push(note);
        }
    }
}