using System;
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
        void Parse(Span<byte> content);
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

        public virtual void Parse(Span<byte> content)
        {
            if (!content.StartsWithMagicNumber(MagicNumber))
            {
                throw new Exception($"Content does not start with '{MagicNumber}'");
            }
            ParseHeader(content.Slice(MagicNumber.Length, HeaderLength));
            ParseBody(content.Slice(MagicNumber.Length + HeaderLength));
        }

        protected virtual void ValidateMagicNumber(string magicNumber, Span<byte> content)
        {
            if (!content.StartsWithMagicNumber(magicNumber))
            {
                throw new Exception($"Content does not start with '{magicNumber}'");
            }
        }

        protected abstract void ParseHeader(Span<byte> header);
        protected abstract void ParseBody(Span<byte> body);

        public TParent? Parent { get; private set; }
        public int HeaderLength { get; }

        protected IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] values)
        {
            return values.SelectMany(t => t);
        }
    }
}
