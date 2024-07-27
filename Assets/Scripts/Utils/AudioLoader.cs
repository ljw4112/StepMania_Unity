using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils
{
    public static class AudioLoader
    {
        public static async UniTask<AudioClip> GetAudioClip(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                using UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.OGGVORBIS);
                var result = await req.SendWebRequest();

                return result.result == UnityWebRequest.Result.ConnectionError ? null : DownloadHandlerAudioClip.GetContent(req);
            }
            
            using UnityWebRequest _req = UnityWebRequest.Get(filePath);
            
            var _result = await _req.SendWebRequest();
            if (_result.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
                return null;

            byte[] results = _req.downloadHandler.data;
            var memStream = new System.IO.MemoryStream(results);
            var mpgFile = new NLayer.MpegFile(memStream);
            var samples = new float[mpgFile.Length];
            mpgFile.ReadSamples(samples, 0, (int)mpgFile.Length);


            var clip = AudioClip.Create("name", samples.Length, mpgFile.Channels, mpgFile.SampleRate, false);
            clip.SetData(samples, 0);

            return clip;
        } 
    }
}