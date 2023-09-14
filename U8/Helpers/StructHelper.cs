using System.Runtime.InteropServices;

namespace U8.Helpers
{
    public static class StructHelper
    {
        public static T ReadStruct<T>(this Span<byte> bytes, out int offset)
            where T : struct
        {
            offset = Marshal.SizeOf<T>();
            return bytes.Slice(0, offset).CastToStruct<T>();
        }
        public static T CastToStruct<T>(this Span<byte> data) where T : struct
        {
            return data.ToArray().CastToStruct<T>();
        }
        public static T CastToStruct<T>(this byte[] data) where T : struct
        {
            var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
            var obj = Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T));

            if (obj == null) throw new NullReferenceException("Marshaling data returned null");

            var result = (T)obj;
            pData.Free();
            return result;
        }

        public static byte[] CastToArray<T>(this T data) where T : struct
        {
            var result = new byte[Marshal.SizeOf(typeof(T))];
            var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
            pResult.Free();
            return result;
        }

        public static T ReadStruct<T>(BinaryReader reader)
            where T : struct
        {
            return CastToStruct<T>(reader.ReadBytes(Marshal.SizeOf<T>()));
        }

        public static T[] ReadStructArray<T>(BinaryReader reader, uint length)
            where T : struct
        {
            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = ReadStruct<T>(reader);
            }
            return result;
        }

        public static T[] ReadStructArray<T>(this Span<byte> bytes, uint length, out int offset)
            where T : struct
        {
            T[] result = new T[length];
            offset = 0;
            for (int i = 0; i < length; i++)
            {
                result[i] = bytes.Slice(offset).ReadStruct<T>(out var o);
                offset += o;
            }
            return result;
        }

    }
}
