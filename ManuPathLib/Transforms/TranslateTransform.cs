using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ManuPath.Transforms
{
    public class TranslateTransform : ITransform
    {
        private readonly Vector2 _shift;

        public TranslateTransform(Vector2 shift) 
        {
            _shift = shift;
        }

        public TranslateTransform(float x, float y): this(new Vector2(x, y)) { }

        public Vector2 Transform(Vector2 coord)
        {
            return coord + _shift;
        }
    }
}
