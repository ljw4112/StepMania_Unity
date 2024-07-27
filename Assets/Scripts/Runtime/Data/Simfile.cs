using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Runtime.Data.Factory;
using UnityEngine;
using Utils;

namespace Runtime.Data
{
    public enum Difficulty
    {
        None,
        Medium,
        Challenge
    }
    
    public class Simfile
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Artist { get; set; }
        public string TitleTranslit { get; set; }
        public string SubtitleTranslit { get; set; }
        public string ArtistTranslit { get; set; }
        public string Genre { get; set; }
        public string Credit { get; set; }
        public string BannerSrc { get; set; }
        public string BackgroundSrc { get; set; }
        public string LyricsPath { get; set; }
        public string CdtitleSrc { get; set; }
        public string Music { get; set; }
        public double Offset { get; set; }
        public double SampleStart { get; set; }
        public double SampleLength { get; set; }
        public string Selectable { get; set; }
        public string Bpms { get; set; }
        public string Stop { get; set; }
        public string Bgchanges { get; set; }
        public string KeySound { get; set; }

        public Queue<(int seconds, float bpm)> BPM { get; } = new();

        public Dictionary<Difficulty, NoteData> NoteDatas { get; } = new();
        public Dictionary<Difficulty, int> Difficulty { get; } = new();

        public AudioClip MusicAudioClip { get; set; }
        
        public class NoteData
        {
            public readonly Dictionary<int, List<string>> NoteInMeasure = new();
        }

        public override int GetHashCode()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType != typeof(Dictionary<Difficulty, NoteData>) && 
                            p.PropertyType != typeof(Dictionary<Difficulty, int>))
                .ToArray();

            int hash = properties.Select(property => property.GetValue(this))
                .Aggregate(17, (current, value) => current * 23 + (value != null ? value.GetHashCode() : 0));
        
            return hash;
        }

        public Simfile ConvertData()
        {
            var bpmDatas = Bpms.Split(',');
            var bpmContainerByMeasure = bpmDatas.Select(data => data.Split('='))
                .Where(bpmData => bpmData.Length >= 2)
                .ToDictionary(bpmData => double.Parse(bpmData[0]), bpmData => float.Parse(bpmData[1]));

            double currentBpm = bpmContainerByMeasure.First().Value;

            double singleMeasureTime = 60 / currentBpm * 4;

            double prevMeasure = 0;
            
            foreach ((double measure, float bpm) in bpmContainerByMeasure)
            {
                if (measure == 0)
                {
                    BPM.Enqueue(((int)(Offset * 1000), bpm));

                    prevMeasure = 0;
                    
                    continue;
                }

                double prevSeconds = (double)BPM.Last().seconds / 1000;
                
                double time = Math.Round(singleMeasureTime * (measure - prevMeasure) / 4 + prevSeconds, 3);
                
                BPM.Enqueue(((int)(time * 1000), bpm));

                singleMeasureTime = Math.Round(60 / bpm * 4, 3);

                prevMeasure = measure;
            }
            
            return this;
        }

        public Simfile LoadMusic(string filePath)
        {
            MusicAudioClip = AudioLoader.GetAudioClip(filePath + Music).GetAwaiter().GetResult();

            return MusicAudioClip != null ? this : null;
        }
    }
}