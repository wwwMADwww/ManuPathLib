using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.FillGenerators
{
    public class EmptyFillGenerator : IPrimitiveFillGenerator
    {
        private readonly Path _poly;

        public EmptyFillGenerator(Path poly)
        {
            _poly = poly;
        }


        public Path GenerateFill()
        {
            return new Path()
            {
                StrokeColor = _poly.FillColor,
                Primitives = new Dot[0]
            };

        }

    }
}
