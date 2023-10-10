namespace DolphinSubtitles
{
    public class SubtitleEntry
    {
        public string FileName { get; set; }
        public string Translation { get; set; }
        public uint Miliseconds { get; set; }
        public uint Color { get; set; }
        public bool Enabled { get; set; }
        public bool AllowDuplicate { get; set; }
        public float Scale { get; set; }
    }
}