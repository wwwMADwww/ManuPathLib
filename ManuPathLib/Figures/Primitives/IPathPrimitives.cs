using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace ManuPath.Figures.Primitives
{
    public interface IPathPrimitive : IEquatable<IPathPrimitive>
    {
        Vector2 FirstPoint { get; }
        Vector2 LastPoint { get; }
        RectangleF Bounds { get; }

        void Reverse();
    }
}
