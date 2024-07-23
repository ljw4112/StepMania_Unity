using System;
using UnityEngine;

namespace Runtime.Object
{
    public class Cube : MonoBehaviour
    {
        [SerializeField] private AudioClip _audioClip;

        [SerializeField] private AudioSource _audioSource;
        
        private Transform noteBar;

        private bool bClapped;
        
        public void SetNoteBar(Transform tr)
        {
            noteBar = tr;

            _audioSource.clip = _audioClip;
        }
        
        private void Update()
        {
            if (noteBar is null) return;
            
            if (!(Vector3.Distance(noteBar.position, transform.position) < 0.001f) || bClapped) return;
            
            bClapped = true;
            
            _audioSource.Play();
            
            Debug.Log("Play Tick Sound");
        }
    }
}