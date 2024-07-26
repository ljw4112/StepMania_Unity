using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        public string MusicSrc { get; set; }
        public double Offset { get; set; }
        public double SampleStart { get; set; }
        public double SampleLength { get; set; }
        public string Selectable { get; set; }
        public string Bpm { get; set; }
        public string Stop { get; set; }
        public string Bgchanges { get; set; }
        public string KeySound { get; set; }

        public Dictionary<double, float> BPM { get; } = new();

        public Dictionary<Difficulty, NoteData> NoteDatas { get; } = new();
        public Dictionary<Difficulty, int> Difficulty { get; } = new();
        
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

            foreach (var kvp in BPM)
            {
                hash = hash * 23 + kvp.Key.GetHashCode();
                hash = hash * 23 + kvp.Value.GetHashCode();
            }
        
            return hash;
        }

        public Simfile ConvertData()
        {
            var bpmDatas = Bpm.Split('=');
            BPM.Add(double.Parse(bpmDatas[0]), float.Parse(bpmDatas[1]));
            
            return this;
        }
    }
}