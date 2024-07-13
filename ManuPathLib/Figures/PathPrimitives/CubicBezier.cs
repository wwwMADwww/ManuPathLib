using System.Drawing;
using System.Linq;
using System.Numerics;
using ManuPath.Extensions;
using ManuPath.Maths;
using ManuPath.Transforms;

namespace ManuPath.Figures.PathPrimitives
{
    public class CubicBezier : IPathPrimitive
    {
        public CubicBezier() { }

        public CubicBezier(Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
            C1 = c1;
            C2 = c2;
        }

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }
        public Vector2 C1 { get; set; }
        public Vector2 C2 { get; set; }

        public Vector2 FirstPoint => P1;
        public Vector2 LastPoint => P2;

        public RectangleF GetBounds()
        {
            // https://eliot-jones.com/2019/12/cubic-bezier-curve-bounding-boxes

            var solX = BezierMath.GetBezierQuadraticRoots(P1.X, C1.X, C2.X, P2.X).Select(t => BezierMath.BezierCoord(t, P1.X, C1.X, C2.X, P2.X));
            var solY = BezierMath.GetBezierQuadraticRoots(P1.Y, C1.Y, C2.Y, P2.Y).Select(t => BezierMath.BezierCoord(t, P1.Y, C1.Y, C2.Y, P2.Y));

            solX = solX.Concat(new[] { P1.X, P2.X }).ToArray();
            solY = solY.Concat(new[] { P1.Y, P2.Y }).ToArray();

            var minX = solX.Min();
            var minY = solY.Min();

            var maxX = solX.Max();
            var maxY = solY.Max();


            return new RectangleF(new PointF(minX, minY), new SizeF(maxX - minX, maxY - minY));
        }

        public IPathPrimitive Transform(ITransform[] transforms)
        {
            var cb = (CubicBezier)Clone();
            foreach (var transform in transforms.EmptyIfNull())
            {
                var pivot = cb.P1;
                cb.P1 = transform.Transform(pivot, cb.P1);
                cb.C1 = transform.Transform(pivot, cb.C1);
                cb.C2 = transform.Transform(pivot, cb.C2);
                cb.P2 = transform.Transform(pivot, cb.P2);
            }

            return cb;
        }

        public void Reverse()
        {
            (P2, C2, C1, P1) = (P1, C1, C2, P2);
        }

        public object Clone()
        {
            return new CubicBezier(P1, C1, C2, P2);
        }

    }

}
