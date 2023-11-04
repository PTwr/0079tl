using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace FileProcessing
{
    public class StringOrFileContent
    {
        private readonly string pathOrValue;

        public StringOrFileContent(string pathOrValue) 
        {
            this.pathOrValue = pathOrValue;
        }

        public static implicit operator string(StringOrFileContent reader)
        {
            return reader.ToString();
        }
        public override string ToString()
        {
            if (File.Exists(this.pathOrValue))
            {
                return File.ReadAllText(this.pathOrValue);
            }
            return this.pathOrValue;
        }
    }
}
