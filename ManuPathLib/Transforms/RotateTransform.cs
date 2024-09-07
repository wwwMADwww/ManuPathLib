using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ManuPath.Maths;

namespace ManuPath.Transforms
{
    public class RotateTransform : ITransform
    {
        private readonly Vector2 _center;
        private readonly float _angle;

        public RotateTransform(Vector2 center, float angle)
        {
            _center = center;
            _angle = angle;
        }

        public Vector2 Transform(Vector2 coord)
        {
            var newCoord = CommonMath.Rotate(_center, coord, CommonMath.DegToRad(_angle));
            return newCoord;
        }
    }
}
