using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ManuPath.Extensions;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Maths;

namespace ManuPath.DotGenerators.FillGenerators
{
    public class IntervalFillDotGenerator : IDotGenerator
    {
        private readonly Vector2 _intervalMin;
        private readonly Vector2 _intervalMax;
        private readonly Vector2 _randomRadiusMax;
        private readonly Vector2 _randomRadiusMin;
        private readonly IFigure _figure;
        private static Random _random = new Random(DateTime.Now.Millisecond);
        private RectangleF _pathbounds;
        private byte _intensity;
        private Vector2 _interval;
        private RowRangeEnds[] _rowsRangeEnds;

        struct Intersection
        {
            public Vector2 point;
            public double dySign;
        }

        struct RowRangeEnds
        {
            public float Y;
            public Vector2[] RangeEnds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="figure">Path to generate fill for</param>
        /// <param name="intervalMin">Interval between points on minimum intensity</param>
        /// <param name="intervalMax">Interval between points on maximum intensity</param>
        /// <param name="randomRadiusMin">Randomization radius on minimum intensity</param>
        /// <param name="randomRadiusMax">Randomization radius on maximum intensity</param>
        public IntervalFillDotGenerator(
            IFigure figure,
            bool transform,
            Vector2 intervalMin,
            Vector2 intervalMax,
            Vector2 randomRadiusMin = default,
            Vector2 randomRadiusMax = default)
        {
            if (figure.Fill == null) throw new ArgumentException("Figure must have Fill");

            _intervalMin = intervalMin;
            _intervalMax = intervalMax;
            _randomRadiusMin = randomRadiusMin;
            _randomRadiusMax = randomRadiusMax;

            // TODO: implement ellipse and rectangle
            _figure = figure.ToPath(transform); 
            // var primitives = ((Path)_figure).Primitives;
            // for (int i = 0; i < primitives.Length; i++)
            // {
            //     var p = primitives[i];
            //     if (p is QuadraticBezier qb)
            //     {
            //         primitives[i] = qb.ToCubicBezier();
            //     }
            // }

            FilterAndClosePrimitives((Path)_figure);

            _pathbounds = _figure.GetBounds();

            _intensity = _figure.Fill.Color.A;

            // more intensity - less interval
            _interval = new Vector2(
                CommonMath.ConvertRange(0, 255, _intervalMin.X, _intervalMax.X, _intensity),
                CommonMath.ConvertRange(0, 255, _intervalMin.Y, _intervalMax.Y, _intensity)
            );

            CalculatePathRowsRangeEnds((Path)_figure, _interval);
        }



        public GeneratedDots[] Generate()
        {

            Vector2 randomRadius = Vector2.Zero;

            if (_randomRadiusMin != Vector2.Zero || _randomRadiusMax != Vector2.Zero)
            {
                randomRadius = new Vector2(
                    CommonMath.ConvertRange(0, 255, _randomRadiusMin.X, _randomRadiusMax.X, _intensity),
                    CommonMath.ConvertRange(0, 255, _randomRadiusMin.Y, _randomRadiusMax.Y, _intensity)
                );
            }

            Vector2[] dots;

            if (_figure is Path path)
            {
                dots = GenerateForPath(randomRadius);
            }
            else
            {
                throw new NotImplementedException($"Figure {_figure.GetType().Name} is not supported yet. Convert it to Path first.)");
            }

            return new[] { new GeneratedDots()
            {
                Color = _figure.Fill.Color,
                Dots = dots
            }};
        }

