using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ManuPath.Maths
{
    public static class CommonMath
    {
                                      
        public const float _epsilonf = 0.000001f;
        public const double _epsilond = 0.000001;

        public static bool IsFloatEquals(float a, float b, float epsilon = _epsilonf) => 
            Math.Abs(a - b) < epsilon;

        public static bool IsDoubleEquals(double a, double b, double epsilon = _epsilond) => 
            Math.Abs(a - b) < epsilon;

        public static bool IsDoubleLess(double a, double b, double epsilon = _epsilond) =>
            (Math.Abs(a - b) >= epsilon) && a < b;

        public static bool IsDoubleLessEq(double a, double b, double epsilon = _epsilond) =>
            (Math.Abs(a - b) < epsilon) || a < b;

        public static bool IsDoubleGreater(double a, double b, double epsilon = _epsilond) =>
            (Math.Abs(a - b) >= epsilon) && a > b;
        public static bool IsDoubleGreaterEq(double a, double b, double epsilon = _epsilond) =>
            (Math.Abs(a - b) < epsilon) || a > b;

        public static bool IsVectorsEquals(Vector2 a, Vector2 b, float epsilon = _epsilonf) =>
            IsFloatEquals(a.X, b.X, epsilon) && IsFloatEquals(a.Y, b.Y, epsilon);


        public static float Distance(Vector2 p1, Vector2 p2) => (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));

        public static float? Distance(Vector2? p1, Vector2? p2) => p1.HasValue && p2.HasValue
            ? (float?)Math.Sqrt(Math.Pow(p2.Value.X - p1.Value.X, 2) + Math.Pow(p2.Value.Y - p1.Value.Y, 2))
            : null;

        public static bool IsInRange(float start, float end, float value)
        {
            return start <= end
                ? start <= value && value <= end
                : start >= value && value >= end;
        }


        public static bool IsRangesOverlapping(float r1start, float r1end, float r2start, float r2end)
        {
            if (r1start > r1end) (r1start, r1end) = (r1end, r1start);
            if (r2start > r2end) (r2start, r2end) = (r2end, r2start);

            return Math.Max(r1start, r2start) - Math.Min(r1end, r2end) <= 0;
        }



        // public static bool IsOnBothSegments(Segment seg1, Segment seg2, Vector2 point)
        public static bool IsOnBothSegments(Vector2 sa1, Vector2 sa2, Vector2 sb1, Vector2 sb2, Vector2 point)
        {
            return
                IsInRange(sa1.X, sa2.X, point.X) && IsInRange(sa1.Y, sa2.Y, point.Y) &&
                IsInRange(sb1.X, sb2.X, point.X) && IsInRange(sb1.Y, sb2.Y, point.Y);
        }



        // https://stackoverflow.com/questions/4229662/convert-numbers-within-a-range-to-numbers-within-another-range
        public static float ConvertRange(
            float originalStart, float originalEnd, // original range
            float newStart, float newEnd, // desired range
            float value) // value to convert
        {
            var scale = (newEnd - newStart) / (originalEnd - originalStart);
            return newStart + (value - originalStart) * scale;
        }



        public static bool IsInRectangleInc(RectangleF rect, Vector2 p)
        {
            // laying on border included
            return
                rect.X <= p.X && p.X <= rect.X + Math.Abs(rect.Width) &&
                rect.Y <= p.Y && p.Y <= rect.Y + Math.Abs(rect.Height);
        }



        public static bool IsInRectangleExc(RectangleF rect, Vector2 p)
        {
            // laying on border excluded
            return
                rect.X < p.X && p.X < rect.X + Math.Abs(rect.Width) &&
                rect.Y < p.Y && p.Y < rect.Y + Math.Abs(rect.Height);
        }



        public static (Vector2? point, bool intersects, bool parallel) SegmentsIntersections(Vector2 sa1, Vector2 sa2, Vector2 sb1, Vector2 sb2)
        {
            var vaVertical = sa1.X == sa2.X;
            var vbVertical = sb1.X == sb2.X;

            var kb = vbVertical ? 0 : (sb2.Y - sb1.Y) / (sb2.X - sb1.X);
            var ba = sb1.Y - kb * sb1.X;

            var ka = vaVertical ? 0 : (sa2.Y - sa1.Y) / (sa2.X - sa1.X);
            var bb = sa1.Y - ka * sa1.X;

            if (vaVertical && vbVertical) // both vertical hence parallel and maybe on same line
            {
                if (sa1.X == sb1.X)
                    return (null, IsRangesOverlapping(sa1.X, sa2.X, sb1.X, sb2.X), true); // on same line. check overlapping
                else
                    return (null, false, true); // just parallel
            }



            if (sa1.Y == sa2.Y && sb1.Y == sb2.Y) // both horizontal hence parallel and maybe on same line
            {
                if (sa1.Y == sb1.Y)
                    return (null, IsRangesOverlapping(sa1.Y, sa2.Y, sb1.Y, sb2.Y), true); // on same line. check overlapping
                else
                    return (null, false, true); // just parallel
            }




            if (vaVertical) // va vertical, vb is not
            {
                if (IsRangesOverlapping(sb1.X, sb2.X, sa1.X, sa2.X)) // va.x in vb horizontal range
                {
                    var pby = kb * sa1.X + ba;

                    if (IsOnBothSegments(sa1, sa2, sb1, sb2, new Vector2(sa1.X, pby))) // intersection point lays on both segments
                        return (new Vector2(sa1.X, pby), true, false);
                    else
                        return (null, false, false); // intersection point out of one or both segments
                }
                else
                    return (null, false, false); // va.x out of vb horizontal range
            }



            if (vbVertical) // vb vertical, va is not
            {
                if (IsRangesOverlapping(sa1.X, sa2.X, sb1.X, sb2.X)) // vb.x in va horizontal range
                {
                    var pay = ka * sb1.X + bb;

                    if (IsOnBothSegments(sa1, sa2, sb1, sb2, new Vector2(sb1.X, pay))) // intersection point lays on both segments
                        return (new Vector2(sb1.X, pay), true, false);
                    else
                        return (null, false, false); // intersection point out of one or both segments
                }
                else
                    return (null, false, false); // vb.x out of va horizontal range
            }



            // both lines has slope

            var px = Math.Abs(bb - ba) / Math.Abs(ka - kb);
            var py = kb * px + ba;

            var p = new Vector2(px, py);

            if (IsOnBothSegments(sa1, sa2, sb1, sb2, p)) // intersection point lays on both segments
                return (p, true, false);
            else
                return (null, false, false);  // intersection point out of one or both segments


        }


        public static Vector2? LineLineIntesection(Vector2 sa1, Vector2 sa2, Vector2 sb1, Vector2 sb2)
        {
            // https://pomax.github.io/bezierinfo/index.html#intersections

            var nx = (sa1.X * sa2.Y - sa1.Y * sa2.X) * (sb1.X - sb2.X) - (sa1.X - sa2.X) * (sb1.X * sb2.Y - sb1.Y * sb2.X);
            var ny = (sa1.X * sa2.Y - sa1.Y * sa2.X) * (sb1.Y - sb2.Y) - (sa1.Y - sa2.Y) * (sb1.X * sb2.Y - sb1.Y * sb2.X);
            var d = (sa1.X - sa2.X) * (sb1.Y - sb2.Y) - (sa1.Y - sa2.Y) * (sb1.X - sb2.X);
            if (d == 0)
                return null;
            var p = new Vector2(nx / d, ny / d);
            return IsOnBothSegments(sa1, sa2, sb1, sb2, p) ? p : (Vector2?)null;
        }


        public static RectangleF ScaleAndShiftRect(RectangleF r, Vector2 scale, Vector2 shift)
        {
            r.X = r.X * scale.X + shift.Y;
            r.Y = r.Y * scale.Y + shift.Y;
            r.Width = r.Width * scale.X + shift.Y;
            r.Height = r.Height * scale.Y + shift.Y;
            return r;
        }



        public static bool IsRightRayIntersectsWithLine(Vector2 s1, Vector2 s2, Vector2 rayStart)
        {
            // casting ray from rayStart to right
            // graphical representation: https://i.imgur.com/iqyCWya.png

            //  lineX1  lineX2
            //    sn.x  sn.x
            //     .     .
            //  P--.--P--.-P-> miss 1  
            // - - + - - + - - lineY1, sn.y
            //     .    /.    
            //     . P-+-.---> cross 2
            //     .  /  . P-> miss 2   
            //  P--.-+---.---> cross 1
            //     ./  P-.---> miss 3
            // - - + - - + - - lineY2, sn.y
            //  P--.--P--.-P-> miss 1  
            //     .     .

            var segYmin = Math.Min(s1.Y, s2.Y);
            var segYmax = Math.Max(s1.Y, s2.Y);


            var segXmin = Math.Min(s1.X, s2.X);
            var segXmax = Math.Max(s1.X, s2.X);


            if (segYmin <= rayStart.Y && rayStart.Y <= segYmax)
            {
                // ray vertically in level with segment

                if (rayStart.X <= segXmin)
                {
                    // ray start fully on the left
                    // cross 1
                    return true;
                }
                else if (rayStart.X >= segXmax)
                {
                    // ray start fully on the right
                    // miss 2
                    return false;
                }
                else
                {
                    // ray horizontaly in level with segment

                    // https://i.imgur.com/74BXqfP.png
                    // https://www.desmos.com/calculator/zkuuqxkz8r

                    var k = (s2.Y - s1.Y) / (rayStart.Y - s1.Y);
                    var lx = (s2.X - s1.X) / k + s1.X;

                    if (rayStart.X <= lx)
                    {
                        // ray start slightly left or laying on segment
                        // cross 2
                        return true;
                    }
                    else
                    {
                        // ray start slightly right
                        // miss 3
                        return false;
                    }
                }
            }
            else
            {
                // ray is fully above or fully below the segment
                // miss 1
                return false;
            }
        }



        public static Vector2[] LineCircleIntersections(Vector2 center, float radius, Vector2 s1, Vector2 s2)
        {
            // http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/

            var dx = s2.X - s1.X;
            var dy = s2.Y - s1.Y;

            var A = dx * dx + dy * dy;
            var B = 2 * (dx * (s1.X - center.X) + dy * (s1.Y - center.Y));
            var C = (s1.X - center.X) * (s1.X - center.X) +
                (s1.Y - center.Y) * (s1.Y - center.Y) -
                radius * radius;

            var det = B * B - 4 * A * C;
            if (A <= _epsilonf || det < 0)
            {
                // No real solutions.
                return Array.Empty<Vector2>();
            }
            else if (det == 0) // can be omited safely
            {
                // One solution.
                var t = -B / (2 * A);
                return new Vector2[] {
                    new Vector2(s1.X + t * dx, s1.Y + t * dy)
                };
            }
            else
            {
                // Two solutions.
                var t1 = (float)((-B + Math.Sqrt(det)) / (2 * A));
                var t2 = (float)((-B - Math.Sqrt(det)) / (2 * A));

                return new Vector2[] {
                    new Vector2(s1.X + t1 * dx, s1.Y + t1 * dy),
                    new Vector2(s1.X + t2 * dx, s1.Y + t2 * dy)
                };
            }
        }

        public static (Vector2 p1, Vector2 p2)[] GetRectangleSegments(float x, float y, float w, float h)
            => GetRectangleSegments(new RectangleF(x, y, w, h));

        public static (Vector2 p1, Vector2 p2)[] GetRectangleSegments(RectangleF rect)
        {
            return new[]
            {
                (new Vector2(rect.Left,  rect.Top)   , new Vector2(rect.Right, rect.Top)),
                (new Vector2(rect.Right, rect.Top)   , new Vector2(rect.Right, rect.Bottom)),
                (new Vector2(rect.Right, rect.Bottom), new Vector2(rect.Left,  rect.Bottom)),
                (new Vector2(rect.Left,  rect.Bottom), new Vector2(rect.Left,  rect.Top))
            };
        }


        public static float DegToRad(float deg)
        {
            var radians = (Math.PI / 180) * deg;
            return (float) radians;
        }

        public static Vector2 CircleCoord(Vector2 center, float radius, float angle) =>
            EllipseCoord(center, new Vector2(radius), angle);

        public static Vector2 EllipseCoord(Vector2 center, Vector2 radius, float angle)
        {
            var x = radius.X * Math.Cos(angle) + center.X;
            var y = radius.Y * Math.Sin(angle) + center.Y;
            return new Vector2((float)x, (float)y);
        }


        public static float GetAngle(Vector2 center, Vector2 p)
        {
            return (float) Math.Atan2(p.Y - center.Y, p.X - center.X);
        }

        public static float GetAngle(float cx, float cy, float px, float py)
        {
            return (float)Math.Atan2(py - cy, px - cx);
        }


        public static Vector2 Rotate(Vector2 center, Vector2 p, float angle, bool absoluteAngle = false)
        {
            var oldAngle = absoluteAngle ? 0f : GetAngle(center, p);
            var distance = Distance(center, p);
            var rotated = CircleCoord(center, distance, oldAngle + angle);  
            return rotated;
        }


        /// <summary>
        /// Convert continuous function f(T) to array of dots
        /// </summary>
        /// <param name="startT">start value of function argument</param>
        /// <param name="endT">end value of function argument</param>
        /// <param name="distanceMin">minimal acceptable distance betweet dots</param>
        /// <param name="distanceMax">maximal acceptable distance betweet dots</param>
        /// <param name="initialDeltaT">initial value to increase function argument on each iteration</param>
        /// <param name="prevPoint">if not null, distance is calculated from this point. Otherwise distance is calculated from f(startT)</param>
        /// <param name="curveFunc">f(T). Curve function of argument T, returning X and Y coordinates</param>
        public static Vector2[] CurveToEquidistantDots(
            float startT, 
            float endT, 
            float distanceMin, 
            float distanceMax,
            float initialDeltaT, 
            Vector2? prevPoint,
            Func<float, Vector2?> curveFunc,
            float initialDeltaTKoeff = 2f, 
            float deltaTKoeff2 = 1.2f)
        {

            var res = new List<Vector2>();

            // debug, statistics
            // var repeatEvent = 0;
            // var repeatTotal = 0;
            // var repeatMax = 0;


            // TODO: first and last points

            // res.Add(bc.P1);

            var p1 = curveFunc(startT);

            if (prevPoint.HasValue)
            {
                var pd = CommonMath.Distance(p1, prevPoint.Value);

                if (pd <= distanceMax)
                    p1 = prevPoint.Value;
            }

            var t = 0f;
            var dt = initialDeltaT;

            while (true)
            {
                Vector2? p2;

                var prevDiv = false;
                var prevMul = false;

                var dtkoeff2 = deltaTKoeff2;
                var dtkoeff = initialDeltaTKoeff;

                // var repeat = -1;

                // перебор значений t так, чтобы расстояние между точками было не менее r1 и не более r2
                while (true)
                {
                    // repeat++;

                    var t2 = t + dt;

                    if (t2 >= 1.0f)
                        t2 = 1.0f;

                    p2 = curveFunc(t2);

                    if (p2 == null)
                    {
                        if (t == 1.0f) break;

                        t = t2;
                        continue;
                    }

                    if (p1 == null)
                    { 
                        p1 = p2;
                        t = t2;
                        continue;
                    }

                    var d = CommonMath.Distance(p1, p2);

                    if (d < distanceMin)
                    {
                        // если отрезок слишком короткий, но мы уже уперлись в последнюю точку, то пропускаем действие.
                        if (t2 < 1.0f)
                        {
                            if (prevDiv)
                                dtkoeff /= dtkoeff2;

                            dt *= dtkoeff;
                            prevMul = true;
                            prevDiv = false;

                            continue;
                        }
                    }
                    else if (d > distanceMax)
                    {
                        if (prevMul)
                            dtkoeff *= dtkoeff2;

                        dt /= dtkoeff;
                        prevDiv = true;
                        prevMul = false;

                        continue;
                    }

                    t = t2;
                    // Console.WriteLine($"t {t}, d {d}");

                    dtkoeff = initialDeltaTKoeff;
                    dtkoeff2 = deltaTKoeff2;
                    // x4 less repeatTotal and x2 less repeatEvent when disabled
                    // dt = initialDeltaT;

                    prevDiv = prevMul = false;
                    break;

                } // while repeat


                // if (repeat > 0)
                // {
                //     repeatEvent++;
                //     repeatTotal += repeat;
                //     if (repeatMax < repeat)
                //         repeatMax = repeat;
                // }

                if (t >= 1)
                    break;
                // если отрезок слишком короткий, то не добавляем его

                res.Add(p2.Value);

                p1 = p2;

            } // while elements


            // res.Add(new Segment(p1, cb.P2));


            return res.ToArray();
        }



    }

}
