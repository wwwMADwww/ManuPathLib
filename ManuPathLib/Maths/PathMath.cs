﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Transforms;

namespace ManuPath.Maths
{
    public static class PathMath
    {


        public static (float? t, Vector2 point)[] IsRightRayIntersectsWithPrim(IPathPrimitive prim, Vector2 rayStart)
        {
            if (prim is Dot)
                throw new ArgumentException();

            // casting ray from rayStart to right
            // graphical representation: https://i.imgur.com/iqyCWya.png

            if (prim is Segment s)
            {
                //  lineX1  lineX2
                //    v?.x  v?.x
                //     .     .
                //  P--.--P--.-P-> miss 1  
                // - - + - - + - - lineY1, v?.y
                //     .    /.    
                //     . P-+-.---> cross 2
                //     .  /  . P-> miss 2   
                //  P--.-+---.---> cross 1
                //     ./  P-.---> miss 3
                // - - + - - + - - lineY2, v?.y
                //  P--.--P--.-P-> miss 1  
                //     .     .

                var bounds = prim.GetBounds();

                var segYmin = bounds.Top;
                var segYmax = bounds.Bottom;


                var segXmin = bounds.Left;
                var segXmax = bounds.Right;


                if (segYmin <= rayStart.Y && rayStart.Y <= segYmax)
                {
                    // ray vertically in level with segment

                    // if (rayStart.X <= segXmin)
                    // {
                    //     // ray start fully on the left
                    //     // cross 1
                    //     return true;
                    // }
                    // else 
                    if (rayStart.X >= segXmax)
                    {
                        // ray start fully on the right
                        // miss 2
                        //return false;
                        return null;
                    }
                    else
                    {
                        // ray horizontaly in level with segment

                        // https://i.imgur.com/74BXqfP.png
                        // https://www.desmos.com/calculator/zkuuqxkz8r


                        var k = (s.P2.Y - s.P1.Y) / (rayStart.Y - s.P1.Y);
                        var lx = (s.P2.X - s.P1.X) / k + s.P1.X;

                        if (rayStart.X <= lx)
                        {
                            // ray start slightly left or laying on segment
                            // cross 2
                            // return true;
                            return new[] { ((float?)null, new Vector2(lx, rayStart.Y)) };
                        }
                        else
                        {
                            // ray start slightly right
                            // miss 3
                            //return false;
                            return null;
                        }

                    }
                }
                else
                {
                    // ray is fully above or fully below the segment
                    // miss 1
                    // return false;
                    return null;
                }

            }
            else if (prim is CubicBezier cb)
            {

                // ray is horizontal, no need for angle correction

                var cbr = new CubicBezier(
                    cb.P1 - rayStart,
                    cb.C1 - rayStart,
                    cb.C2 - rayStart,
                    cb.P2 - rayStart
                    );

                var roots = BezierMath.GetBezierCubicRoots(cbr.P1.Y, cbr.C1.Y, cbr.C2.Y, cbr.P2.Y).ToList();

                //return roots
                //    .Select(r => (t: (float?)r, point: new Vector2(
                //        BezierMath.BezierCoord(r, cbr.P1.X, cbr.C1.X, cbr.C2.X, cbr.P2.X),
                //        BezierMath.BezierCoord(r, cbr.P1.Y, cbr.C1.Y, cbr.C2.Y, cbr.P2.Y)) + rayStart))
                //    .Where(r => r.point.X > rayStart.X)
                //    .ToArray();


                var res = new List<(float? t, Vector2 point)>();

                foreach (var root in roots)
                {
                    var x = BezierMath.BezierCoord(root, cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X);
                    if (x > rayStart.X)
                    {
                        var y = BezierMath.BezierCoord(root, cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);
                        res.Add((root, new Vector2(x, y)));
                    }
                }

                return res.ToArray();
            }
            else
                throw new ArgumentException();
        }


        public static bool IsPointInPolygon(IEnumerable<Segment> polygon, Vector2 point, FillRule rule)
        {

            int count = 0;

            foreach (var s in polygon)
            {
                if (s.IsZeroLength)
                    continue;

                var intersects = CommonMath.IsRightRayIntersectsWithLine(s.P1, s.P2, point); // faster than above
                if (intersects)
                {
                    switch (rule)
                    {
                        case FillRule.EvenOdd:
                            count++;
                            break;

                        case FillRule.NonZeroWinding:

                            // casting ray from point to right
                            // on clockwise - line goes down
                            // on counter clockwise - line goes down

                            var dy = s.P2.Y - s.P1.Y;

                            if (dy < 0)
                                count--; // clockwise
                            else if (dy > 0)
                                count++; // counter clockwise

                            // on dy == 0 line is parallel to ray, does not count

                            break;

                    }
                }

            }


            switch (rule)
            {
                case FillRule.EvenOdd: return count % 2 == 1;
                case FillRule.NonZeroWinding: return count != 0;
                default: throw new Exception("dafuq r u doin?");
            }
        }



        public static bool IsPointInPath(Path path, Vector2 point, float bezierDeltaT = 0.01f)
        {

            int count = 0;

            foreach (var prim in path.Primitives)
            {
                // if (s.IsZeroLength)
                //     continue;

                var intersections = IsRightRayIntersectsWithPrim(prim, point);
                if (intersections?.Any() ?? false)
                {
                    switch (path.Fill.Rule)
                    {
                        case FillRule.EvenOdd:
                            count += intersections.Count();
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


                                if (dy < 0)
                                    count--; // clockwise
                                else if (dy > 0)
                                    count++; // counter clockwise

                                // on dy == 0 line is parallel to ray, does not count
                            }

                            break;

                    }
                }

            }


            switch (path.Fill.Rule)
            {
                case FillRule.EvenOdd: return count % 2 == 1;
                case FillRule.NonZeroWinding: return count != 0;
                default: throw new Exception("dafuq r u doin?");
            }
        }



        public static bool IsSegmentIntersectsWithPoly(IEnumerable<Segment> polygon, Segment s)
        {

            if (s.IsZeroLength)
                return false;

            foreach (var ps in polygon)
            {

                if (ps.IsZeroLength)
                    continue;

                // if (GetIntersectionPoint(ps, s).intersects)
                if (CommonMath.SegmentsIntersections(ps.P1, ps.P2, s.P1, s.P2).intersects)
                    // if (LineLineIntesection(ps, s).HasValue)
                    return true;

            }

            return false;

        }



        public static IEnumerable<IPathPrimitive> ScalePaths(IEnumerable<IPathPrimitive> paths, Vector2 scale, Vector2 shift)
        {

            var res = new List<IPathPrimitive>();

            foreach (var path in paths)
            {
                if (path is CubicBezier cb)
                {
                    var p = new CubicBezier(
                        cb.P1 * scale + shift,
                        cb.C1 * scale + shift,
                        cb.C2 * scale + shift,
                        cb.P2 * scale + shift
                        );

                    res.Add(p);
                }
                else if (path is Segment line)
                {
                    var p = new Segment(
                        line.P1 * scale + shift,
                        line.P2 * scale + shift
                        );
                    res.Add(p);
                }
                else
                {
                    Console.WriteLine($"path type '{path.GetType().Name}' not supported yet");
                }
            }

            return res.ToArray();
        }


    }
}
