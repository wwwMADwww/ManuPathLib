using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ManuPath.Transforms
{
    public class ScaleTransform : ITransform
    {
        private readonly Vector2 _scale;

        public ScaleTransform(Vector2 scale) 
        {
            _scale = scale;
        }

        public ScaleTransform(float x, float y) : this(new Vector2(x, y)) { }

        public Vector2 Transform(Vector2 coord)
        {
            return coord * _scale;
        }
    }
}
