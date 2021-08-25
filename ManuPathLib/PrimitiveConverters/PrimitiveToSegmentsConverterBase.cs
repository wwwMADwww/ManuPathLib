using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.PrimitiveConverters
{
    public abstract class PrimitiveToSegmentsConverterBase : IPrimitiveConverter
    {

        private protected bool _closePath;


        public IEnumerable<IPathPrimitive> Convert(IEnumerable<IPathPrimitive> primitives)
        {
            if (!primitives.Any())
                return new Segment[0];

            var segmentPath = new List<Segment>();

            Vector2? pathStart = null;
            Vector2? lastPoint = null;

            foreach (var primitive in primitives)
            {
                IEnumerable<Segment> segs = null;

                if (primitive is Segment segment)
                {
                    segs = SegmentDivide(segment, lastPoint ?? default);

                    // if (!pathStart.HasValue)
                    //     pathStart = segs.First().FirstPoint; // segment.P1;
                    // lastPoint = segs.Last().LastPoint; // segment.P2;
                }
                else if (primitive is CubicBezier cb)
                {
                    segs = CubicBezierToSegments(cb, lastPoint ?? default);

                    //if (!pathStart.HasValue)
                    //    pathStart = segs.First().FirstPoint; // cb.P1;
                    //lastPoint = segs.Last().LastPoint; // cb.P2;
                }
                else
                {
                    throw new Exception($"primitive type is {primitive.GetType().Name}");
                }

                if (!segs?.Any() ?? true)
                    continue;

                if (!pathStart.HasValue)
                    pathStart = segs.First().FirstPoint; // cb.P1;
                lastPoint = segs.Last().LastPoint; // cb.P2;

                segmentPath.AddRange(segs);
            }

            if (segmentPath.Any())
            {
                var lastPrim = primitives.Last();

                if (lastPoint.Value != lastPrim.LastPoint)
                {
                    segmentPath.Add(new Segment(lastPoint.Value, lastPrim.LastPoint));
                    lastPoint = lastPrim.LastPoint;
                }

                // closing path
                if (_closePath && pathStart.HasValue && lastPoint.HasValue)
                {
                    var seg = new Segment(lastPoint.Value, pathStart.Value);
                    segmentPath.Add(seg);
                }
            }

            return segmentPath.ToArray();

        }



        protected abstract IEnumerable<Segment> CubicBezierToSegments(CubicBezier cubicBezier, Vector2 prevPoint = default);



        protected abstract IEnumerable<Segment> SegmentDivide(Segment segment, Vector2 prevPoint = default);



    }
}
