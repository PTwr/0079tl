using CommandLine;
using RiivolutionPPG;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection.Metadata.Ecma335;

internal class Program
{
    private static void Main(string[] args)
    {
        Parser.Default.ParseArguments<MassDifferentialPatchOptions, DifferentialPatchOptions>(args)
            .WithParsed<MassDifferentialPatchOptions>(o =>
            {
                if (!Directory.Exists(o.Unmodified))
                {
                    Console.WriteLine("Invalid unmodified directory");
                    return;
                }
                if (!Directory.Exists(o.Modified))
                {
                    Console.WriteLine("Invalid modified directory");
                    return;
                }
                MassDifferentialPatch(o);
            })
            .WithParsed<DifferentialPatchOptions>(o =>
            {
                if (!File.Exists(o.Unmodified))
                {
                    Console.WriteLine("Invalid unmodified file");
                    return;
                }
                if (!File.Exists(o.Modified))
                {
                    Console.WriteLine("Invalid modified file");
                    return;
                }
                DifferentialPatch(o);
            });
    }
    public static void MassDifferentialPatch(MassDifferentialPatchOptions options)
    {
        List<wiidiscPatchFile> fileentries = new List<wiidiscPatchFile>();

        Dictionary<string, Dictionary<int, int>> alldiffs = new Dictionary<string, Dictionary<int, int>>();
        foreach (var file in Directory.EnumerateFiles(options.Unmodified, options.Search, SearchOption.AllDirectories))
        {
            var pathstub = Path.GetRelativePath(options.Unmodified, file);
            var modfile = Path.Combine(options.Modified, pathstub);

            if (!File.Exists(modfile)) continue;

            var original = File.ReadAllBytes(file);
            var modified = File.ReadAllBytes(modfile);

            if (original.SequenceEqual(modified)) continue;

            var diffs = GetDiffSegments(original, modified, options.Tolerance, options.Alignment);

            if (!diffs.Any()) continue;

            alldiffs[pathstub] = diffs;

            fileentries.AddRange(GeneratePatchXmlNodes(pathstub, diffs, options.Name));

            if (options.OnlyXml) continue;

            if (diffs.ContainsKey(0x0018289A))
            {

            }

            foreach (var diff in diffs)
            {
                var path = Path.Combine(options.OutputDirectory, options.Name, $"{pathstub}.chunks", $"0x{diff.Key:X8}.bin");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, modified.AsSpan(diff.Key, diff.Value).ToArray());
            }
        }

        if (!alldiffs.Any()) return;

        //"Paste XML as classes" is disgusting :D
        wiidisc xml = new wiidisc()
        {
            version = 1,
            id = new wiidiscID() { game = "R79" },
            options = new wiidiscOptions()
            {
                section = new wiidiscOptionsSection()
                {
                    name = "Differential patch",
                    option = new wiidiscOptionsSectionOption()
                    {
                        name = "Differential patch",
                        choice = new wiidiscOptionsSectionOptionChoice()
                        {
                            name = "EN",
                            patch = new wiidiscOptionsSectionOptionChoicePatch()
                            {
                                id = "diffpatch_EN",
                            }
                        }
                    }
                }
            },
            patch = new wiidiscPatch()
            {
                id = "diffpatch_EN",
                file = fileentries.ToArray(),
            }
        };

