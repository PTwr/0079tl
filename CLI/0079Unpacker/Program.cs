using CommandLine;
using CommandLine.Text;
using U8;

internal class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Directory containing game files")]
        public string InputDir { get; set; }
        [Option('o', "output", Required = true, HelpText = "Where files will be unpacked to")]
        public string OutputDir { get; set; }
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

                ArcUnpackEverything(o.InputDir, o.OutputDir);
            });
    }

    public static void ArcUnpackEverything(string inputDir, string outputDir)
    {
        var files = new System.IO.DirectoryInfo(inputDir)
            .GetFiles("*.arc", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var arcjp = file.FullName;

            var bytes = File.ReadAllBytes(arcjp).AsSpan();
            var root = new U8RootSegment();
            root.Parse(bytes);

            root.DumpToDisk(outputDir);
        }
    }
}