using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using InMemoryBinaryFile.Helpers;

namespace InMemoryBinaryFile
{
    public interface IBinarySegment
    {
        IEnumerable<byte> GetBytes();
        void Parse(Span<byte> content, Span<byte> everything);
    }

    public abstract class _BaseBinarySegment<TParent> : IBinarySegment
        where TParent : IBinarySegment
    {
        public virtual IEnumerable<byte> GetBytes() => Enumerable.Concat(MagicNumber, Enumerable.Concat(HeaderBytes, BodyBytes));

        protected virtual IEnumerable<byte> HeaderBytes => Enumerable.Empty<byte>();
        protected virtual IEnumerable<byte> BodyBytes => Enumerable.Empty<byte>();

        public byte[] MagicNumber { get; }

        public _BaseBinarySegment(TParent? parent, byte[]? magicNumber = null, int headerLength = 0)
        {
            Parent = parent;
            HeaderLength = headerLength;
            MagicNumber = magicNumber ?? new byte[] { };
        }


        public virtual void Parse(Span<byte> content) => Parse(content, content);
        protected virtual void ParseHeader(Span<byte> content) => ParseHeader(content, content);
        protected virtual void ParseBody(Span<byte> content) => ParseBody(content, content);

        public virtual void Parse(Span<byte> content, Span<byte> everything)
        {
            if (!content.StartsWithMagicNumber(MagicNumber))
            {
                throw new Exception($"Content does not start with '{MagicNumber}'");
            }
            ParseHeader(content.Slice(MagicNumber.Length, HeaderLength), everything);
            ParseBody(content.Slice(MagicNumber.Length + HeaderLength), everything);
        }

        protected virtual void ValidateMagicNumber(string magicNumber, Span<byte> content)
        {
            if (!content.StartsWithMagicNumber(magicNumber))
            {
                throw new Exception($"Content does not start with '{magicNumber}'");
            }
        }

        protected abstract void ParseHeader(Span<byte> header, Span<byte> everything);
        protected abstract void ParseBody(Span<byte> body, Span<byte> everything);

        public TParent? Parent { get; private set; }
        public int HeaderLength { get; }

        protected IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] values)
        {
            return values.SelectMany(t => t);
        }
        protected IEnumerable<T> Concatenate<T>(params IEnumerable<IEnumerable<T>>[] values)
        {
            return null;// values.SelectMany(t => t.Sel);
        }
    }
}
