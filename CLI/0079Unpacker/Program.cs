using CommandLine;
using CommandLine.Text;
using GEVLib.GEV;
using Newtonsoft.Json;
using System.Formats.Tar;
using System.Reflection;
using System.Xml;
using U8;
using XBFLib;

internal class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Directory containing game files")]
        public string InputDir { get; set; }
        [Option('o', "output", Required = true, HelpText = "Where files will be unpacked to")]
        public string OutputDir { get; set; }

        [Option('x', "xbf", Required = false, HelpText = "Enable XBF->XML conversion", Default = false)]
        public bool XbfToXml { get; set; }
        [Option('b', "brstm", Required = false, HelpText = "Enable BRSTM->WAV conversion", Default = false)]
        public bool BrstmToWav { get; set; }
        [Option('a', "arc", Required = false, HelpText = "Enable ARC unpacking", Default = false)]
        public bool UnpackArc { get; set; }
        [Option('d', "dict", Required = false, HelpText = "Creates json dictionaries from xbf.xml (requires --xbf in this or previous runs)", Default = false)]
        public bool CreateDict { get; set; }
        [Option('g', "gev", Required = false, HelpText = "Unpacks GEV into human readable form", Default = false)]
        public bool GevDictionary { get; set; }

        [Option('u', "unlockables", Required = false, HelpText = "Parses unlockables to human readable form", Default = false)]
        public bool ParseUnlockables { get; set; }
    }
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o =>
            {
                if (!Directory.Exists(o.InputDir))
                {
                    Console.WriteLine("Invalid input directory");
                    return;
                }

                ArcUnpackEverything(o);
            });
    }

    public class tlentry
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public string[] Lines { get; set; }
        public string TabSpace { get; set; }
        public string Size { get; set; }
        public string? PositionFlag { get; set; }
        public string? CharSpace { get; set; }
        public string? LineSpace { get; set; }
        public string? Color { get; set; }

        public override string ToString()
        {
            if (Lines?.Any() == true)
            {
                return string.Join("\n", Lines);
            }
            return Text;
        }
    }

    private static List<tlentry> ExtractTLEntries(string xbfxmlfile)
    {
        List<tlentry> result = new List<tlentry>();
        if (File.Exists(xbfxmlfile))
        {
            var xml_xbf = new XmlDocument();
            xml_xbf.Load(xbfxmlfile);

            var sgpath = Path.Combine(Path.GetDirectoryName(xbfxmlfile), "StringGroup.xbf.xml");
            var xml_sg = new XmlDocument();
            if (File.Exists(sgpath))
            {
                xml_sg.Load(sgpath);
            }

            var blocks = xml_xbf.SelectNodes("/Texts/Block");
            foreach (XmlElement block in blocks)
            {
                var id = block.SelectSingleNode("ID").InnerText;
                var text = block.SelectSingleNode("Text").InnerText;

                var sgnode = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]");

                var entry = new tlentry()
                {
                    ID = block.SelectSingleNode("ID").InnerText,
                    Lines = block.SelectSingleNode("Text").InnerText.Split("\n").ToArray(),

                    PositionFlag = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]/PositionFlag")?.InnerText,
                    CharSpace = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]/CharSpace")?.InnerText,
                    LineSpace = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]/LineSpace")?.InnerText,
                    TabSpace = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]/TabSpace")?.InnerText,
                    Color = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]/Color")?.InnerText,
                    Size = xml_sg.SelectSingleNode($"/StringGroup/String[Code[text()='{id}']]/Size")?.InnerText,
                };
                result.Add(entry);
            }
        }
        return result;
    }

    public static void ArcUnpackEverything(Options options)
    {
        if (options.UnpackArc)
        {
            var files = new System.IO.DirectoryInfo(options.InputDir)
                .GetFiles("*.arc", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var arcjp = file.FullName;

                var bytes = File.ReadAllBytes(arcjp).AsSpan();
                var root = new U8RootSegment();
                root.Parse(bytes);

                var relativePath = Path.GetRelativePath(options.InputDir, file.FullName);
                var outputPath = Path.Combine(options.OutputDir, relativePath);
                Console.WriteLine("Unpacking " + file.FullName);
                root.DumpToDisk(outputPath);
            }
        }

        if (options.BrstmToWav)
        {
            throw new NotImplementedException();
            //BrstmToWav(options);
        }

        if (options.CreateDict)
        {
            var extractedXbfXml = new System.IO.DirectoryInfo(options.OutputDir)
                        .GetFiles("BlockText.xbf.xml", SearchOption.AllDirectories);

            foreach (var file in extractedXbfXml)
            {
                var dict = ExtractTLEntries(file.FullName);
                var json = JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(file.FullName.Replace(".xbf.xml", ".xbf") + ".json", json);

                var rawtxt = string.Join(Environment.NewLine + "-----------------------------" + Environment.NewLine, dict.Select(i => i.ToString()));
                File.WriteAllText(file.FullName.Replace(".xbf.xml", ".xbf") + ".txt", rawtxt);
            }
        }

        if (options.GevDictionary)
        {
            var gevs = new System.IO.DirectoryInfo(options.InputDir)
                        .GetFiles("*.gev", SearchOption.AllDirectories);

            foreach (var gev in gevs)
            {
                var pathStub = Path.GetRelativePath(options.InputDir, gev.FullName);
                var outputPath = Path.Combine(options.OutputDir, pathStub + $".json");

                GEVBinaryRootSegment g = new GEVBinaryRootSegment();
                g.Parse(File.ReadAllBytes(gev.FullName));

                if (g.STR != null)
                {
                    var json = JsonConvert.SerializeObject(g.STR.Strings, Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(outputPath, json);
                }
            }
        }

        if (options.XbfToXml)
        {
            var files = new System.IO.DirectoryInfo(options.InputDir)
                .GetFiles("*.xbf", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var xbf = file.FullName;

                var bytes = File.ReadAllBytes(xbf).AsSpan();
                var root = new XbfRootSegment();
                root.Parse(bytes);

                var relativePath = Path.GetRelativePath(options.InputDir, file.FullName);
                var outputPath = Path.Combine(options.OutputDir, relativePath) + ".xml";
                Console.WriteLine("Unpacking XBF" + file.FullName);

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                root.DumpToDisk(outputPath);
            }
        }

        if (options.ParseUnlockables)
        {
            var flagctrlfile = Path.Combine(options.InputDir, "DATA\\files\\parameter", "FlagCtrl.xbf");
            var resultsfile = Path.Combine(options.InputDir, "DATA\\files\\parameter", "result_param.xbf");
        }
    }

    private static void BrstmToWav(Options options)
    {
        //var extractedBrstm = new System.IO.DirectoryInfo(options.OutputDir)
        //            .GetFiles("*.brstm", SearchOption.AllDirectories)
        //            .Select(file => new Tuple<string, string>(file.FullName, Path.GetRelativePath(options.OutputDir, file.FullName)));

        //var freefloatingBrstm = new System.IO.DirectoryInfo(options.InputDir)
        //    .GetFiles("*.brstm", SearchOption.AllDirectories)
        //    .Select(file => new Tuple<string, string>(file.FullName, Path.GetRelativePath(options.InputDir, file.FullName)));

        //var allBrstm = extractedBrstm.Concat(freefloatingBrstm);

        //foreach (var brstm in allBrstm)
        //{
        //    Console.WriteLine(brstm.Item1);
        //    var audio = System.Audio.WAV.FromFile(brstm.Item1);
        //    var outputPath = Path.Combine(options.OutputDir, brstm.Item2 + ".wav");
        //    WAV.ToFile(audio, outputPath);
        //}
    }
}