using System.Runtime.InteropServices;

namespace U8
{
    public class U8File
    {
        public bool IsNestedArc(byte[] content)
        {
            return content.Length > Marshal.SizeOf<Structs.U8ArchiveHeader>()
                &&
                content[0] == 0x55
                &&
                content[1] == 0xAA
                &&
                content[2] == 0x38
                &&
                content[3] == 0x2D;
        }
    }
}