using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Figures;
using ManuPath.Figures.Primitives;
using ManuPath.Maths;

namespace ManuPath.DotGenerators.StrokeGenerators
{
    public class StrokeDotGenerator : StrokeDotGeneratorBase
    {
        private readonly int _segmentCount;

        public StrokeDotGenerator(IFigure figure, int segmentCount)
            :base(figure)
        {
            _segmentCount = segmentCount;
        }


        protected override Vector2[] CubicBezierToSegments(CubicBezier cb, Vector2 prevPoint = default)
        {
            var res = new List<Vector2>(_segmentCount) { cb.P1 };

            var dt = 1.0f / _segmentCount;

            for (var i = 1; i <= _segmentCount; i++)
            {
                var t = i * dt;
                var p = new Vector2(
                    BezierMath.BezierCoord(t, cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X),
                    BezierMath.BezierCoord(t, cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y));
                res.Add(p);
            }

            return res.ToArray();
        }



        protected override Vector2[] SegmentDivide(Segment segment, Vector2 prevPoint = default)
        {
            var res = new List<Vector2>(_segmentCount) { segment.P1 };

            var dx = (segment.P2.X - segment.P1.X) / _segmentCount;
            var dy = (segment.P2.Y - segment.P1.Y) / _segmentCount;

            for (int i = 1; i <= _segmentCount; i++)
            {
                var p = new Vector2(segment.P1.X + dx * i, segment.P1.Y + dy * i);
                res.Add(p);
            }

            return res.ToArray();
        }



        protected override Vector2[] RectangleDivide(Rectangle rect, Vector2 prevPoint = default) => throw new NotImplementedException();

        protected override Vector2[] EllipseDivide(Ellipse ellipse, Vector2 prevPoint = default) => throw new NotImplementedException();

    }
}
