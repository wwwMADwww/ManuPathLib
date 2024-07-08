using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.DotGenerators
{
    public class EmptyDotGenerator : IDotGenerator
    {
        public GeneratedDots[] Generate()
        {
            return Array.Empty<GeneratedDots>();
        }
    }
}
