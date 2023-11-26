using CommandLine;
using InMemoryBinaryFile.Helpers;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static InMemoryBinaryFile.Helpers.EncodingHelper;
using static Program.PatchModel;

internal class Program
{
    [Verb("extract", HelpText = "Finds strings in binary file")]
    public class ExtractOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source file or directory")]
        public string SourcePath { get; set; }
        [Option('f', "filter", Required = false, HelpText = "File filter for when source is directory", Default = "*")]
        public string SourceFilter { get; set; }
        [Option('r', "recursive", Required = false, HelpText = "Enables recursive (in-depth) parsing of source directory")]
        public bool Recursive { get; set; }

        [Option('p', "patch", Required = true, HelpText = "Output for patch file(s)")]
        public string PatchPath { get; set; }
        [Option('e', "encoding", Required = true, HelpText = "Encoding of text in input file(s), eg. shift_jis or utf-16")]
        public string Encoding { get; set; }
        [Option('v', "verbose", Required = false, HelpText = "Dumps results to console")]
        public bool Verbose { get; set; }

        [Option('p', "pattern", Required = true, HelpText = "Regex pattern to locate text, text value should be named group text, eg (?<text>.*?)")]
        public IEnumerable<string> Patterns { get; set; }

        [Option('m', "merge", Required = false, HelpText = "Result JSON will be merged into single file")]
        public bool MergeResults { get; set; }

        [Option('i', "index", Required = false, HelpText = "Fills Replacement string with easily searchable incremental indexes")]
        public bool AutoIndexer { get; set; }

