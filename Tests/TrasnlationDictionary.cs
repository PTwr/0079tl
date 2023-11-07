using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class TrasnlationDictionary
    {
        private static uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }
        static uint defaulColor = 4278255615;
        static uint unknown = ColorToUInt(Color.Yellow);
        static uint white = ColorToUInt(Color.White); //Gundam White, Amura, Christina
        static uint green = ColorToUInt(Color.Green); //Zaku Green, Bernard
        static uint red = ColorToUInt(Color.Red); //Chars Red, Char
        static uint pink = ColorToUInt(Color.Pink); //Girlish Pink, Aina, Hoa
        static uint blue = ColorToUInt(Color.Blue); //Destiny Blue, Yuu, Ramba, Norris
        static uint purple = ColorToUInt(Color.Purple); //Flamboyant Purple, M'Quve
        static uint orange = ColorToUInt(Color.Orange); //Ignorable Orange, Hayato, Kai
        static uint palegreen = ColorToUInt(Color.PaleGreen); //Mobs
        //na001 na002 outro? year is 0080, Char narrates
        Dictionary<string, uint> sbutitlecolors = new Dictionary<string, uint>()
        {
            //every character will get slightly different color even, so search-and-replace can change it easily
            
            { "chr", ++red }, //Char, MSG
            { "amr", ++white }, //Amuro Ray , MSG
            { "crs", ++white }, //Christina Mackenzie, War in the Pocket
            { "you", ++blue }, //Yuu Kajima, Blue Destiny?
            { "ber", ++green }, //Bernard Wiseman ,War in the Pocket
            { "ain", ++pink }, //Aina Sahalin, 8th MS
            { "rmb", ++blue }, //Ramba Ral, MSG
            { "nrs", ++blue }, //Norris Packard, 8th MS?
            { "mqv", ++purple }, //M'Quve MSG?

            //side characters
            { "hyt", ++orange }, //Hayato MSG
            { "kai", ++orange }, //Kai, MSG

            //SRT-1
            { "hoa", ++pink }, //Hoa Blanchet, SRT-1 inteligence officer
            { "eva", ++unknown }, //plot, Ace
            { "eve", ++unknown }, //plot, EFF
            { "evs", ++unknown }, //plot, shared (zeon 3rd ending?)
            { "evz", ++unknown }, //plot, Zeon
            
            //minion voices
            { "sla", ++palegreen }, //voice with static? NPC?
            { "slb", ++palegreen }, //voice with static? NPC?
            { "slc", ++palegreen }, //voice with static? NPC?
            { "sld", ++palegreen }, //voice with static? NPC?
            { "sle", ++palegreen }, //voice with static? NPC?
            { "slf", ++palegreen }, //voice with static? NPC?

            { "aka", ++unknown },
            { "aln", ++unknown },
            { "anf", ++unknown },
            { "bng", ++unknown },
            { "cls", ++unknown },
            { "dnb", ++unknown },
            { "eva", ++unknown },
            { "evs", ++unknown },
            { "evz", ++unknown },
            { "exm", ++unknown },
            { "gat", ++unknown },
            { "guy", ++unknown },
            { "krn", ++unknown },
            { "leo", ++unknown },
            { "lls", ++unknown },
            { "msv", ++unknown },
            { "nim", ++unknown },
            { "sir", ++unknown }, //Shiro Amada, 8th MS?
            { "trd", ++unknown },
            { "tsj", ++unknown },
            { "tut", ++unknown }, //tutorial
            { "yaz", ++unknown }, //some evil dude
        };

        [Fact]
        public void PrepareTLJson()
        {
            var soundFiles = Directory.EnumerateFiles(@"C:\games\wii\0079\0079_jp\DATA\files\sound\stream", "*.brstm");

            var structure = soundFiles
                .Select(s => new FileInfo(s))
                .Select(fi =>
                {
                    uint color = defaulColor;
                    var characterCode = fi.Name.Substring(0, 3);
                    if (!sbutitlecolors.TryGetValue(characterCode, out color))
                    {
                        color = defaulColor;
                    }

                    var item = new
                    {
                        FileName = "sound/stream/" + fi.Name,
                        Translation = fi.Name,
                        Miliseconds = 1000 * 5,
                        Color = color, //cyan
                        Enabled = !fi.Name.Contains("bgm"),
                        AllowDuplicate = false,
                    };

                    return item;
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(structure, Formatting.Indented);
            File.WriteAllText(@"C:\games\wii\translate.json", json);
        }
    }
}
