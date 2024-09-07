using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ManuPath.Transforms
{
    public class MatrixTransform : ITransform
    {
        private readonly Matrix3x2 _matrix;

        public MatrixTransform(Matrix3x2 matrix) 
        {
            _matrix = matrix;
        }

        public Vector2 Transform(Vector2 coord)
        {
            var newCoord = Vector2.Transform(coord, _matrix);
            return newCoord;
        }
    }
}