        [Option('j', "indexformat", Required = false, HelpText = "Formats indexer into whatever you want")]
        public string IndexFormat { get; set; }
    }

    [Verb("patch", HelpText = "Patches strings in binary file")]
    public class PatchOptions
    {
        [Option('s', "source", Required = true, HelpText = "Source file or directory")]
        public string SourcePath { get; set; }
        [Option('f', "filter", Required = false, HelpText = "File filter for when source is directory", Default = "*")]
        public string SourceFilter { get; set; }
        [Option('r', "recursive", Required = false, HelpText = "Enables recursive (in-depth) parsing of source directory")]
        public bool Recursive { get; set; }

        [Option('p', "patch", Required = true, HelpText = "Patch json file")]
        public string PatchPath { get; set; }
        [Option('o', "output", Required = false, HelpText = "Output file")]
        public string OutputPath { get; set; }
        [Option('e', "encoding", Required = true, HelpText = "Encoding of text in input file(s)")]
        public string Encoding { get; set; }
        [Option('v', "verbose", Required = false, HelpText = "Dumps results to console")]
        public bool Verbose { get; set; }

        [Option('c', "padvalue", Required = false, Default = 0, HelpText = "Padding character ascii code (decimal), defaults to null (0)")]
        public byte PadValue { get; set; }
    }
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExtractOptions))] //prevent codetrim, CLI uses reflections
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PatchOptions))] //prevent codetrim, CLI uses reflections
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PatchModel))] //prevent codetrim, NSJ uses reflections
    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;
        Console.InputEncoding = Encoding.Unicode;

        Parser.Default.ParseArguments<ExtractOptions, PatchOptions>(args)
            .WithParsed((Action<ExtractOptions>)(options =>
            {
                if (!EncodingHelper.TryParse(options.Encoding, out var encoding))
                {
                    throw new ArgumentException($"Unrecognized encoding '{options.Encoding}'");
                }
                EncodingHelper.ConsoleEncoding(encoding);

                var result = FileEx.ForEachFile(
                            options.SourcePath,
                            options.PatchPath,
                            options.SourceFilter,
                            options.Recursive,
                            (i, o) => ParseFile(i, o, encoding, options.Verbose, options.Patterns, options.AutoIndexer, options.IndexFormat))
                    .ToDictionary(i => PathEx.GetRelativePath(options.SourcePath, i.path), i => i.patch);

                if (options.MergeResults)
                {
                    Directory.CreateDirectory(options.PatchPath);
                    var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                    File.WriteAllText(options.PatchPath, json);
                }
                else
                {
                    foreach (var kvp in result)
                    {
                        var outfile = Path.Combine(options.PatchPath, kvp.Key) + ".json";
                        Directory.CreateDirectory(Path.GetDirectoryName(outfile));

                        var json = JsonConvert.SerializeObject(kvp.Value, Formatting.Indented);
                        File.WriteAllText(outfile, json);
                    }
                }
            }))
            .WithParsed((Action<PatchOptions>)(options =>
            {
                if (!EncodingHelper.TryParse(options.Encoding, out var encoding))
                {
                    throw new ArgumentException($"Unrecognized encoding '{options.Encoding}'");
                }
                EncodingHelper.ConsoleEncoding(encoding);

                Dictionary<string, PatchModel> combinedPatch = new Dictionary<string, PatchModel>();
                if (File.Exists(options.PatchPath))
                {
                    combinedPatch = JsonConvert.DeserializeObject<Dictionary<string, PatchModel>>(
                        File.ReadAllText(options.PatchPath)
                        )
                        .ToDictionary(i => i.Key, i => i.Value, StringComparer.InvariantCultureIgnoreCase);
                }

                bool TryGetPatch(string sourceFile, out PatchModel patch)
                {
                    if (combinedPatch.TryGetValue(sourceFile, out patch))
                    {
                        return true;
                    }

                    var patchPath = PathEx.RebasePath(sourceFile, options.SourcePath, options.PatchPath) + ".json";

                    if (File.Exists(patchPath))
                    {
                        patch = JsonConvert.DeserializeObject<PatchModel>(
                            File.ReadAllText(patchPath)
                            );
                        return true;
                    }

                    return false;
                }

                FileEx.ForEachFile(
                            options.SourcePath,
                            options.OutputPath,
                            options.SourceFilter,
                            options.Recursive,
                            (i, o) =>
                            {
                                if (TryGetPatch(i, out var patch))
                                {
                                    PatchFile(i, o, patch, encoding, options.Verbose, options.PadValue);
                                }
                            });
            }));
    }

    static void PatchFile(string input, string output, PatchModel patch, Encoding encoding, bool verbose, byte padValue)
    {
        if (verbose)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine(input);
            Console.WriteLine("=================================================================");
        }

        var bytes = File.ReadAllBytes(input);

        foreach (var entry in patch.Entries)
        {
            if (entry.OriginalText == entry.Replacement)
            {
                continue;
            }

            if (verbose)
            {
                Console.WriteLine($"[{entry.PositionHex}] #{entry.Index}");
                Console.WriteLine(entry.OriginalText);
                Console.WriteLine(entry.Replacement);
                Console.WriteLine("=================================================================");
            }

            var replacementBytes = entry.Replacement.ToBytes(encoding);

            if (replacementBytes.Length > entry.ByteLengthDec)
            {
                throw new Exception($"Length mismatch at #{entry.Index}! {replacementBytes.Length} > {entry.ByteLengthDec}");
            }

            if (replacementBytes.Length < entry.ByteLengthDec)
            {
                replacementBytes = replacementBytes.PadRight(entry.ByteLengthDec - replacementBytes.Length, padValue).ToArray();
            }

            bytes = bytes.Update(entry.PositionDec, replacementBytes);
        }

        File.WriteAllBytes(output, bytes);
    }


    private static (string path, PatchModel patch) ParseFile(string input, string output, Encoding encoding, bool verbose, IEnumerable<string> patterns, bool AutoIndexer, string IndexMask)
    {
        if (verbose)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine(input);
            Console.WriteLine("=================================================================");
        }
        PatchModel result = new PatchModel()
        {
        };

        var bytes = File.ReadAllBytes(input);
        var str = bytes.AsSpan().ToDecodedString(encoding);

        int n = 0;
        foreach (var pattern in patterns)
        {
            Regex regex = new Regex(pattern, RegexOptions.Compiled);

            foreach (Match match in regex.Matches(str))
            {
                if (!match.Groups.ContainsKey("text")) continue;

                var txt = match.Groups["text"];

                var origBytes = txt.Value.ToBytes(encoding);

                var pos = str
                    .Substring(0, txt.Index)
                    .ToBytes(encoding)
                    .Length;

                var entry = new StringEntry()
                {
                    Index = n,

                    OriginalText = txt.Value,
                    OriginalBytes = origBytes.ToHexString(-1),

                    ByteLengthDec = origBytes.Length,
                    ByteLengthHex = $"0x{origBytes.Length:X8}",

                    PositionDec = pos,
                    PositionHex = $"0x{pos:X8}",

                    Replacement = txt.Value,
                };

                if (AutoIndexer)
                {
                    if (!string.IsNullOrWhiteSpace(IndexMask))
                    {
                        entry.Replacement = string.Format(IndexMask, n);
                    }
                    else
                    {
                        entry.Replacement = $"|{n}|";
                    }
                }
                n++;

                if (verbose)
                    Console.WriteLine($"[{entry.PositionHex}] {entry.OriginalText}");

                result.Entries.Add(entry);
            }
        }

        return (input, result);
    }

    public class PatchModel
    {
        public List<StringEntry> Entries { get; set; } = new List<StringEntry>();
        public class StringEntry
        {
            public int Index { get; set; }

            public int PositionDec { get; set; }
            public string PositionHex { get; set; }

            public int ByteLengthDec { get; set; }
            public string ByteLengthHex { get; set; }

            public string OriginalText { get; set; }
            public string Replacement { get; set; }

            public string OriginalBytes { get; set; }
        }
    }
}