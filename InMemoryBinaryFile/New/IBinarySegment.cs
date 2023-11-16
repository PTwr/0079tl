using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New
{
    public interface IPostProcessing
    {
        void AfterDeserialization();
    }
    public interface IBinarySegment
    {
    }

    public interface IBinarySegment<TParent> : IBinarySegment
        where TParent : IBinarySegment
    {
        TParent? Parent { get; }
    }

    public abstract class BinarySegment<TParent> : IBinarySegment<TParent>
        where TParent : IBinarySegment
    {
        public BinarySegment(TParent parent)
        {
            Parent = parent;
        }

        public TParent? Parent { get; private set; }
    }
}
