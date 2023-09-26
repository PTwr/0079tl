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

    [Fact]
    public void ArcAplyPatch()
    {
        var files = new System.IO.DirectoryInfo(@"C:\games\wii\0079\0079_jp\DATA")
            .GetFiles("*.arc", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var arcjp = file.FullName;
            var patchDir = arcjp.Replace(@"C:\games\wii\0079\0079_jp", @"../../../../Patcher/Translation/Patch");
            var unpackeddir = arcjp.Replace(@"C:\games\wii\0079\0079_jp", @"C:\games\wii\0079\0079_unpacked");

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            if(arcjp.Contains("BR_ME02_text.arc"))
            {

            }

            var updated = UpdateU8Root(root, patchDir, unpackeddir);

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
    private bool UpdateU8Root(U8RootSegment root, string dumpDir, string unpackeddir)
    {
        var windowjson = File.ReadAllText("../../../../Patcher/Translation/Translationdict.json");
        var windowdict = JsonConvert.DeserializeObject<List<windowjsonentry>>(windowjson)
            .ToDictionary(i=>i.ID, i=>i.Text);

        bool updated = false;
        int offsetChange = 0;
        foreach (var node in root.Nodes)
        {
            if (node.IsArc)
            {
                var nestedRoot = new U8RootSegment();
                nestedRoot.Parse(node.BinaryData);
                updated |= UpdateU8Root(nestedRoot, Path.Combine(dumpDir, node.Path), Path.Combine(unpackeddir, node.Path));

                var newData = nestedRoot.GetBytes().ToArray();
                offsetChange -= node.BinaryData.Length;
                offsetChange += newData.Length;

                node.BinaryData = newData;
                node.Size = newData.Length;
            }
            else if (node.IsXbf)
            {
                var xmlenpath = Path.Combine(dumpDir, node.Path);
                xmlenpath = xmlenpath.Replace(".xbf", ".xbf.en.xml");

                if (!File.Exists(xmlenpath))
                {
                    //for Window.arc
                    //if (xmlenpath.Contains("Window.arc") && xmlenpath.Contains("BlockText.xbf"))
                    {
                        //patch even without translation because we use shared dict
                        xmlenpath = Path.Combine(unpackeddir, node.Path) + ".xml";
                    }
                }
                if (xmlenpath.Contains("Briefing_Select.arc") && xmlenpath.Contains("BlockText"))
                {

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

                            if (windowdict.TryGetValue(id, out var tl))
                            {
                                block.ChildNodes[1].InnerText = tl;
                            }
                        }
                    }

                    var parsed = new XbfRootSegment(doc, XbfRootSegment.ShouldBeUTF8(xmlenpath));

                    var newData = parsed.GetBytes().ToArray();

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;

                    updated = true;
                }
            }
            else if (node.IsFile && node.Name.EndsWith(".lua"))
            {
                var luapath = Path.Combine(dumpDir, node.Path);
                luapath = luapath.Replace(".lua", ".en.lua");
                if (File.Exists(luapath))
                {
                    //var lua = File.ReadAllText(luapath);

                    var newData = File.ReadAllBytes(luapath);

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;

                    updated = true;
                }
            }
            else if (node.IsFile && node.Name.EndsWith(".xml"))
            {
                var path = Path.Combine(dumpDir, node.Path);
                path = path.Replace(".xml", ".en.xml");
                if (File.Exists(path))
                {
                    //var lua = File.ReadAllText(path);

                    var newData = File.ReadAllBytes(path);

                    offsetChange -= node.BinaryData.Length;
                    offsetChange += newData.Length;

                    node.BinaryData = newData;
                    node.Size = newData.Length;

                    updated = true;
                }
            }
        }
        return updated;
    }
}