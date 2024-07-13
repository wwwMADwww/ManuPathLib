using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ManuPath.Transforms
{
    public interface ITransform
    {
        Vector2 Transform(Vector2 pivot, Vector2 coord);
    }
}
