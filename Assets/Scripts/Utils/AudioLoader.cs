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

            // OGG 파일 처리
            if (extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.OGGVORBIS))
                {
                    var result = await req.SendWebRequest();

                    if (result.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError($"Connection error while downloading audio clip: {filePath}");
                        return null;
                    }

                    return DownloadHandlerAudioClip.GetContent(req);
                }
            }

            // MP3 파일 처리
            using (UnityWebRequest req = UnityWebRequest.Get(filePath))
            {
                var result = await req.SendWebRequest();

                if (result.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError($"Error while downloading file: {filePath}, Error: {result.error}");
                    return null;
                }

                byte[] results = req.downloadHandler.data;

                using (var memStream = new System.IO.MemoryStream(results))
                {
                    var mpgFile = new NLayer.MpegFile(memStream);
                    var samples = new float[mpgFile.Length];
                    mpgFile.ReadSamples(samples, 0, (int)mpgFile.Length);

                    var clip = AudioClip.Create("AudioClip", samples.Length, mpgFile.Channels, mpgFile.SampleRate, false);
                    clip.SetData(samples, 0);

                    return clip;
                }
            }
        }

    }
}