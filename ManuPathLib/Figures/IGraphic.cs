using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace ManuPath.Figures
{
    public interface IGraphic
    {
        Vector2 FirstPoint { get; }

        Vector2 LastPoint { get; }

        void Reverse();
    }
}
