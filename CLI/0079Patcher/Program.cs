using CommandLine.Text;
using CommandLine;
using U8;
using System.Xml;
using XBFLib;
using Newtonsoft.Json;
using InMemoryBinaryFile.Helpers;
using System.Reflection.Metadata.Ecma335;
using GEVLib.GEV;
using System.Text;
using _0079Shared;
using System.Text.RegularExpressions;
using XBFLib.New;
using InMemoryBinaryFile.New.Serialization;

internal class Program
{
    [Obsolete("Move to unpacker")]
    [Verb("mission", HelpText = "Extracts human-readable form for translation.")]
    public class ExtractionOptions
    {
        public ExtractionOptions()
        {
            InputDir = "";
            OutputDir = "";
            Overwrite = false;
            LanguageCode = "";
        }

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
        public Options()
        {
            InputDir = "";
            OutputDir = "";
            PatchDir = "";
            LanguageCode = "";
            Rewrite = false;
            Clean = false;
            Subtitles = "";
            GevPatch = false;
            ArcPatch = false;
        }

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
        [Option('s', "subtitles", Required = false, HelpText = "Output dir for subtitle files", Default = null)]
        public string Subtitles { get; set; }
        [Option('g', "gev", Required = false, HelpText = "Patch GEV scripts", Default = false)]
        public bool GevPatch { get; set; }
        [Option('a', "arc", Required = false, HelpText = "Patch ARC archives", Default = false)]
        public bool ArcPatch { get; set; }
    }
    private static void Main(string[] args)
    {
        new Options();
        new ExtractionOptions();

        Parser.Default.ParseArguments<ExtractionOptions, Options>(args)
            .WithParsed((Action<ExtractionOptions>)(o =>
            {
                if (!Directory.Exists(o.InputDir))
                {
                    Console.WriteLine("Invalid input directory");
                    return;
                }

                ExtractMissionTL(o.InputDir, o.OutputDir, o.LanguageCode, o.Overwrite);

            }))
            .WithParsed((Action<Options>)(o =>
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

                if (o.ArcPatch)
                {
                    ArcPatchEerything(o.InputDir, o.OutputDir, Path.Combine(o.PatchDir, "Patch"), o.LanguageCode, o.Rewrite, o.Clean);
                }
                CopySubtitles(o);
                PatchGev(o);
            }));
    }

    private static void PatchGev(Options o)
    {
        if (!o.GevPatch) return;

        var jpgevs = Directory.EnumerateFiles(o.InputDir, $"*.gev", SearchOption.AllDirectories);

        foreach (var jpgev in jpgevs)
        {
            var stub = Path.GetRelativePath(o.InputDir, jpgev);
            var output = Path.Combine(o.OutputDir, stub);

            var dict = Path.Combine(o.PatchDir, "Patch/GEV", Path.GetFileNameWithoutExtension(jpgev) + ".gev.en.json");
            if (File.Exists(dict))
            {
                List<string> strings = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(dict));

                var gev = new GEVBinaryRootSegment();
                gev.Parse(File.ReadAllBytes(jpgev));

                gev.STR.ReplaceStrings(strings);
                gev.OFS.UpdateIndexes();

                var bytes = gev.GetBytes().ToArray();

                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllBytes(output, bytes);
            }
        }
    }

    private static void CopySubtitles(Options o)
    {
        if (!string.IsNullOrWhiteSpace(o.Subtitles))
        {
            var outputDir = Path.Combine(o.Subtitles, "Subtitles");
            var subtitleDir = Path.Combine(o.PatchDir, "Subtitles");

            if (o.Clean && Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }

            Directory.CreateDirectory(outputDir);

            var subtitleFiles = Directory.EnumerateFiles(subtitleDir, $"*.{o.LanguageCode}.json");

            foreach (var file in subtitleFiles)
            {
                var stub = Path.GetRelativePath(subtitleDir, file);
                var output = Path.Combine(outputDir, stub);

                File.Copy(file, output);
            }
        }
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
            List<XBFTextEntry> ChatTL = new List<XBFTextEntry>();
            List<XBFTextEntry> ObjectivesAndIntel = new List<XBFTextEntry>();

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

    private static void ExtractTLEntries(List<XBFTextEntry> ChatTL, string chatfile)
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

                var entry = new XBFTextEntry()
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

        //free flaoting xbf
        //TODO rewrite into reusable module
        //var hacked = @"C:\Users\LordOfTheSkrzynka\Documents\Dolphin Emulator\Load\Riivolution\parameter\result_param.xbf.en.xml";
        //Directory.CreateDirectory(@"C:\Users\LordOfTheSkrzynka\Documents\Dolphin Emulator\Load\Riivolution\R79JAF_EN\parameter");
        //var hhh = @"C:\Users\LordOfTheSkrzynka\Documents\Dolphin Emulator\Load\Riivolution\R79JAF_EN\parameter\result_param.xbf";
        //var xbfh = new XbfFile(File.ReadAllText(hacked));
        //var bbb = Serializer.Serialize(xbfh);
        //File.WriteAllBytes(hhh, bbb.ToArray());

        //get global dicts first
        Dictionary<string, XBFTextEntry> globaldict = GetTLDict(Path.Combine(patchDir, UniqueDir), languageCode);

        var files = new System.IO.DirectoryInfo(inputDir)
            .GetFiles("*.arc", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var arcjp = file.FullName;

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            var arcPath = Path.GetRelativePath(inputDir, file.FullName);
            var updated = UpdateU8Root(root, Path.Combine(patchDir, UniqueDir, arcPath), Path.Combine(patchDir, CommonDir), Path.Combine(inputDir, arcPath), languageCode, new List<Dictionary<string, XBFTextEntry>>() { globaldict });

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

    private static Dictionary<string, XBFTextEntry> GetTLDict(string patchDir, string languageCode)
    {
        if (!Directory.Exists(patchDir))
        {
            return new Dictionary<string, XBFTextEntry>();
        }

        var paths = Directory
            .EnumerateFiles(patchDir, string.Format(TranslationDictFilenameMask, languageCode), SearchOption.TopDirectoryOnly);

        Dictionary<string, XBFTextEntry> result = new Dictionary<string, XBFTextEntry>();

        //allow for multi-file dicts
        foreach (var path in paths)
        {
            var json = File.ReadAllText(path);

            try
            {
                var dict = JsonConvert.DeserializeObject<List<XBFTextEntry>>(json);

                foreach (var entry in dict)
                {
                    result[entry.ID] = entry;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        };

        return result;
    }

    const string CommonDir = "Common";
    const string UniqueDir = "Unique";
    private static bool UpdateU8Root(U8RootSegment root, string patchDir, string commonDir, string inputDir, string languageCode, List<Dictionary<string, XBFTextEntry>> dicts)
    {
        dicts = [.. dicts, GetTLDict(patchDir, languageCode)];

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
                var appendinxPath = xmlenpath.Replace(".xbf", $".xbf.{languageCode}.appendix.xml");
                xmlenpath = xmlenpath.Replace(".xbf", $".xbf.{languageCode}.xml");

                XmlDocument doc = new XmlDocument();
                if (File.Exists(xmlenpath)) //patch file
                {
                    doc.Load(xmlenpath);
                }
                else
                {
                    //var xbf = (node as XbfRootSegment);
                    //var xbf = new XbfRootSegment(XbfRootSegment.ShouldBeUTF8(xmlenpath));
                    //xbf.Parse(node.BinaryData.AsSpan(), node.BinaryData.AsSpan());

                    var xbffile = Deserializer.Deserialize<XbfFile>(node.BinaryData);

                    doc = xbffile.GetXmlDocument();
                }

                if (File.Exists(appendinxPath))
                {
                    var appendixDoc = new XmlDocument();
                    appendixDoc.LoadXml(File.ReadAllText(appendinxPath));
                    var children = appendixDoc.ChildNodes.Cast<XmlNode>().Where(i => i is not XmlDeclaration).First();
                    foreach (XmlNode child in children.ChildNodes)
                    {
                        var imported = doc.ImportNode(child, true);
                        doc.FirstChild.AppendChild(imported);
                    }
                }

                if (xmlenpath.Contains("BlockText.xbf"))
                {
                    var textNode = doc.SelectSingleNode("/Texts");
                    var blocks = doc.SelectNodes("/Texts/Block");
                    foreach (XmlElement block in blocks)
                    {
                        var id = block.SelectSingleNode("ID").InnerText;
                        //var text = block.SelectSingleNode("Text").InnerText;

                        //from newest dict to oldest
                        foreach (var dict in dicts.AsQueryable().Reverse())
                        {
                            if (dict.TryGetValue(id, out var tl))
                            {
                                block.SelectSingleNode("Text").InnerText = tl.ToString();

                                if (tl.LineSplit > 0)
                                {
                                    var lines = tl.Split();
                                    for (int n = 0; n < lines.Count; n++)
                                    {
                                        var clone = block.Clone();
                                        clone.SelectSingleNode("ID").InnerText = $"{clone.SelectSingleNode("ID").InnerText}-{n}";
                                        clone.SelectSingleNode("Text").InnerText = lines[n];
                                        textNode.AppendChild(clone);
                                    }
                                }

                                //dont look in parent dict if TL was found
                                break;
                            }
                        }
                    }
                }
                if (xmlenpath.Contains("StringGroup.xbf"))
                {
                    var group = doc.SelectSingleNode("/StringGroup");
                    var strs = doc.SelectNodes("/StringGroup/String");
                    foreach (XmlElement str in strs)
                    {
                        var id = str.SelectSingleNode("Code").InnerText;
                        var tabspace = str.SelectSingleNode("TabSpace");
                        var size = str.SelectSingleNode("Size");
                        var positionFlag = str.SelectSingleNode("PositionFlag");

                        //from newest dict to oldest
                        foreach (var dict in dicts.AsQueryable().Reverse())
                        {
                            if (dict.TryGetValue(id, out var tl))
                            {
                                if (!string.IsNullOrWhiteSpace(tl.TabSpace))
                                {
                                    tabspace.InnerText = tl.TabSpace;
                                }
                                if (!string.IsNullOrWhiteSpace(tl.Size))
                                {
                                    size.InnerText = tl.Size;
                                }
                                if (!string.IsNullOrWhiteSpace(tl.TabSpace))
                                {
                                    positionFlag.InnerText = tl.PositionFlag;
                                }


                                if (tl.LineSplit > 0)
                                {
                                    var lines = tl.Split();
                                    for (int n = 0; n < lines.Count; n++)
                                    {
                                        var clone = str.Clone();
                                        clone.SelectSingleNode("Code").InnerText = $"{clone.SelectSingleNode("Code").InnerText}-{n}";
                                        clone.SelectSingleNode("ID").InnerText = $"{clone.SelectSingleNode("ID").InnerText}-{n}";
                                        group.AppendChild(clone);
                                    }
                                }
                                break;
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
                var encoding = node.Name.StartsWith("d_") ? Encoding.UTF8 : EncodingHelper.Shift_JIS;

                byte[] newData = null;

                var patchfile = Path.Combine(patchDir, node.Path);

                patchfile = patchfile.Replace(".lua", $".{languageCode}.lua");

                if (File.Exists(patchfile)) //patch file
                {
                    var txt = File.ReadAllText(patchfile);
                    newData = txt.ToBytes(encoding);
                }
                else
                {
                    //override script with common file
                    var filename = Path.GetFileName(Path.GetFileName(patchfile));
                    var commonfile = Directory //allow for subdirectories
                        .EnumerateFiles(commonDir, "*.*", SearchOption.AllDirectories)
                        .Where(i => Path.GetFileName(i) == filename)
                        .FirstOrDefault();

                    if (File.Exists(commonfile))
                    {
                        var txt = File.ReadAllText(commonfile);
                        newData = txt.ToBytes(encoding);
                        patchfile = commonfile;
                    }
                }

                //code inserts
                {
                    var insertsFiles = Directory.EnumerateFiles(commonDir, $"{Path.GetFileNameWithoutExtension(node.Name)}.{languageCode}.*.lua", SearchOption.AllDirectories)
                        .Select(path => new
                        {
                            text = File.ReadAllText(path),
                            line = int.Parse(path.Split(".")[^2])
                        })
                        //add from bottom to top, to simplify processing
                        .OrderByDescending(i => i.line)
                        .ToList();

                    if (insertsFiles.Any())
                    {
                        var lines = (newData ?? node.BinaryData).AsSpan()
                            .ToDecodedString(encoding)
                            .Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
                            .ToList();

                        foreach (var insert in insertsFiles.Where(i => i.line > 0))
                        {
                            lines.Insert(insert.line - 1, insert.text);
                        }
                        foreach (var insert in insertsFiles.Where(i => i.line < 0))
                        {
                            var replacementLines = insert.text
                                .Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

                            var start = Math.Abs(insert.line) - 1;
                            for (int i = 0; i < replacementLines.Length; i++)
                            {
                                lines[start+i] = replacementLines[i];
                            }
                        }

                        var moddedText = string.Join(Environment.NewLine, lines);

                        newData = moddedText.ToBytes(encoding);
                    }
                }

                if (newData != null)
                {
                    //try to match patch file length
                    if (newData.Length > node.BinaryData.Length)
                    {
                        var oldText = node.BinaryData.AsSpan().ToDecodedString(encoding);
                        //dictionaries are in utf8, rest seem to be in shiftjis

                        var dataReadAsText = newData.AsSpan().ToDecodedString(encoding);

                        var trimmedscript = dataReadAsText.MinifyLua();

                        newData = trimmedscript.ToBytes(encoding);

                        //padd spaces to match length without tweaking .arc for nicer partial Riivolution patches
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

