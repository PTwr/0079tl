using InMemoryBinaryFile.Helpers;
using Newtonsoft.Json;
using System.Xml;
using U8;
using XBFLib;

namespace Tests;

public class U8Tests
{
    [Fact]
    public void ArcUnpackEverything()
    {
        var files = new System.IO.DirectoryInfo(@"C:\games\wii\0079\0079_jp\DATA")
            .GetFiles("*.arc", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var arcjp = file.FullName;
            var dumpDir = arcjp.Replace("0079_jp", "0079_unpacked");

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            root.DumpToDisk(dumpDir);
        }
    }

    const string PatchDir = @"../../../../Patcher/Translation/Patch";
    const string TranslationDictFilename = "dict.json";
    [Fact]
    public void ArcAplyPatch()
    {
        var globaldictjson = File.ReadAllText(PatchDir + "/global." + TranslationDictFilename);
        var globaldict = JsonConvert.DeserializeObject<List<windowjsonentry>>(globaldictjson)
            .ToDictionary(i => i.ID, i => i.Text);

        var files = new System.IO.DirectoryInfo(@"C:\games\wii\0079\0079_jp\DATA")
            .GetFiles("*.arc", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var arcjp = file.FullName;
            var patchDir = arcjp.Replace(@"C:\games\wii\0079\0079_jp", PatchDir);
            var unpackeddir = arcjp.Replace(@"C:\games\wii\0079\0079_jp", @"C:\games\wii\0079\0079_unpacked");

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            var updated = UpdateU8Root(root, patchDir, unpackeddir, new List<Dictionary<string, string>>() { globaldict });

            if (updated)
            {
                var newbytes = root.GetBytes().ToArray();
                File.WriteAllBytes(file.FullName.Replace("0079_jp", "0079_en"), newbytes);
            }
        }
    }

    public class windowjsonentry
    {
        public string ID { get; set; }
        public string Text { get; set; }
    }
    private bool UpdateU8Root(U8RootSegment root, string patchDir, string unpackeddir, List<Dictionary<string, string>> dicts)
    {
        if (patchDir.Contains("BR_ME01.arc"))
        {

        }
        var dictPath = Path.Combine(patchDir, TranslationDictFilename);
        if (File.Exists(dictPath))
        {
            var nesteddictjson = File.ReadAllText(dictPath);
            var nesteddict = JsonConvert.DeserializeObject<List<windowjsonentry>>(nesteddictjson)
                .ToDictionary(i => i.ID, i => i.Text);
            //copy list, as its passed as reference not value
            dicts = new List<Dictionary<string, string>>(dicts);
            dicts.Add(nesteddict);
        }

        bool updated = false;
        int offsetChange = 0;
        foreach (var node in root.Nodes)
        {
            if (node.IsArc)
            {
                var nestedRoot = new U8RootSegment();
                nestedRoot.Parse(node.BinaryData);
                updated |= UpdateU8Root(nestedRoot, Path.Combine(patchDir, node.Path), Path.Combine(unpackeddir, node.Path), dicts);

                var newData = nestedRoot.GetBytes().ToArray();
                offsetChange -= node.BinaryData.Length;
                offsetChange += newData.Length;

                node.BinaryData = newData;
                node.Size = newData.Length;
            }
            else if (node.IsXbf)
            {
                var xmlenpath = Path.Combine(patchDir, node.Path);
                xmlenpath = xmlenpath.Replace(".xbf", ".xbf.en.xml");

                //TODO refactor
                if (!File.Exists(xmlenpath))
                {
                    if (xmlenpath.Contains("BlockText.xbf"))
                    {
                        //patch even without translation because we use shared dict
                        xmlenpath = Path.Combine(unpackeddir, node.Path) + ".xml";
                    }
                }
                if (File.Exists(xmlenpath))
                {
                    var xml = File.ReadAllText(xmlenpath);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    //if (xmlenpath.Contains("Window.arc"))
                    {
                        var blocks = doc.SelectNodes("/Texts/Block");
                        foreach (XmlElement block in blocks)
                        {
                            var id = block.ChildNodes[0].InnerText;
                            var text = block.ChildNodes[1].InnerText;

                            //from newest dict to oldest
                            foreach (var dict in dicts.AsQueryable().Reverse())
                            {
                                if (dict.TryGetValue(id, out var tl))
                                {
                                    block.ChildNodes[1].InnerText = tl;
                                    //dont look in parent dict if TL was found
                                    break;
                                }
                            }
                        }
                    }

                    var parsed = new XbfRootSegment(doc, XbfRootSegment.ShouldBeUTF8(xmlenpath));

                    var newData = parsed.GetBytes().ToArray();

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;

                    updated |= true;
                }
            }
            else if (node.IsFile && node.Name.EndsWith(".lua"))
            {
                var luapath = Path.Combine(patchDir, node.Path);
                luapath = luapath.Replace(".lua", ".en.lua");
                if (File.Exists(luapath))
                {
                    //var lua = File.ReadAllText(luapath);

                    var newData = File.ReadAllBytes(luapath);

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;

                    updated |= true;
                }
            }
            else if (node.IsFile && node.Name.EndsWith(".xml"))
            {
                var path = Path.Combine(patchDir, node.Path);
                path = path.Replace(".xml", ".en.xml");
                if (File.Exists(path))
                {
                    //var lua = File.ReadAllText(path);

                    var newData = File.ReadAllBytes(path);

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;

                    updated |= true;
                }
            }
        }
        return true;
    }
}