        private Vector2[] GenerateForPath(Vector2 randomRadius)
        {
            var dots = new List<Vector2>();

            var cols = (int)(_pathbounds.Width / _interval.X);

            foreach (var rowRangeEnds in _rowsRangeEnds)
            {
                var y = rowRangeEnds.Y;
                var rangeEnds = rowRangeEnds.RangeEnds;

                for (var col = 0; col < cols; col++)
                {
                    var x = _pathbounds.Left + (_interval.X * col);

                    for (int i = 0; i < rangeEnds.Length; i += 2)
                    {
                        if (rangeEnds[i].X <= x && x <= rangeEnds[i + 1].X)
                        {
                            if (randomRadius == Vector2.Zero)
                            {
                                dots.Add(new Vector2(x, y));
                            }
                            else
                            {
                                dots.Add(new Vector2(
                                    x + (float)(_random.NextDouble() - 0.5) * randomRadius.X,
                                    y + (float)(_random.NextDouble() - 0.5) * randomRadius.Y));
                            }

                            break;
                        }
                    }
                }
            }

            return dots.ToArray();
        }

        private void CalculatePathRowsRangeEnds(Path path, Vector2 interval)
        {            
            var rows = (int) ((_pathbounds.Bottom - _pathbounds.Top) / interval.Y);

            var rowsRangeEnds = new List<RowRangeEnds>(rows); 

            for (var row = 0; row < rows; row++)
            {
                var y = _pathbounds.Top + (interval.Y * row);

                var rayStart = new Vector2(_pathbounds.Left - 1, y);

                var intersections = path.Fill.Rule switch
                {
                    FillRule.EvenOdd => CalculateIntersectionsEvenOdd(path.Primitives, rayStart),
                    FillRule.NonZeroWinding => CalculateIntersectionsNonZeroWinding(path.Primitives, rayStart),
                    _ => throw new ArgumentOutOfRangeException(nameof(path.Fill) + "." + nameof(path.Fill.Rule), path.Fill.Rule, "Unknown FillRule"),
                };

                if (intersections.Length <= 1) continue;

                var rangesEnds = path.Fill.Rule switch
                {
                    FillRule.EvenOdd => CalculateRangeEndsEvenOdd(intersections),
                    FillRule.NonZeroWinding => CalculateRangeEndsNonZeroWinding(intersections),
                    _ => throw new ArgumentOutOfRangeException(nameof(path.Fill) + "." + nameof(path.Fill.Rule), path.Fill.Rule, "Unknown FillRule"),
                };

                rangesEnds = rangesEnds.OrderBy(p => p.X).Distinct().ToArray();

                if (rangesEnds.Length <= 1) continue;

                if (rangesEnds.Length % 2 == 1)
                {
                    rangesEnds = rangesEnds[..^1];
                }

                var rowRangeEnds = new RowRangeEnds() { Y = y, RangeEnds = rangesEnds };

                rowsRangeEnds.Add(rowRangeEnds);

            } // /for y

            _rowsRangeEnds = rowsRangeEnds.ToArray();
        }

        #region CalculateIntersections

        private Intersection[] CalculateIntersectionsEvenOdd(IPathPrimitive[] primitives, Vector2 rayStart)
        {
            var intersections = new List<Intersection>();

            foreach (var prim in primitives)
            {
                var primitiveIntersections = PathMath.GetRightRayPrimIntersections(prim, rayStart);

                if (primitiveIntersections.IsNullOrEmpty()) 
                    continue;

                intersections.AddRange(primitiveIntersections.Select(i => new Intersection() { point = i.point }));
            }

            return intersections.ToArray();
        }

