using CommandLine.Text;
using CommandLine;
using U8;
using System.Xml;
using XBFLib;
using Newtonsoft.Json;
using InMemoryBinaryFile.Helpers;
using System.Reflection.Metadata.Ecma335;

internal class Program
{
    [Verb("mission", HelpText = "Extracts human-readable form for translation.")]
    public class ExtractionOptions
    {
        [Option('i', "input", Required = true, HelpText = "Directory containing unpacked game files")]
        public string InputDir { get; set; }
        [Option('o', "output", Required = true, HelpText = "Directory for patch files")]
        public string OutputDir { get; set; }
        [Option('o', "overwrite", Required = false, HelpText = "Overwrites existing files", Default = false)]
        public bool Overwrite { get; set; }
        [Option('l', "language", Required = false, HelpText = "Language code, defaults to en", Default = "en")]
        public string LanguageCode { get; set; }
    }
    [Verb("patch", HelpText = "Create game patch from translation")]
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Directory containing clean game files")]
        public string InputDir { get; set; }
        [Option('o', "output", Required = true, HelpText = "Directory containing patched game")]
        public string OutputDir { get; set; }
        [Option('p', "patch", Required = true, HelpText = "Directory containing patch files")]
        public string PatchDir { get; set; }
        [Option('l', "language", Required = false, HelpText = "Language code, defaults to en", Default = "en")]
        public string LanguageCode { get; set; }
        [Option('r', "rewrite", Required = false, HelpText = "Rewrite everything, even if not in patch", Default = false)]
        public bool Rewrite { get; set; }
        [Option('c', "clean", Required = false, HelpText = "Delets previous patch", Default = false)]
        public bool Clean { get; set; }
    }
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<ExtractionOptions, Options>(args)
            .WithParsed<ExtractionOptions>(o =>
            {
                if (!Directory.Exists(o.InputDir))
                {
                    Console.WriteLine("Invalid input directory");
                    return;
                }

                ExtractMissionTL(o.InputDir, o.OutputDir, o.LanguageCode, o.Overwrite);

            })
            .WithParsed<Options>(o =>
            {
                if (!Directory.Exists(o.InputDir))
                {
                    Console.WriteLine("Invalid input directory");
                    return;
                }
                if (!Directory.Exists(o.PatchDir))
                {
                    Console.WriteLine("Invalid patch directory");
                    return;
                }

                ArcPatchEerything(o.InputDir, o.OutputDir, o.PatchDir, o.LanguageCode, o.Rewrite, o.Clean);
            });
    }

    public static void ExtractMissionTL(string inputDir, string outputDir, string lang, bool overwrite)
    {
        Dictionary<string, string> missionCodes = new()
        {
            { "ME", "EFF" },
            { "MZ", "Zeon" },
            {"AA", "Ace_Amuro_Ray" }, //EFF
            {"AB", "Ace_Bernard_Wiseman" }, //Zeon War in The Pocekt (Zaku Kai)
            {"AC", "Ace_Char_Aznable" }, //Zeon
            {"AG", "Ace_Gaia" }, //Zeon (Black Tri Star)
            {"AH", "Ace_Akahana" }, //Zeon MSG, demolition expert in Acguy
            {"AK", "Ace_Christina_Mackenzie" }, //EFF War In The Pocket (Alex NT1)
            {"AN", "Ace_Norris_Packard" }, //Zeon
            {"AR", "Ace_Ramba_Ral" }, //Zeon
            {"AS", "Ace_Shiro_Amada" }, //EFF 8th MS team
            {"AY", "Ace_Yuu_Kajima" }, //EFF blue destiny
            {"CE", "Combination_EFF" }, //EFF
            {"CZ", "Combination_Zeon" }, //Zeon
        };

        var dirs = Directory.EnumerateDirectories(inputDir, "BR_*_text.arc", SearchOption.AllDirectories); ;

        foreach (var missionDir in dirs)
        {
            var arcName = Path.GetFileName(missionDir);
            var missionCode = arcName.Substring(3, 2);
            var missionNumber = arcName.Substring(5, 2);
            List<tlentry> ChatTL = new List<tlentry>();
            List<tlentry> ObjectivesAndIntel = new List<tlentry>();

            var chatfile = Path.Combine(missionDir, $"arc/CH_{missionCode}{missionNumber}.arc/arc/BlockText.xbf.xml");
            var objectivefile = Path.Combine(missionDir, $"arc/OP_{missionCode}{missionNumber}.arc/arc/BlockText.xbf.xml");
            var intelfile = Path.Combine(missionDir, $"arc/BR_{missionCode}{missionNumber}.arc/arc/BlockText.xbf.xml");
            ExtractTLEntries(ChatTL, chatfile);
            ExtractTLEntries(ObjectivesAndIntel, objectivefile);
            ExtractTLEntries(ObjectivesAndIntel, intelfile);

            if (ChatTL.Any())
            {
                var json = JsonConvert.SerializeObject(ChatTL, Newtonsoft.Json.Formatting.Indented);
                var path = Path.Combine(outputDir, "Unique/_2d/Briefing", arcName, $"dict-{missionCodes[missionCode]}_{missionNumber}-chat.{lang}.json");
                if (!File.Exists(path) || overwrite)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    File.WriteAllText(path, json);
                }
            }
            if (ObjectivesAndIntel.Any())
            {
                var json = JsonConvert.SerializeObject(ObjectivesAndIntel, Newtonsoft.Json.Formatting.Indented);
                var path = Path.Combine(outputDir, "Unique/_2d/Briefing", arcName, $"dict-{missionCodes[missionCode]}_{missionNumber}.{lang}.json");
                if (!File.Exists(path) || overwrite)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    File.WriteAllText(path, json);
                }
            }
        }

    }

    private static void ExtractTLEntries(List<tlentry> ChatTL, string chatfile)
    {
        if (File.Exists(chatfile))
        {
            var xmldoc = new XmlDocument();
            xmldoc.Load(chatfile);

            var blocks = xmldoc.SelectNodes("/Texts/Block");
            foreach (XmlElement block in blocks)
            {
                var id = block.SelectSingleNode("ID").InnerText;
                var text = block.SelectSingleNode("Text").InnerText;

                var entry = new tlentry()
                {
                    ID = block.SelectSingleNode("ID").InnerText,
                    Lines = block.SelectSingleNode("Text").InnerText.Split("\n").ToArray(),
                };
                ChatTL.Add(entry);
            }
        }
    }

    const string TranslationDictFilenameMask = "dict*.{0}.json";
    public static void ArcPatchEerything(string inputDir, string outputDir, string patchDir, string languageCode, bool rewrite, bool clean)
    {
        if (Directory.Exists(outputDir) && clean)
        {
            Directory.Delete(outputDir, true);
        }

        //get global dicts first
        Dictionary<string, tlentry> globaldict = GetTLDict(Path.Combine(patchDir, UniqueDir), languageCode);

        var files = new System.IO.DirectoryInfo(inputDir)
            .GetFiles("*.arc", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var arcjp = file.FullName;

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            var arcPath = Path.GetRelativePath(inputDir, file.FullName);
            var updated = UpdateU8Root(root, Path.Combine(patchDir, UniqueDir, arcPath), Path.Combine(patchDir, CommonDir), Path.Combine(inputDir, arcPath), languageCode, new List<Dictionary<string, tlentry>>() { globaldict });

            if (updated || rewrite)
            {
                var newbytes = root.GetBytes().ToArray();
                var stub = Path.GetRelativePath(inputDir, file.FullName);
                var patchedFile = Path.Combine(outputDir, stub);
                Directory.CreateDirectory(Path.GetDirectoryName(patchedFile));
                File.WriteAllBytes(patchedFile, newbytes);
            }
        }
    }

    private static Dictionary<string, tlentry> GetTLDict(string patchDir, string languageCode)
    {
        if (!Directory.Exists(patchDir))
        {
            return new Dictionary<string, tlentry>();
        }

        var paths = Directory
            .EnumerateFiles(patchDir, string.Format(TranslationDictFilenameMask, languageCode), SearchOption.TopDirectoryOnly);

        Dictionary<string, tlentry> result = new Dictionary<string, tlentry>();

        //allow for multi-file dicts
        foreach (var path in paths)
        {
            var json = File.ReadAllText(path);
            var dict = JsonConvert.DeserializeObject<List<tlentry>>(json);

            foreach (var entry in dict)
            {
                result[entry.ID] = entry;
            }
        };

        return result;
    }

    public class tlentry
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public string[] Lines { get; set; }
        public string TabSpace { get; set; }
        public string Size { get; set; }

        public override string ToString()
        {
            if (Lines?.Any() == true)
            {
                return string.Join("\n", Lines);
            }
            return Text;
        }
    }

    const string CommonDir = "Common";
    const string UniqueDir = "Unique";
    private static bool UpdateU8Root(U8RootSegment root, string patchDir, string commonDir, string inputDir, string languageCode, List<Dictionary<string, tlentry>> dicts)
    {
        dicts = new List<Dictionary<string, tlentry>>(dicts) //clone list
        {
            //append new dict
            GetTLDict(patchDir, languageCode)
        };

        bool updated = false;
        int offsetChange = 0;
        foreach (var node in root.Nodes)
        {
            if (node.IsArc)
            {
                var nestedRoot = new U8RootSegment();
                nestedRoot.Parse(node.BinaryData);
                var nestedArcUpdated = UpdateU8Root(nestedRoot, Path.Combine(patchDir, node.Path), commonDir, Path.Combine(inputDir, node.Path), languageCode, dicts);

                if (nestedArcUpdated)
                {
                    var newData = nestedRoot.GetBytes().ToArray();

                    updated |= !newData.SequenceEqual(node.BinaryData);

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;
                }
            }
            else if (node.IsXbf)
            {
                var xmlenpath = Path.Combine(patchDir, node.Path);
                xmlenpath = xmlenpath.Replace(".xbf", $".xbf.{languageCode}.xml");

                XmlDocument doc = new XmlDocument();
                if (File.Exists(xmlenpath)) //patch file
                {
                    doc.Load(xmlenpath);
                }
                else
                {
                    //var xbf = (node as XbfRootSegment);
                    var xbf = new XbfRootSegment(XbfRootSegment.ShouldBeUTF8(xmlenpath));
                    xbf.Parse(node.BinaryData.AsSpan(), node.BinaryData.AsSpan());
                    doc = xbf.NodeTree.XmlDocument;
                }

                if (xmlenpath.Contains("BlockText.xbf"))
                {

                    var blocks = doc.SelectNodes("/Texts/Block");
                    foreach (XmlElement block in blocks)
                    {
                        var id = block.SelectSingleNode("ID").InnerText;
                        var text = block.SelectSingleNode("Text").InnerText;

                        //from newest dict to oldest
                        foreach (var dict in dicts.AsQueryable().Reverse())
                        {
                            if (dict.TryGetValue(id, out var tl))
                            {
                                block.SelectSingleNode("Text").InnerText = tl.ToString();
                                //dont look in parent dict if TL was found
                                break;
                            }
                        }
                    }
                }
                if (xmlenpath.Contains("StringGroup.xbf"))
                {
                    var strs = doc.SelectNodes("/StringGroup/String");
                    foreach (XmlElement str in strs)
                    {
                        var id = str.SelectSingleNode("Code");
                        var tabspace = str.SelectSingleNode("TabSpace");
                        var size = str.SelectSingleNode("Size");

                        //from newest dict to oldest
                        foreach (var dict in dicts.AsQueryable().Reverse())
                        {
                            if (dict.TryGetValue(id.InnerText, out var tl))
                            {
                                if (!string.IsNullOrWhiteSpace(tl.TabSpace))
                                {
                                    tabspace.InnerText = tl.TabSpace;
                                    break;
                                }
                            }
                        }
                        //from newest dict to oldest
                        foreach (var dict in dicts.AsQueryable().Reverse())
                        {
                            if (dict.TryGetValue(id.InnerText, out var tl))
                            {
                                if (!string.IsNullOrWhiteSpace(tl.Size))
                                {
                                    size.InnerText = tl.Size;
                                    break;
                                }
                            }
                        }
                    }
                }

                var parsed = new XbfRootSegment(doc, XbfRootSegment.ShouldBeUTF8(xmlenpath));

                var newData = parsed.GetBytes().ToArray();

                Console.WriteLine("XBF: " + inputDir + "/" + node.Name);
                updated |= !newData.SequenceEqual(node.BinaryData);

                offsetChange -= node.BinaryData.Length;
                offsetChange += newData.Length;

                node.BinaryData = newData;
                node.Size = newData.Length;
            }
            else if (node.IsFile && node.Name.EndsWith(".lua"))
            {
                byte[] newData = null;

                var patchfile = Path.Combine(patchDir, node.Path);
                patchfile = patchfile.Replace(".lua", $".{languageCode}.lua");

                if (File.Exists(patchfile)) //patch file
                {
                    newData = File.ReadAllBytes(patchfile);
                }
                else
                {
                    //override script with commont file
                    var filename = Path.GetFileName(Path.GetFileName(patchfile));
                    var commonfile = Directory //allow for subdirectories
                        .EnumerateFiles(commonDir, "*.*", SearchOption.AllDirectories)
                        .Where(i => Path.GetFileName(i) == filename)
                        .FirstOrDefault();

                    if (File.Exists(commonfile))
                    {
                        newData = File.ReadAllBytes(commonfile);
                        patchfile = commonfile;
                    }
                }

                if (newData != null)
                {
                    //try to match patch file length
                    if (newData.Length > node.BinaryData.Length)
                    {
                        var dataReadAsText = File.ReadAllText(patchfile);

                        var lines = dataReadAsText
                            .Split('\x0D', '\x0A');

                        var trimmedLines = lines
                            .Select(i => i.Trim()) //ignore formatting
                            .Select(i => i.Replace(" = ", "=")) // unnecessary spaces in middle of dict lines
                            .Where(i => !i.StartsWith("--")) //skip comments
                            .Where(i => !string.IsNullOrWhiteSpace(i)) //skip empty lines
                            ;

                        var trimmedscript = string.Join("\x0D\x0A", trimmedLines);
                        newData = trimmedscript.ToUTF8Bytes();

                        //padd spaces to match lnegth without tweaking .arc
                        if (newData.Length < node.BinaryData.Length)
                        {
                            var diff = node.BinaryData.Length - newData.Length;
                            var spaces = Enumerable.Repeat((byte)0x20, diff).ToArray();
                            newData = newData.Concat(spaces).ToArray();
                        }
                        else
                        {
                            Console.WriteLine("LUA trimming failed for: " + inputDir + "/" + node.Name);
                        }
                    }
                    else
                    { 
                        var diff = node.BinaryData.Length - newData.Length;
                        var spaces = Enumerable.Repeat((byte)0x20, diff).ToArray();
                        newData = newData.Concat(spaces).ToArray();
                    }

                    updated |= !newData.SequenceEqual(node.BinaryData);

                    Console.WriteLine("LUA: " + inputDir + "/" + node.Name);
                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;
                }
            }
            else if (node.IsFile && node.Name.EndsWith(".xml"))
            {
                var path = Path.Combine(patchDir, node.Path);
                path = path.Replace(".xml", $".{languageCode}.xml");
                if (File.Exists(path))
                {
                    var newData = File.ReadAllBytes(path);

                    updated |= !newData.SequenceEqual(node.BinaryData);

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;
                }
            }
        }
        return updated;
    }
}

