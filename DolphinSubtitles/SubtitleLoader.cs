using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DolphinSubtitles
{
    public static class SubtitleLoader
    {
        const string TranslationDictFilenameMask = "*.{0}.json";
        public static Dictionary<string, SubtitleEntry> LoadFlattenedDicionary(string directory, bool recursive, string languageCode)
        {
            if (!Directory.Exists(directory))
            {
                return new Dictionary<string, SubtitleEntry>();
            }

            var paths = Directory
                .EnumerateFiles(
                    directory, 
                    string.Format(TranslationDictFilenameMask, languageCode),
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            Dictionary<string, SubtitleEntry> result = new Dictionary<string, SubtitleEntry>();

            //allow for multi-file dicts
            foreach (var path in paths)
            {
                var json = File.ReadAllText(path);

                try
                {
                    var dict = JsonConvert.DeserializeObject<List<SubtitleEntry>>(json);

                    foreach (var entry in dict)
                    {
                        result[entry.FileName] = entry;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };

            return result;
        }
    }
}
