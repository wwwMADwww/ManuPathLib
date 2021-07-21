using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.FillGenerators
{
    public class IntervalDotsFillGenerator: IPrimitiveFillGenerator
    {
        const float bezierDeltaT = 0.001f;

        private readonly Vector2 _intervalMin;
        private readonly Vector2 _intervalMax;
        private readonly Vector2 _randomRadius;
        private readonly Path _path;
        private static Random _random = new Random(DateTime.Now.Millisecond);
        private RectangleF _pathbounds;

        class Intersection
        {
            public Vector2 point;
            public float? dy;
        }


        public IntervalDotsFillGenerator(Path path, Vector2 intervalMin, Vector2 intervalMax, Vector2 randomRadius = default)
        {
            if (!path.FillColor.HasValue)
                throw new ArgumentException("polygon must have FillColor");

            _intervalMin = intervalMin;
            _intervalMax = intervalMax;
            _randomRadius = randomRadius;
            _path = path;
            _pathbounds = _path.Bounds;
        }



        public Path GenerateFill()
        {

            var res = new List<Vector2>();

            var intensity = _path.FillColor.Value.A;

            var interval = (_intervalMin + (_intervalMax - ((_intervalMax - _intervalMin) * (intensity / 255f))));

            //var bounds = _path.Bounds;
            var bounds = _pathbounds;
            for (var y = bounds.Top; y < bounds.Bottom; y += interval.Y)
            {

                var segpoints = new List<Intersection>();

                foreach (var prim in _path.Primitives)
                {

                    var intersections = PathMath.IsRightRayIntersectsWithPrim(prim, new Vector2(bounds.Left - 1, y));
                    if (!intersections?.Any() ?? true)
                        continue;

                    switch (_path.FillRule)
                    {
                        case PathFillRule.EvenOdd:
                            segpoints.AddRange(intersections.Select(i => new Intersection() { point = i.point }));
                            break;

                        case PathFillRule.NonZeroWinding:

                            // casting ray from point to right
                            // on clockwise - line goes down
                            // on counter clockwise - line goes down

                            foreach (var intersection in intersections)
                            {

                                float dy = float.NaN;

                                if (prim is Segment s)
                                {
                                    dy = s.P2.Y - s.P1.Y;
                                }
                                else if (prim is CubicBezier cb)
                                {
                                    var t2 = intersection.t.Value + bezierDeltaT;
                                    var y2 = BezierMath.BezierCoord(t2, cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);
                                    dy = y2 - intersection.point.Y;
                                }
                                else
                                    throw new ArgumentException();

                                segpoints.Add(new Intersection() { point = intersection.point, dy = dy });
                            }

                            break;

                    } // /switch fillrule



                } // /foreach prim

                if (segpoints.Count <= 1)
                    continue;


                var ranges = new List<Vector2>();

                switch (_path.FillRule)
                {
                    case PathFillRule.EvenOdd:
                        ranges = segpoints.Select(sp => sp.point).ToList();
                        break;

                    case PathFillRule.NonZeroWinding:

                        segpoints = segpoints.OrderBy(p => p.point.X).ToList();

                        var countold = 0;
                        var p1 = segpoints[0].point;
                        Vector2 p2 = Vector2.Zero;

                        for (int i = 1; i < segpoints.Count; i++)
                        {
                            var count = 0;

                            foreach (var sp in segpoints.Skip(i))
                            {
                                if (sp.dy < 0)
                                    count--; // clockwise
                                else if (sp.dy > 0)
                                    count++; // counter clockwise
                            }

                            if (count == 0)
                            {
                                ranges.Add(p1);
                                ranges.Add(p2);
                                p1 = segpoints[i].point;
                                p2 = Vector2.Zero;
                            }
                            else
                            {
                                if (p2 == Vector2.Zero || Math.Abs(count) < Math.Abs(countold))
                                    p2 = segpoints[i].point;
                            }

                            countold = count;
                        }
                        ranges.Add(p1);
                        ranges.Add(p2);
                        break;
                }


                if (ranges.Count <= 1)
                    continue;

                ranges = ranges.OrderBy(p => p.X).ToList();

                if (ranges.Count % 2 == 1)
                {
                    // ranges.Add(new Vector2(bounds.Right, y));
                    ranges.RemoveAt(ranges.Count - 1);
                }



                for (var x = bounds.Left; x < bounds.Right; x += interval.X)
                {
                    for (int i = 0; i < ranges.Count; i += 2 )
                    {
                        if (ranges[i].X <= x && x <= ranges[i + 1].X)
                        {

                            if (_randomRadius == Vector2.Zero)
                                res.Add(new Vector2(x, y));
                            else
                                res.Add(new Vector2(
                                    x + (float)(_random.NextDouble() - 0.5) * _randomRadius.X,
                                    y + (float)(_random.NextDouble() - 0.5) * _randomRadius.Y));

                            break;
                        }
                    }
                } // /for x


            } // /for y

            return new Path()
            {
                StrokeColor = _path.FillColor,
                Primitives = res.Select(v => new Dot(v)).ToArray()
            };

        }

    }
}
