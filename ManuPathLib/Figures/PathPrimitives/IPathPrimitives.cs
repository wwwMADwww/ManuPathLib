using ManuPath.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace ManuPath.Figures.PathPrimitives
{
    public interface IPathPrimitive: IGraphic, ICloneable
    {
        IPathPrimitive Transform(ITransform[] transforms);

        RectangleF GetBounds();
    }
}
