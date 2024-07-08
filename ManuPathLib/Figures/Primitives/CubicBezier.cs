using System.Drawing;
using System.Linq;
using System.Numerics;
using ManuPath.Maths;

namespace ManuPath.Figures.Primitives
{
    public class CubicBezier : IPathPrimitive
    {
        public CubicBezier(Vector2 p1, Vector2 p2, Vector2 c1, Vector2 c2)
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



        public RectangleF Bounds
        {
            get
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
        }

        public void Reverse()
        {
            (P2, C2, C1, P1) = (P1, C1, C2, P2);
        }


        public override bool Equals(object obj)
        {
            return obj is CubicBezier bezier &&
                   P1.Equals(bezier.P1) &&
                   P2.Equals(bezier.P2) &&
                   C1.Equals(bezier.C1) &&
                   C2.Equals(bezier.C2);
        }

        public bool Equals(IPathPrimitive other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            int hashCode = -176715934;
            hashCode = hashCode * -1521134295 + P1.GetHashCode();
            hashCode = hashCode * -1521134295 + P2.GetHashCode();
            hashCode = hashCode * -1521134295 + C1.GetHashCode();
            hashCode = hashCode * -1521134295 + C2.GetHashCode();
            return hashCode;
        }

    }

}
