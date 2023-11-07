using BrresLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class BrressTests
    {
        [Fact]
        void Load()
        {
            BrresContainerSegment brress = new BrresContainerSegment();
            var bytes = File.ReadAllBytes(@"C:\games\wii\0079\0079_unpacked\DATA\files\_2d\Indicator\IZNANM.arc\arc\IZNANM.brres");
            brress.Parse(bytes.AsSpan(), bytes.AsSpan());
        }
    }
}