        SerializeRiivolutionPatch(options.OutputDirectory, xml, options.Name);
    }

    private static IEnumerable<wiidiscPatchFile> GeneratePatchXmlNodes(string pathstub, Dictionary<int, int> patches, string patchname)
    {
        pathstub = pathstub.Replace('\\', '/');
        return patches.Select(p =>
                        new wiidiscPatchFile()
                        {
                            disc = "/" + pathstub.TrimStart('/'),
                            external = $"{patchname}/{pathstub}.chunks/0x{p.Key:X8}.bin",
                            resize = false,
                            create = false,
                            offset = $"0x{p.Key:X8}",
                            length = $"0x{p.Value:X8}",
                        });
    }

    public static Dictionary<int, int> GetDiffSegments(byte[] original, byte[] modified, int tolerance = 16, int alignment = 16)
    {
        //TODO deduplicate bin chunks
        Dictionary<int, int> patches = new Dictionary<int, int>();

        var end = Math.Min(original.Length, modified.Length);
        for (int i = 0; i < end; i++)
        {
            //search for modified bytes
            if (original[i] == modified[i]) continue;

            //store offset;
            int offset = i;
            int lastModified = i;

            for (int j = i + 1; j < end; j++)
            {
                //search for unmodified bytes this time
                if (original[j] != modified[j])
                {
                    lastModified = j;
                    continue;
                };

                //if tollerance length ran out
                if (j - lastModified > tolerance)
                {
                    //give up on merging modified chunks that are spread too far
                    break;
                }
            }

            //alignt to offset
            offset = offset / alignment * alignment;

            //if aligned offset overlaps with previous, merge them
            if (patches.Any() && patches.Last().Key >= offset)
            {
                var previousOffset = patches.Last().Key;
                //update length
                patches[previousOffset] = lastModified - offset + 1;
            }
            else
            {
                //insert new offset
                patches.Add(offset, lastModified - offset + 1);
            }

            //fast forward priamry loop
            i = lastModified;
        }

        //if modified file is longer, add one final patch section
        if (modified.Length > original.Length)
        {
            //after the end of original file
            patches[original.Length + 1] =
                //add whatever's left over
                modified.Length - original.Length - 1;
        }

        return patches;
    }

    public static void DifferentialPatch(DifferentialPatchOptions options, int alignment = 16)
    {
        Dictionary<int, int> patches = new Dictionary<int, int>();

        var original = File.ReadAllBytes(options.Unmodified);
        var modified = File.ReadAllBytes(options.Modified);

        var chunksDir = Path.Combine(options.OutputDirectory, "chunks");

        Directory.CreateDirectory(chunksDir);

        var end = Math.Min(original.Length, modified.Length);
        for (int i = 0; i < end; i++)
        {
            //search for modified bytes
            if (original[i] == modified[i]) continue;

            //store offset;
            int offset = i;
            int lastModified = i;

            for (int j = i + 1; j < end; j++)
            {
                //search for unmodofied bytes this time
                if (original[j] != modified[j])
                {
                    lastModified = j;
                    continue;
                };

                //if tollerance length ran out
                if (j - lastModified > options.Tolerance)
                {
                    //give up on merging modified chunks that are spread too far
                    break;
                }
            }

            patches[offset] = lastModified - offset + 1; //length is 1 based :)

            //fast forward priamry loop
            i = lastModified;
        }

        //if modified file is long, add one final patch section
        if (modified.Length > original.Length)
        {
            //after the end of original file
            patches[original.Length + 1] =
                //add whatever's left over
                modified.Length - original.Length;
        }

        //"Paste XML as classes" is disgusting :D
        wiidisc xml = new wiidisc()
        {
            version = 1,
            id = new wiidiscID() { game = "R79" },
            options = new wiidiscOptions()
            {
                section = new wiidiscOptionsSection()
                {
                    name = "Differential patch",
                    option = new wiidiscOptionsSectionOption()
                    {
                        name = "Differential patch",
                        choice = new wiidiscOptionsSectionOptionChoice()
                        {
                            name = "EN",
                            patch = new wiidiscOptionsSectionOptionChoicePatch()
                            {
                                id = "diffpatch_EN",
                            }
                        }
                    }
                }
            },
            patch = new wiidiscPatch()
            {
                id = "diffpatch_EN",
                file = GeneratePatchXmlNodes(Path.GetFileName(options.Unmodified), patches, "R79JAF_diffpatch").ToArray(),
            }
        };

        if (!options.OnlyXml)
        {
            foreach (var kvp in patches)
            {
                var filename = Path.Combine(chunksDir, $"0x{kvp.Key:X8}.bin");
                var bytes = modified.AsSpan(kvp.Key, kvp.Value);
                File.WriteAllBytes(filename, bytes.ToArray());
            }
        }

        SerializeRiivolutionPatch(options.OutputDirectory, xml, "R79JAF_diffpatch");
    }

    private static void SerializeRiivolutionPatch(string outputDirectory, wiidisc xml, string patchname)
    {
        XmlSerializer xsSubmit = new XmlSerializer(typeof(wiidisc));

        using (var sww = new StringWriter())
        {
            using (XmlWriter writer = XmlWriter.Create(sww, new XmlWriterSettings()
            {
                Indent = true,
            }))
            {
                xsSubmit.Serialize(writer, xml);
                var s = sww.ToString(); // Your XML
                File.WriteAllText(Path.Combine(outputDirectory, $"{patchname}.xml"), s);
            }
        }
    }
}

[Verb("massdiff", HelpText = "Generates Riivolution patch from differences between directories.")]
public class MassDifferentialPatchOptions
{
    [Option('n', "name", Required = false, HelpText = "Patch name.", Default = "R79JAF_diffpatch")]
    public string Name { get; set; }

    [Option('s', "search", Required = false, HelpText = "Search mask for files to patch.", Default = "*.arc")]
    public string Search { get; set; }

    [Option('u', "unmodifed", Required = true, HelpText = "Original, unmodified, directory as found on game disc.")]
    public string Unmodified { get; set; }
    [Option('m', "modified", Required = true, HelpText = "Modified directory.")]
    public string Modified { get; set; }
    [Option('t', "tolerance", Required = false, HelpText = "Number of unmodified bytes between modified sections that will be included in patch.", Default = 16)]
    public int Tolerance { get; set; }
    [Option('a', "alignment", Required = false, HelpText = "Byte alignment of patch offsets.", Default = 16)]
    public int Alignment { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output directory for patch files")]
    public string OutputDirectory { get; set; }
    [Option("onlyxml", Required = false, HelpText = "Skip writing out binary chunks for patch.", Default = false)]
    public bool OnlyXml { get; set; }
}

[Verb("differential", HelpText = "Generates Riivolution patch from differences between files.")]
public class DifferentialPatchOptions
{
    [Option('u', "unmodifed", Required = true, HelpText = "Original, unmodified, file as found on game disc.")]
    public string Unmodified { get; set; }
    [Option('m', "modified", Required = true, HelpText = "Modified file.")]
    public string Modified { get; set; }
    [Option('t', "tolerance", Required = false, HelpText = "Number of unmodified bytes between modified sections that will be included in patch.", Default = 16)]
    public int Tolerance { get; set; }

    [Option('o', "output", Required = true, HelpText = "Output directory for patch files")]
    public string OutputDirectory { get; set; }
    [Option("onlyxml", Required = false, HelpText = "Skip writing out binary chunks for patch.", Default = false)]
    public bool OnlyXml { get; set; }
}