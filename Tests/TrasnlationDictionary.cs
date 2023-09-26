using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class TrasnlationDictionary
    {
        [Fact]
        public void PrepareTLJson()
        {
            var soundFiles = Directory.EnumerateFiles(@"C:\games\wii\0079\0079_en\DATA\files\sound\stream", "*.brstm");

            var structure = soundFiles
                .Select(s => new FileInfo(s))
                .Select(fi => new
                {
                    FileName = "sound/stream/" + fi.Name,
                    Translation = fi.Name,
                    Miliseconds = 1000 * 5,
                    Color = 4278255615, //cyan
                    Enabled = !fi.Name.Contains("bgm"),
                    AllowDuplicate = false,
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(structure, Formatting.Indented);
            File.WriteAllText(@"C:\games\wii\translate.json", json);
        }
    }
}
