using System.Text.Json.Serialization;

namespace _0079Shared
{
    public class XBFTextEntry
    {
        [JsonIgnore]
        public string Source { get; set; }

        public double LineSplit { get; set; }
        public List<string> Split()
        {
            return Lines
                .Select((s, n) => (s, n))
                .GroupBy(x => (int)(x.Item2 / LineSplit)) //group into sequential chunks
                .Select(x => string.Join("\n", x.Select(y => y.s)))
                .ToList();
        }

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