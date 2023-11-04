namespace System.IO
{
    public static partial class PathEx
    {
        static char[] allPathSeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public static string RemoveDuplicateSeparators(string path)
        {
            foreach(var separator in allPathSeparators)
            {
                path = path.Replace(new string(separator, 2), separator.ToString());
            }
            return path;
        }

        public static List<string> RecursiveSearch(string wildcardPath)
        {
            if (File.Exists(wildcardPath))
            {
                return new List<string> { wildcardPath };
            }
            if (Directory.Exists(wildcardPath))
            {
                return Directory.EnumerateFiles(wildcardPath, "*", SearchOption.AllDirectories).ToList();
            }

            var segments = wildcardPath.Split(allPathSeparators, StringSplitOptions.RemoveEmptyEntries);

            var path = "";
            List<string> directories = new List<string>();

            //gather directories
            foreach(var segment in segments[0..^1])
            {
                if (PathEx.IsWildcard(segment))
                {
                    if (!directories.Any())
                    {
                        directories = Directory
                            .EnumerateDirectories(path, segment, SearchOption.AllDirectories)
                            .ToList();
                    }
                    else
                    {
                        directories = directories
                            .SelectMany(dir => Directory
                                .EnumerateDirectories(Path.Join(dir, path), segment, SearchOption.AllDirectories)
                            ).ToList();
                    }
                    path = "";
                }
                else
                {
                    path = Path.Join(path, segment);
                }
            }

            var result = directories
                .SelectMany(dir => Directory.EnumerateFiles(dir, segments[^1], SearchOption.AllDirectories))
                .ToList();

            return result;
        }

        public static bool IsWildcard(string path)
        {
            return path.Contains("*") || path.Contains("?");
        }

        /// <summary>
        /// Wraps Path.GetRelativePath to provide some useful functions
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="path"></param>
        /// <param name="doNothingForSamePath">returns path without change if its same as relativeTo</param>
        /// <returns></returns>
        public static string GetRelativePath(string relativeTo, string path, bool doNothingForSamePath = true)
        {
            if (doNothingForSamePath && string.Equals(relativeTo, path, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
            return Path.GetRelativePath(relativeTo, path);
        }

        /// <summary>
        /// Wrapper for
        /// Path.Join(newParent, Path.GetRelativePath(oldParent, path))
        /// </summary>
        /// <param name="path"></param>
        /// <param name="oldParent"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public static string RebasePath(string path, string oldParent, string newParent)
        {
            var stub = Path.GetRelativePath(oldParent, path);
            return Path.Join(newParent, stub);
        }
    }
}