using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class FileEx
    {
        public static IEnumerable<T> ForEachFile<T>(IEnumerable<string> inputs, string output, string filter, bool recursive, Func<string, string, T> action, string outputSuffix = "", string? inputRoot = null)
        {
            foreach (var input in inputs)
            {
                foreach (var result in ForEachFile<T>(input, output, filter, recursive, action, outputSuffix, inputRoot))
                {
                    yield return result;
                }
            }
        }
        public static void ForEachFile(IEnumerable<string> inputs, string output, string filter, bool recursive, Action<string, string> action, string outputSuffix = "", string? inputRoot = null)
        {
            foreach (var input in inputs)
            {
                ForEachFile(input, output, filter, recursive, action, outputSuffix, inputRoot);
            }
        }
        public static IEnumerable<T> ForEachFile<T>(string input, string output, string filter, bool recursive, Func<string, string, T> action, string outputSuffix = "", string? inputRoot = null)
        {
            if (File.Exists(input))
            {
                if (!string.IsNullOrWhiteSpace(inputRoot))
                {
                    output = PathEx.RebasePath(input, inputRoot, output) + outputSuffix;
                }
                yield return action(input, output + outputSuffix);
            }
            else if (Directory.Exists(input))
            {
                foreach (var file in Directory.EnumerateFiles(input, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    yield return action(file, PathEx.RebasePath(file, inputRoot ?? input, output));
                }
            }
        }
        public static void ForEachFile(string input, string output, string filter, bool recursive, Action<string, string> action, string outputSuffix = "", string? inputRoot = null)
        {
            if (File.Exists(input))
            {
                if (!string.IsNullOrWhiteSpace(inputRoot))
                {
                    output = PathEx.RebasePath(input, inputRoot, output) + outputSuffix;
                }
                action(input, output + outputSuffix);
            }
            else if (Directory.Exists(input))
            {
                foreach (var file in Directory.EnumerateFiles(input, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    action(file, PathEx.RebasePath(file, inputRoot ?? input, output));
                }
            }
        }

    }
}
