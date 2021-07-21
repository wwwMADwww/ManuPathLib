using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath
{
    public static class BezierMath
    {


        public static float BezierCoord(float t, float i0, float i1, float i2, float i3)
        {
            return (float)(
                i0 * Math.Pow((1 - t), 3) +
                i1 * 3 * t * Math.Pow((1 - t), 2) +
                i2 * 3 * Math.Pow(t, 2) * (1 - t) +
                i3 * Math.Pow(t, 3)
            );
        }




        // Now then: given cubic coordinates {pa, pb, pc, pd} find all roots.
        public static float[] GetBezierCubicRoots(float pa, float pb, float pc, float pd)
        {

            // https://pomax.github.io/bezierinfo/index.html#extremities

            // A helper function to filter for values in the [0,1] interval:
            bool Accept(double t) => 0f <= t && t <= 1f;

            // A real-cuberoots-only function:
            double CubeRoot(double v)
            {
                return (v < 0)
                    ? -Math.Pow(-v, 1.0 / 3.0)
                    : Math.Pow(v, 1.0 / 3.0);
            }



            var a = (3 * pa - 6 * pb + 3 * pc);
            var b = (-3 * pa + 3 * pb);
            var c = pa;
            var d = (-pa + 3 * pb - 3 * pc + pd);

            // do a check to see whether we even need cubic solving:
            if (CommonMath.IsFloatEquals(d, 0))
            {
                // this is not a cubic curve.
                if (CommonMath.IsFloatEquals(a, 0))
                {
                    // in fact, this is not a quadratic curve either.
                    if (CommonMath.IsFloatEquals(b, 0))
                    {
                        // in fact in fact, there are no solutions.
                        return new float[0];
                    }
                    // linear solution
                    return new[] { -c / b }.Where(x => Accept(x)).ToArray();
                }
                // quadratic solution
                var q1 = Math.Sqrt(b * b - 4 * a * c);
                var n2a = 2 * a;
                var root01 = (q1 - b) / n2a;
                var root02 = (-b - q1) / n2a;
                return new[] { root01, root02 }.Where(x => Accept(x)).Select(x => (float)x).ToArray();
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
            if (discriminant < 0)
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
                return new[] { root1, root2, root3 }.Where(x => Accept(x)).Select(x => (float)x).ToArray();
            }

            // three real roots, but two of them are equal:
            if (discriminant == 0)
            {
                u1 = q2 < 0 ? CubeRoot(-q2) : -CubeRoot(q2);
                root1 = 2 * u1 - a / 3;
                root2 = -u1 - a / 3;
                return new[] { root1, root2 }.Where(x => Accept(x)).Select(x => (float)x).ToArray();
            }

            // one real root, two complex roots
            var sd = Math.Sqrt(discriminant);
            u1 = CubeRoot(sd - q2);
            v1 = CubeRoot(sd + q2);
            root1 = u1 - v1 - a / 3;

            return new[] { root1 }.Where(x => Accept(x)).Select(x => (float)x).ToArray();
        }


        public static float[] GetBezierQuadraticRoots(float p0, float p1, float p2, float p3)
        {
            // https://eliot-jones.com/2019/12/cubic-bezier-curve-bounding-boxes

            float i = p1 - p0;
            float j = p2 - p1;
            float k = p3 - p2;

            // P'(x) = (3i - 6j + 3k)t^2 + (-6i + 6j)t + 3i
            float a = (3 * i) - (6 * j) + (3 * k);
            float b = (6 * j) - (6 * i);
            float c = (3 * i);

            float sqrtPart = (b * b) - (4 * a * c);
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




    }
}
