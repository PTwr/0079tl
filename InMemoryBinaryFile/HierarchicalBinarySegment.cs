using InMemoryBinaryFile.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile
{
    public abstract class ParentBinarySegment<TParent, TChild> : _BaseBinarySegment<TParent>
        where TParent : IBinarySegment
        where TChild : IBinarySegment
    {
        protected override IEnumerable<byte> BodyBytes => children.SelectMany(c => c.GetBytes());

        protected virtual List<TChild> children => new List<TChild>();

        protected ParentBinarySegment(TParent? parent, byte[]? magicNumber = null, int headerLength = 0) : base(parent, magicNumber, headerLength)
        {
        }

        public ReadOnlyCollection<TChild> Children => children.AsReadOnly();
    }
}
