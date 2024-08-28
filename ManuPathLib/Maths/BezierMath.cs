using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.Maths
{
    public static class BezierMath
    {


        public static float CubicBezierCoord(float t, float p1, float c1, float c2, float p2)
        {
            return (float)(
                p1 * Math.Pow(1 - t, 3) +
                c1 * 3 * t * Math.Pow(1 - t, 2) +
                c2 * 3 * Math.Pow(t, 2) * (1 - t) +
                p2 * Math.Pow(t, 3)
            );
        }

        public static Vector2 CubicBezierCoords(float t, Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2)
        {
            return new Vector2(
                BezierMath.CubicBezierCoord(t, p1.X, c1.X, c2.X, p2.X),
                BezierMath.CubicBezierCoord(t, p1.Y, c1.Y, c2.Y, p2.Y));
        }


        public static float QuadBezierCoord(float t, float p1, float c, float p2)
        {
            // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves
            // https://stackoverflow.com/a/5634528
            return
                (1 - t) * (1 - t) * p1 +
                2 * (1 - t) * t * c +
                t * t * p2;
        }

        public static Vector2 QuadBezierCoords(float t, Vector2 p1, Vector2 c, Vector2 p2)
        {
            return new Vector2(
                BezierMath.QuadBezierCoord(t, p1.X, c.X, p2.X),
                BezierMath.QuadBezierCoord(t, p1.Y, c.Y, p2.Y));
        }

        public static float? QuadBezierLinearRoot(double p1, double c, double p2)
        {
            // https://iquilezles.org/articles/bezierbbox/

            var t = (p1 - c) / (p1 - (2.0f * c) + p2);

            if (!double.IsNaN(t) && (0 <= t && t <= 1)) return (float)t;
            else return null;
        }

        public static double[] CubicBezierCubicRoots(double p1, double c1, double c2, double p2)
        {
            // Now then: given cubic coordinates {pa, pb, pc, pd} find all roots.
            // https://pomax.github.io/bezierinfo/index.html#extremities

            // A helper function to filter for values in the [0,1] interval:
            bool Accept(double t) => CommonMath.IsDoubleGreaterEq(t, 0) && CommonMath.IsDoubleLessEq(t, 1);

            // A real-cuberoots-only function:
            double CubeRoot(double v)
            {
                return CommonMath.IsDoubleLess(v, 0)
                    ? -Math.Pow(-v, 1.0 / 3.0)
                    :  Math.Pow( v, 1.0 / 3.0);
            }


            var (pa, pb, pc, pd) = (p1, c1, c2, p2);

            var a = 3 * pa - 6 * pb + 3 * pc;
            var b = -3 * pa + 3 * pb;
            var c = pa;
            var d = -pa + 3 * pb - 3 * pc + pd;

            // do a check to see whether we even need cubic solving:
            if (CommonMath.IsDoubleEquals(d, 0))
            {
                // this is not a cubic curve.
                if (CommonMath.IsDoubleEquals(a, 0))
                {
                    // in fact, this is not a quadratic curve either.
                    if (CommonMath.IsDoubleEquals(b, 0))
                    {
                        // in fact in fact, there are no solutions.
                        return Array.Empty<double>();
                    }
                    // linear solution
                    return new[] { -c / b }.Where(x => Accept(x)).ToArray();
                }
                // quadratic solution
                var q1 = Math.Sqrt(b * b - 4 * a * c);
                var n2a = 2 * a;
                var root01 = (q1 - b) / n2a;
                var root02 = (-b - q1) / n2a;
                return new[] { root01, root02 }.Where(x => Accept(x)).ToArray();
            }

            // at this point, we know we need a cubic solution.

            a /= d;
            b /= d;
            c /= d;

            var p = (3 * b - a * a) / 3;
            var p3 = p / 3;
            var q = (2 * a * a * a - 9 * a * b + 27 * c) / 27;
            var q2 = q / 2;
            var discriminant = q2 * q2 + p3 * p3 * p3;

            // and some variables we're going to use later on:
            double u1, v1, root1, root2, root3;

            // three possible real roots:
            if (CommonMath.IsDoubleLess(discriminant, 0))
            {
                var mp3 = -p / 3;
                var mp33 = mp3 * mp3 * mp3;
                var r = Math.Sqrt(mp33);
                var t = -q / (2 * r);
                var cosphi = t < -1 ? -1 : t > 1 ? 1 : t;
                var phi = Math.Acos(cosphi);
                var crtr = CubeRoot(r);
                var t1 = 2 * crtr;
                root1 = t1 * Math.Cos(phi / 3) - a / 3;
                root2 = t1 * Math.Cos((phi + 2 * Math.PI) / 3) - a / 3;
                root3 = t1 * Math.Cos((phi + 4 * Math.PI) / 3) - a / 3;
                return new[] { root1, root2, root3 }.Where(x => Accept(x)).ToArray();
            }

            // three real roots, but two of them are equal:
            if (CommonMath.IsDoubleEquals(discriminant, 0))
            {
                u1 = q2 < 0 ? CubeRoot(-q2) : -CubeRoot(q2);
                root1 = 2 * u1 - a / 3;
                root2 = -u1 - a / 3;
                return new[] { root1, root2 }.Where(x => Accept(x)).ToArray();
            }

            // one real root, two complex roots
            var sd = Math.Sqrt(discriminant);
            u1 = CubeRoot(sd - q2);
            v1 = CubeRoot(sd + q2);
            root1 = u1 - v1 - a / 3;

            return new[] { root1 }.Where(x => Accept(x)).ToArray();
        }


        public static float[] CubicBezierQuadRoots(float p0, float p1, float p2, float p3)
        {
            // https://eliot-jones.com/2019/12/cubic-bezier-curve-bounding-boxes

            float i = p1 - p0;
            float j = p2 - p1;
            float k = p3 - p2;

            // P'(x) = (3i - 6j + 3k)t^2 + (-6i + 6j)t + 3i
            float a = 3 * i - 6 * j + 3 * k;
            float b = 6 * j - 6 * i;
            float c = 3 * i;

            float sqrtPart = b * b - 4 * a * c;
            bool hasSolution = sqrtPart >= 0;
            if (!hasSolution)
            {
                return new float[0];
            }

            float t1 = (-b + (float)Math.Sqrt(sqrtPart)) / (2 * a);
            float t2 = (-b - (float)Math.Sqrt(sqrtPart)) / (2 * a);

            float? s1 = null;
            float? s2 = null;

            if (t1 >= 0 && t1 <= 1)
            {
                s1 = (float)t1;
            }

            if (t2 >= 0 && t2 <= 1)
            {
                s2 = (float)t2;
            }

            return new[] { s1, s2 }.Where(s => s.HasValue).Select(s => s.Value).ToArray();
        }




        public static (Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2) CubicBezierFromQuad(Vector2 p1, Vector2 c, Vector2 p2)
        {
            var c1x = (float)((c.X - p1.X) * 2.0 / 3.0) + p1.X;
            var c1y = (float)((c.Y - p1.Y) * 2.0 / 3.0) + p1.Y;

            var c2x = (float)((c.X - p2.X) * 2.0 / 3.0) + p2.X;
            var c2y = (float)((c.Y - p2.Y) * 2.0 / 3.0) + p2.Y;

            return (p1, new Vector2(c1x, c1y), new Vector2(c2x, c2y), p2);
        }

        public static (Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2) CubicBezierFromArc
            (Vector2 center, Vector2 radius, double angleStart, double angleEnd)
        {
            // https://pomax.github.io/bezierinfo/#circles_cubic
            // https://pomax.github.io/bezierinfo/chapters/circles_cubic/arc-approximation.js
            // modified for two angles

            var da = angleEnd - angleStart;

            //var k = (4.0 / 3.0) * Math.Tan(da / 4.0);
            var k = (4.0 * Math.Tan(da / 4.0)) / 3.0;

            var cosAngleStart = Math.Cos(angleStart);
            var sinAngleStart = Math.Sin(angleStart);

            var cosAngleEnd = Math.Cos(angleEnd);
            var sinAngleEnd = Math.Sin(angleEnd);

            var p1 = new Vector2(
                x: (float)(center.X + radius.X * cosAngleStart),
                y: (float)(center.Y + radius.Y * sinAngleStart)
            );

            var c1 = new Vector2(
                x: (float) (center.X + radius.X * (cosAngleStart - k * sinAngleStart)),
                y: (float) (center.Y + radius.Y * (sinAngleStart + k * cosAngleStart))
            );

            var c2 = new Vector2(
                x: (float)(center.X + radius.X * (cosAngleEnd + k * sinAngleEnd)),
                y: (float)(center.Y + radius.Y * (sinAngleEnd - k * cosAngleEnd))
            );

            var p2 = new Vector2( 
                x: (float)(center.X + radius.X * cosAngleEnd), 
                y: (float)(center.Y + radius.Y * sinAngleEnd)
            );
            
            return (p1, c1, c2, p2);
        }

        public static (Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2)[] CubicBeziersFromEllipse(Vector2 center, Vector2 radius, int arcsNum = 4)
        {
            if (arcsNum < 2) throw new ArgumentOutOfRangeException(nameof(arcsNum), arcsNum, "Arcs number can't be less than 2.");

            var arcs = new List<(Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2)>(arcsNum);

            var a1 = 0.0;

            for (int i = 1; i <= arcsNum; i++)
            {
                var a2 = ((Math.PI * 2) / arcsNum) * i;

                var curve = CubicBezierFromArc(center, radius, a1, a2);

                arcs.Add(curve);

                a1 = a2;
            }

            return arcs.ToArray();
        }


    }
}
