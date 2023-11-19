using InMemoryBinaryFile.New.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New.Serialization
{
    internal static class helpers
    {
        internal static (int pos, int length, int count) GetFieldMetadata(this object obj, PropertyInfo prop, BinaryFieldAttribute attrib, List<(int absolute, int header, int body)> offsetsHistory)
        {
            var pos = attrib.GetOffset(obj, prop);
            var length = attrib.GetLength(obj, prop);
            var count = attrib.GetCount(obj, prop);

            var offsetScope = offsetsHistory.Last();
            switch (attrib.OffsetScope)
            {
                case OffsetScope.Absolute:
                    offsetScope = offsetsHistory.Last();
                    break;
                case OffsetScope.Segment:
                    offsetScope = offsetsHistory[0];
                    break;
                case OffsetScope.Parent:
                    offsetScope = offsetsHistory[1];
                    break;
                case OffsetScope.GrandParent:
                    offsetScope = offsetsHistory[2];
                    break;
                default:
                    break;
            }

            pos += offsetScope.absolute;
            switch (attrib.OffsetZone)
            {
                case OffsetZone.Absolute:
                    break;
                case OffsetZone.Header:
                    pos += offsetScope.header;
                    break;
                case OffsetZone.Body:
                    pos += offsetScope.body;
                    break;
                default:
                    break;
            }

            return (pos, length, count);
        }

        internal static Span<byte> Segment(this Span<byte> bytes, int pos, int l)
        {
            return (l >= 0) ? bytes.Slice(pos, l) : bytes.Slice(pos);
        }
    }
}
