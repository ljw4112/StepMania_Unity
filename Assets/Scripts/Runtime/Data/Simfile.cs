using System.Collections.Generic;

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

        public readonly Dictionary<Difficulty, NoteData> NoteDatas = new();
        public readonly Dictionary<Difficulty, int> Difficulty = new();
    }

    public class NoteData
    {
        public readonly Dictionary<int, List<string>> NoteInMeasure = new();
    }
}