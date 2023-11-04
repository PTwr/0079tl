using CommandLine;
using Newtonsoft.Json;
using DeepL;
using static Program;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FileProcessing;

internal class Program
{
    public abstract class Options
    {
        [Option('k', "key", Required = true, HelpText = "DeepL API key or path to key file")]
        public string DeepLApiKeyOKeyFilePath { get; set; }

        //TODO fork CommandLine to make it use string->class cast operators?
        public StringOrFileContent DeepLKey => new StringOrFileContent(DeepLApiKeyOKeyFilePath);

        [Option("sourcelang", Required = true, HelpText = "DeepL API key or path to key file", Default = null)]
        public string SourceLanguageCode { get; set; }
        [Option("targetlang", Required = false, HelpText = "DeepL API key or path to key file", Default = "en-US")]
        public string TargetLanguageCode { get; set; }
    }

    [Verb("json")]
    public class OptionsJSON : Options
    {
        [Option('i', "input", Required = true, HelpText = "Input JSON file or directory")]
        public string Input { get; set; }
        [Option('f', "filter", Required = false, HelpText = "Input file filter, eg *.json, for when input is directory")]
        public string Filter { get; set; }
        [Option('o', "output", Required = true, HelpText = "Output JSON file")]
        public string Output { get; set; }
        [Option("suffix", Required = false, HelpText = "Output JSON file suffix", Default = "")]
        public string OutputSuffix { get; set; }

        public IOFile IOFile => new IOFile(Input, Output, Filter);

        [Option('p', "path", Required = true, HelpText = "JSON Path to translated field")]
        public string JsonPath { get; set; }

        [Option('t', "terminator", Required = false, HelpText = "Sentence terminator, eg. japanese dot 。", Default = ".")]
        public string SentenceTerminator { get; set; }

        [Option('s', "split", Required = false, HelpText = "Split text by Terminator and translates each sentence separately")]
        public bool SplitSentences { get; set; }

        //public OptionsJSON()
        //{
        //    Input = "";
        //    Output = "";
        //    JsonPath = "";
        //    SentenceTerminator = "";
        //    SplitSentences = false;
        //}
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OptionsJSON))] //prevent codetrim, CLI uses reflections
    private static void Main(string[] args)
    {
        var pp = new Parser(settings =>
        {
            settings.AllowMultiInstance = true;
            settings.EnableDashDash = true;
        });
        Parser.Default.ParseArguments<OptionsJSON>(args)
            .WithParsed<OptionsJSON>(o =>
            {
                var tl = new Translator(
                    o.DeepLKey,
                    new TranslatorOptions() { sendPlatformInfo = false });

                var files = o.IOFile;

                files.ForEach((input, output) =>
                {
                    var json = File.ReadAllText(input).Trim();
                    IEnumerable<JToken> tokens = null;
                    if (json.StartsWith("["))
                    {
                        var jobject = JArray.Parse(json);

                        //dont bother with InsertOrUpdate
                        jobject.SelectTokens("$..DeepL_mtl").ToList().ForEach(t => t.Parent.Remove());

                        tokens = jobject.SelectTokens(o.JsonPath);

                    }
                    else if (json.StartsWith("{"))
                    {
                        var jobject = JArray.Parse(json);

                        //dont bother with InsertOrUpdate
                        jobject.SelectTokens("$..DeepL_mtl").ToList().ForEach(t => t.Parent.Remove());

                        tokens = jobject.SelectTokens(o.JsonPath);
                    }
                    else
                    {
                        throw new Exception("Can't parse that json as of yet :(");
                    }

                    foreach (var token in tokens)
                    {
                        string str = "";

                        switch (token.Type)
                        {
                            case JTokenType.Array:
                                var lines = token.Values().Select(t => t.ToString());
                                str = string.Join(null, lines);
                                break;
                            case JTokenType.String:
                                str = token.ToString();
                                break;
                            case JTokenType.None:
                            case JTokenType.Object:
                            case JTokenType.Constructor:
                            case JTokenType.Property:
                            case JTokenType.Comment:
                            case JTokenType.Integer:
                            case JTokenType.Float:
                            case JTokenType.Boolean:
                            case JTokenType.Null:
                            case JTokenType.Undefined:
                            case JTokenType.Date:
                            case JTokenType.Raw:
                            case JTokenType.Bytes:
                            case JTokenType.Guid:
                            case JTokenType.Uri:
                            case JTokenType.TimeSpan:
                            default:
                                throw new Exception($"Unsuported jtoken type {token.Type}");
                        }

                        List<string> sentences = new List<string>();

                        if (o.SplitSentences)
                        {
                            sentences.AddRange(
                                str.Split(o.SentenceTerminator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Select(s => s + o.SentenceTerminator));
                        }
                        else
                        {
                            sentences.Add(str);
                        }

                        List<string> translation = new List<string>();

                        foreach (var sentence in sentences)
                        {
                            var s = tl.TranslateTextAsync(sentence, LanguageCode.Japanese, LanguageCode.EnglishAmerican).Result.Text;
                            translation.Add(s);
                        }

                        var jprop = new JProperty("DeepL_mtl", translation);

                        token //text field or line array
                            .Parent //text grouping
                            .AddAfterSelf(jprop);
                    }

                    json = tokens.First().Root.ToString(Formatting.Indented);

                    File.WriteAllText(output, json);
                });
            });

        Console.WriteLine("Hello, World!");
    }


}