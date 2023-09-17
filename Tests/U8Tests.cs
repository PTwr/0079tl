using InMemoryBinaryFile.Helpers;
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
    public void ArcUpdateXbf()
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

            var updated = UpdateU8Root(root, dumpDir);

            if (updated)
            {
                var newbytes = root.GetBytes().ToArray();
                File.WriteAllBytes(file.FullName.Replace("0079_jp", "0079_en"), newbytes);
            }
        }
    }

    private bool UpdateU8Root(U8RootSegment root, string dumpDir)
    {
        bool updated = false;
        int offsetChange = 0;
        foreach (var node in root.Nodes)
        {
            if (node.IsArc)
            {
                var nestedRoot = new U8RootSegment();
                nestedRoot.Parse(node.BinaryData);
                updated |= UpdateU8Root(nestedRoot, Path.Combine(dumpDir, node.Path));

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
                if (File.Exists(xmlenpath))
                {
                    var xml = File.ReadAllText(xmlenpath);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

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