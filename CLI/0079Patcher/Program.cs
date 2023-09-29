using CommandLine.Text;
using CommandLine;
using U8;
using System.Xml;
using XBFLib;
using Newtonsoft.Json;

internal class Program
{
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
    }
    private static void Main(string[] args)
    {
        //if (args?.Any() == false)
        //{
        //    var h = new HelpText().AddOptions(Parser.Default.ParseArguments<Options>(args));
        //    Console.WriteLine(h.ToString());
        //    return;
        //}

        Parser.Default.ParseArguments<Options>(args)
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

                ArcPatchEerything(o.InputDir, o.OutputDir, o.PatchDir, o.LanguageCode, o.Rewrite);
            });
    }

    const string TranslationDictFilenameMask = "dict*.{0}.json";
    public static void ArcPatchEerything(string inputDir, string outputDir, string patchDir, string languageCode, bool rewrite)
    {
        //get global dicts first
        Dictionary<string, windowjsonentry> globaldict = GetTLDict(Path.Combine(patchDir, UniqueDir), languageCode);

        var files = new System.IO.DirectoryInfo(inputDir)
            .GetFiles("*.arc", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var arcjp = file.FullName;

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            var arcPath = Path.GetRelativePath(inputDir, file.FullName);
            var updated = UpdateU8Root(root, Path.Combine(patchDir, UniqueDir, arcPath), Path.Combine(patchDir, CommonDir), Path.Combine(inputDir, arcPath), languageCode, new List<Dictionary<string, windowjsonentry>>() { globaldict });

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

    private static Dictionary<string, windowjsonentry> GetTLDict(string patchDir, string languageCode)
    {
        if (!Directory.Exists(patchDir))
        {
            return new Dictionary<string, windowjsonentry>();
        }

        var paths = Directory
            .EnumerateFiles(patchDir, string.Format(TranslationDictFilenameMask, languageCode), SearchOption.TopDirectoryOnly);

        Dictionary<string, windowjsonentry> result = new Dictionary<string, windowjsonentry>();

        //allow for multi-file dicts
        foreach (var path in paths)
        {
            var json = File.ReadAllText(path);
            var dict = JsonConvert.DeserializeObject<List<windowjsonentry>>(json);

            foreach (var entry in dict)
            {
                result[entry.ID] = entry;
            }
        };

        return result;
    }

    public class windowjsonentry
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
    private static bool UpdateU8Root(U8RootSegment root, string patchDir, string commonDir, string inputDir, string languageCode, List<Dictionary<string, windowjsonentry>> dicts)
    {
        dicts = new List<Dictionary<string, windowjsonentry>>(dicts) //clone list
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
                Console.WriteLine("XBF: " + inputDir + "/" + node.Name);
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
                    xbf.Parse(node.BinaryData.AsSpan());
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

                updated |= !newData.SequenceEqual(node.BinaryData);

                offsetChange -= node.BinaryData.Length;
                offsetChange += newData.Length;

                node.BinaryData = newData;
                node.Size = newData.Length;
            }
            else if (node.IsFile && node.Name.EndsWith(".lua"))
            {
                byte[] newData = null;

                Console.WriteLine("LUA: " + inputDir + "/" + node.Name);
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
                    }
                }

                if (newData != null)
                {
                    updated |= !newData.SequenceEqual(node.BinaryData);

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

