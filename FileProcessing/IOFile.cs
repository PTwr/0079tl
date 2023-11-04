using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessing
{
    public class IOFile
    {
        private readonly List<string> Paths;
        private readonly string inputRootPath;
        private readonly string outputRootPath;
        private readonly string filter;

        public IOFile(string inputRootPath, string outputRootPath, string filter = "*")
        {
            Paths = PathEx.RecursiveSearch(
                PathEx.RemoveDuplicateSeparators(
                    Path.Join(inputRootPath, filter))
                );
            this.inputRootPath = inputRootPath;
            this.outputRootPath = outputRootPath;
            this.filter = filter;
        }

        public bool Exists()
        {
            return Paths.All(p => File.Exists(p) || Directory.Exists(p));
        }

        public void ForEach(Action<string> action, bool recursive = true, string outputSuffix = "")
        {
            FileEx.ForEachFile(Paths, outputRootPath, filter, recursive, (i, o) => action(i), outputSuffix, inputRootPath);
        }
        public void ForEach(Action<string, string> action, bool recursive = true, string outputSuffix = "")
        {
            FileEx.ForEachFile(Paths, outputRootPath, filter, recursive, action, outputSuffix, inputRootPath);
        }
    }
}
