using System;
using System.Drawing;
using System.Numerics;
using ManuPath.Maths;

namespace ManuPath.Figures.Primitives
{
    public class Segment : IPathPrimitive
    {
        public Segment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        public Vector2 FirstPoint => P1;
        public Vector2 LastPoint => P2;

        public bool IsZeroLength => P1 - P2 == Vector2.Zero;

        public float Length => CommonMath.Distance(P1, P2);

        public RectangleF Bounds
        {
            get
            {
                var xmin = Math.Min(P1.X, P2.X);
                var xmax = Math.Max(P1.X, P2.X);

                var ymin = Math.Min(P1.Y, P2.Y);
                var ymax = Math.Max(P1.Y, P2.Y);

                return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
            }
        }

        public void Reverse()
        {
            (P2, P1) = (P1, P2);
        }


        public override bool Equals(object obj)
        {
            return obj is Segment segment &&
                   P1.Equals(segment.P1) &&
                   P2.Equals(segment.P2);
        }

        public bool Equals(IPathPrimitive other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            int hashCode = 162377905;
            hashCode = hashCode * -1521134295 + P1.GetHashCode();
            hashCode = hashCode * -1521134295 + P2.GetHashCode();
            return hashCode;
        }

    }

}
