using CommandLine;
using DolphinSubtitles;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;

namespace Subtitles
{
    [Verb("prepare", HelpText = "Prepares Dolphin Subtitles file(s) from source directory")]
    public class SubtitleGenerator
    {
        [Option('r', "root", Required = true, HelpText = "Directory containing unpacked disc files")]
        public string GameRoot { get; set; }
        [Option('f', "filter", Required = true, HelpText = "Sound file filter, eg. sound/stream/ain*.brstm")]
        public string Filter { get; set; }
        [Option('d', "directories", Required = false, HelpText = "Maintain directories while generating split files")]
        public bool Directories { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file or directory")]
        public string Output { get; set; }

        [Option('c', "clean", Required = false, HelpText = "Delets old files from output directory")]
        public bool Clean { get; set; }

        [Option('m', "miliseconds", Required = false, Default = 5000, HelpText = "How long subs will be displayed for (in miliseconds)")]
        public int Miliseconds { get; set; }

        [Option('c', "color", Required = false, Default = "White", HelpText = "Font color, in web (html) color name")]
        public string Color { get; set; }

        internal string OutputDir => Path.HasExtension(Output) ? Path.GetDirectoryName(Output) : Output;
        internal string OutputFile => Path.HasExtension(Output) ? Output : Path.Join(Output, "Subtitles.json");
    }

    internal class Program
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SubtitleGenerator))] //prevent codetrim, CLI uses reflections
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SubtitleEntry))] //prevent codetrim, CLI uses reflections
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SubtitleGenerator>(args)
                .WithParsed((Action<SubtitleGenerator>)(o =>
                {
                    if (!Directory.Exists(o.GameRoot))
                    {
                        Console.WriteLine("Invalid GameRoot directory");
                        return;
                    }

                    //drop old files if needed
                    if (o.Clean && Directory.Exists(o.OutputDir))
                    {
                        Directory.Delete(o.OutputDir, true);
                    }

                    //ensure directory exists
                    Directory.CreateDirectory(o.OutputDir);

                    var files = PathEx.RecursiveSearch(o.Filter, o.GameRoot);

                    List<SubtitleEntry> entries = new List<SubtitleEntry>();

                    foreach (var file in files)
                    {
                        Console.WriteLine(file);
                        entries.Add(new SubtitleEntry()
                        {
                            Scale = 1,
                            AllowDuplicate = false,
                            Enabled = true,
                            FileName = PathEx.GetRelativePath(o.GameRoot, file).Replace("\\", "/"),
                            Color = o.Color,
                            Miliseconds = (uint)o.Miliseconds,
                            Translation = PathEx.GetRelativePath(o.GameRoot, file).Replace("\\", "/"),
                        });
                    }

                    File.WriteAllText(o.OutputFile, JsonConvert.SerializeObject(entries, Formatting.Indented));
                }));
        }
    }
}
