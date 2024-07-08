using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ManuPath.DotGenerators
{
    public interface IDotGenerator
    {
        GeneratedDots[] Generate();
    }
}
