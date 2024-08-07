﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Maths;

namespace ManuPath.DotGenerators.FillGenerators
{
    public class IntervalFillDotGenerator : IDotGenerator
    {
        const float bezierDeltaT = 0.001f;

        private readonly Vector2 _intervalMin;
        private readonly Vector2 _intervalMax;
        private readonly Vector2 _randomRadiusMax;
        private readonly Vector2 _randomRadiusMin;
        private readonly IFigure _figure;
        private static Random _random = new Random(DateTime.Now.Millisecond);
        private RectangleF _pathbounds;

        class Intersection
        {
            public Vector2 point;
            public float? dy;
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

            FilterAndClosePrimitives((Path)_figure);

            _pathbounds = _figure.GetBounds();
        }



        public GeneratedDots[] Generate()
        {
            var intensity = _figure.Fill.Color.A;

            // more intensity - less interval
            var interval = new Vector2(
                CommonMath.ConvertRange(0, 255, _intervalMin.X, _intervalMax.X, intensity),
                CommonMath.ConvertRange(0, 255, _intervalMin.Y, _intervalMax.Y, intensity)
               );

            Vector2 randomRadius = default;

            if (_randomRadiusMin != Vector2.Zero || _randomRadiusMax != Vector2.Zero)
            {
                randomRadius = new Vector2(
                    CommonMath.ConvertRange(0, 255, _randomRadiusMin.X, _randomRadiusMax.X, intensity),
                    CommonMath.ConvertRange(0, 255, _randomRadiusMin.Y, _randomRadiusMax.Y, intensity)
                );
            }

            Vector2[] dots;

            if (_figure is Path path)
            {
                dots = GenerateForPath(path, interval, randomRadius);
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


        private Vector2[] GenerateForPath(Path path, Vector2 interval, Vector2 randomRadius)
        {
            var dots = new List<Vector2>();

            var bounds = _pathbounds;
            for (var y = bounds.Top; y < bounds.Bottom; y += interval.Y)
            {

                var segpoints = new List<Intersection>();

                foreach (var prim in path.Primitives)
                {

                    var intersections = PathMath.IsRightRayIntersectsWithPrim(prim, new Vector2(bounds.Left - 1, y));
                    if (!intersections?.Any() ?? true)
                        continue;

                    switch (_figure.Fill.Rule)
                    {
                        case FillRule.EvenOdd:
                            segpoints.AddRange(intersections.Select(i => new Intersection() { point = i.point }));
                            break;

                        case FillRule.NonZeroWinding:

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

                switch (_figure.Fill.Rule)
                {
                    case FillRule.EvenOdd:
                        ranges = segpoints.Select(sp => sp.point).ToList();
                        break;

                    case FillRule.NonZeroWinding:

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
                    for (int i = 0; i < ranges.Count; i += 2)
                    {
                        if (ranges[i].X <= x && x <= ranges[i + 1].X)
                        {

                            if (randomRadius == Vector2.Zero)
                                dots.Add(new Vector2(x, y));
                            else
                                dots.Add(new Vector2(
                                    x + (float)(_random.NextDouble() - 0.5) * randomRadius.X,
                                    y + (float)(_random.NextDouble() - 0.5) * randomRadius.Y));

                            break;
                        }
                    }
                } // /for x


            } // /for y

            return dots.ToArray();
        }

        private void FilterAndClosePrimitives(Path path)
        {
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
