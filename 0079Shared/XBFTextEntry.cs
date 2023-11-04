using System.Text.Json.Serialization;

namespace _0079Shared
{
    public class XBFTextEntry
    {
        [JsonIgnore]
        public string Source { get; set; }

        public string ID { get; set; }
        public string Text { get; set; }
        public string[] Lines { get; set; }

        public string? Color { get; set; }
        public string? TabSpace { get; set; }
        public string? Size { get; set; }

        public string? CharSpace { get; set; }
        public string? LineSpace { get; set; }
        public string? PositionFlag { get; set; }

        public override string ToString()
        {
            if (Lines?.Any() == true)
            {
                return string.Join("\n", Lines);
            }
            return Text;
        }
    }
}