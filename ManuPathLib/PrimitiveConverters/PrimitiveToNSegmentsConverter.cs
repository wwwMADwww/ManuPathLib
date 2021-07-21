using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.PrimitiveConverters
{
    public class PrimitiveToNSegmentsConverter: PrimitiveToSegmentsConverterBase
    {
        private readonly int _segmentCount;

        public PrimitiveToNSegmentsConverter(int segmentCount, bool closePath)
        {
            _segmentCount = segmentCount;
            _closePath = closePath;
        }


        protected override IEnumerable<Segment> CubicBezierToSegments(CubicBezier cb, Vector2 prevPoint = default)
        {
            var res = new List<Segment>();

            var dt = 1.0f / _segmentCount;

            Vector2 p1 = cb.P1;

            for (var i = 1; i <= _segmentCount; i++)
            {
                var t = i * dt;
                var p2 = new Vector2(
                    BezierMath.BezierCoord(t, cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X),
                    BezierMath.BezierCoord(t, cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y));
                res.Add(new Segment(p1, p2));
                p1 = p2;
            }

            return res;
        }



        protected override IEnumerable<Segment> SegmentDivide(Segment segment, Vector2 prevPoint = default)
        {
            var res = new List<Segment>();

            var dx = (segment.P2.X - segment.P1.X) / _segmentCount;
            var dy = (segment.P2.Y - segment.P1.Y) / _segmentCount;

            var p1 = segment.P1;

            for (int i = 1; i <= _segmentCount; i++)
            {
                var p2 = new Vector2(segment.P1.X + dx * i, segment.P1.Y + dy * i);
                res.Add(new Segment(p1, p2));
                p1 = p2;
            }

            return res.ToArray();
        }

    }
}
