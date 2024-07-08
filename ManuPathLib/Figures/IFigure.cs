using ManuPath.Maths;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace ManuPath.Figures
{
    public interface IFigure
    {
        string Id { get; set; }

        Vector2 FirstPoint { get; }

        Vector2 LastPoint { get; }

        Fill Fill { get; set; }

        Stroke Stroke { get; set; }

        RectangleF Bounds { get; }

        void Reverse();

        Path ToPath();
    }
}
