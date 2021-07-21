using System;
using System.Collections.Generic;
using System.Text;

namespace ManuPath
{
    public interface IPrimitiveConverter
    {

        IEnumerable<IPathPrimitive> Convert(IEnumerable<IPathPrimitive> primitives);

    }
}
