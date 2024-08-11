using System.Drawing;
using System.Linq;
using System.Numerics;
using ManuPath.Extensions;
using ManuPath.Maths;
using ManuPath.Transforms;

namespace ManuPath.Figures.PathPrimitives
{
    public class QuadraticBezier : IPathPrimitive
    {
        public QuadraticBezier() { }

        public QuadraticBezier(Vector2 p1, Vector2 c, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
            C = c;
        }

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }
        public Vector2 C { get; set; }

        public Vector2 FirstPoint => P1;
        public Vector2 LastPoint => P2;

        public RectangleF GetBounds()
        {
            var trX = BezierMath.QuadBezierLinearRoot(P1.X, C.X, P2.X);
            var trY = BezierMath.QuadBezierLinearRoot(P1.Y, C.Y, P2.Y);

            var tX = trX.HasValue ? BezierMath.QuadBezierCoord(trX.Value, P1.X, C.X, P2.X) : (float?)null;
            var tY = trY.HasValue ? BezierMath.QuadBezierCoord(trY.Value, P1.Y, C.Y, P2.Y) : (float?)null;

            var minX = new[] { tX, P1.X, P2.X }.Min().Value;
            var minY = new[] { tY, P1.Y, P2.Y }.Min().Value;

            var maxX = new[] { tX, P1.X, P2.X }.Max().Value;
            var maxY = new[] { tY, P1.Y, P2.Y }.Max().Value;

            return new RectangleF(new PointF(minX, minY), new SizeF(maxX - minX, maxY - minY));
        }

        public IPathPrimitive Transform(ITransform[] transforms)
        {
            var cb = (QuadraticBezier)Clone();
            foreach (var transform in transforms.EmptyIfNull())
            {
                var pivot = cb.P1;
                cb.P1 = transform.Transform(pivot, cb.P1);
                cb.C  = transform.Transform(pivot, cb.C );
                cb.P2 = transform.Transform(pivot, cb.P2);
            }

            return cb;
        }

        public CubicBezier ToCubicBezier()
        {
            var coords = BezierMath.CubicBezierFromQuad(P1, C, P2);
            return new CubicBezier(coords.p1, coords.c1, coords.c2, coords.p2);
        }

        public void Reverse()
        {
            (P2, C, P1) = (P1, C, P2);
        }

        public object Clone()
        {
            return new QuadraticBezier(P1, C, P2);
        }

    }

}
