using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryBinaryFile.New
{
    public interface IBinaryFile
    {
    }
    public interface IBinarySegment<TParent>
    {
        public TParent Parent { get; }
    }
}
