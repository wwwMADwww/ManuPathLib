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

            var solX = BezierMath.CubicBezierQuadRoots(P1.X, C1.X, C2.X, P2.X);
            var solY = BezierMath.CubicBezierQuadRoots(P1.Y, C1.Y, C2.Y, P2.Y);


            var coordsX = solX.Select(t => BezierMath.CubicBezierCoord(t, P1.X, C1.X, C2.X, P2.X));
            var coordsY = solY.Select(t => BezierMath.CubicBezierCoord(t, P1.Y, C1.Y, C2.Y, P2.Y));

            coordsX = coordsX.Concat(new[] { P1.X, P2.X }).ToArray();
            coordsY = coordsY.Concat(new[] { P1.Y, P2.Y }).ToArray();

            var minX = coordsX.Min();
            var minY = coordsY.Min();
             
            var maxX = coordsX.Max();
            var maxY = coordsY.Max();
            
            var rect = new RectangleF(new PointF(minX, minY), new SizeF(maxX - minX, maxY - minY));

            return rect;
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