        private Intersection[] CalculateIntersectionsNonZeroWinding(IPathPrimitive[] primitives, Vector2 rayStart)
        {
            var intersections = new List<Intersection>();

            foreach (var prim in primitives)
            {
                var primitiveIntersections = PathMath.GetRightRayPrimIntersections(prim, rayStart);

                if (primitiveIntersections.IsNullOrEmpty()) 
                    continue;

                // casting ray from point to right
                // on clockwise - line goes down
                // on counter clockwise - line goes down

                foreach (var intersection in primitiveIntersections)
                {
                    double dySign;

                    if (prim is Segment s)
                    {
                        dySign = Math.Sign(s.P2.Y - (double)s.P1.Y);
                    }
                    else if (prim is CubicBezier cb)
                    {
                        var dy = BezierMath.CubicBezierDerivative(intersection.t.Value, cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);
                        dySign = CommonMath.IsDoubleEquals(dy, 0) ? 0 : Math.Sign(dy);
                    }
                    else if (prim is QuadraticBezier qb)
                    {
                        var dy = BezierMath.QuadBezierDerivative(intersection.t.Value, qb.P1.Y, qb.C.Y, qb.P2.Y);
                        dySign = CommonMath.IsDoubleEquals(dy, 0) ? 0 : Math.Sign(dy);
                    }
                    else
                    {
                        throw new NotSupportedException($"Primitive {prim.GetType().Name} is not supported.");
                    }

                    intersections.Add(new Intersection() { point = intersection.point, dySign = dySign });
                }
            }

            return intersections.ToArray();
        }

        #endregion CalculateIntersections

        #region CalculateRangeEnds

        private Vector2[] CalculateRangeEndsEvenOdd(Intersection[] intersections)
        {
            var rangesEnds = intersections.Select(sp => sp.point).ToArray();

            return rangesEnds;
        }

        private Vector2[] CalculateRangeEndsNonZeroWinding(Intersection[] intersections)
        {
            var rangesEnds = new List<Vector2>();

            intersections = intersections.OrderBy(p => p.point.X).ToArray();

            var countold = 0;
            var p1 = intersections[0].point;
            Vector2? p2 = null;

            for (int i = 1; i < intersections.Length; i++)
            {
                var count = 0;

                foreach (var intersection in intersections.Skip(i))
                {
                    if (intersection.dySign < 0) count--; // clockwise
                    if (intersection.dySign > 0) count++; // counter clockwise
                }

                if (count == 0)
                {
                    if (p2.HasValue)
                    {
                        rangesEnds.Add(p1);
                        rangesEnds.Add(p2.Value);
                        p1 = intersections[i].point;
                        p2 = null;
                    }
                }
                else
                {
                    if (p2 == null || Math.Abs(count) < Math.Abs(countold))
                        p2 = intersections[i].point;
                }

                countold = count;
            }

            if (p2.HasValue)
            {
                rangesEnds.Add(p1);
                rangesEnds.Add(p2.Value);
            }

            return rangesEnds.ToArray();
        }

        #endregion CalculateRangeEnds

        private void FilterAndClosePrimitives(Path path)
        {
            // TODO: fix path with separated Segments. maybe just delete them

            Vector2? lastOpen = null;

            var primitives = path.Primitives
                .Where(p => !(p is Dot))
                .ToArray();

            var len = primitives.Length;

            int start;
            for (start = 0; start < len; start++)
            {
                var currentEnd = primitives[start].LastPoint;

                var nextIndex = start < primitives.Length - 1 ? start + 1 : 0;

                var nextStart = primitives[nextIndex].FirstPoint;

                if (!CommonMath.IsVectorsEquals(nextStart, currentEnd))
                {
                    lastOpen = primitives[nextIndex].FirstPoint;
                }
            }

            if (lastOpen == null)
            {
                path.Primitives = primitives;
            }

            var closedPrims = new List<IPathPrimitive>(primitives.Length + 1);

            for (var i = start; i < len + start; i++)
            {
                closedPrims.Add(primitives[i % len]);

                var currentEnd = primitives[      i % len].LastPoint;

                var nextStart  = primitives[(i + 1) % len].FirstPoint;

                if (!CommonMath.IsVectorsEquals(nextStart, currentEnd))
                {
                    if (lastOpen.HasValue)
                    {
                        var seg = new Segment(currentEnd, lastOpen.Value);
                        lastOpen = nextStart;
                        closedPrims.Add(seg);
                    }
                    else
                    { 
                        lastOpen = currentEnd;
                    }
                }
            }

            path.Primitives = closedPrims.ToArray();

        }

    }
}
