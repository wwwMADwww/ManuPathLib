using System;
using System.Drawing;
using System.Numerics;
using ManuPath.Extensions;
using ManuPath.Maths;
using ManuPath.Transforms;

namespace ManuPath.Figures.PathPrimitives
{
    public class Segment : IPathPrimitive
    {
        public Segment() { }

        public Segment(float x1, float y1, float x2, float y2)
            : this(new Vector2(x1, y1), new Vector2(x2, y2)) { }

        public Segment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        public Vector2 FirstPoint => P1;
        public Vector2 LastPoint => P2;

        public bool IsZeroLength => 
            CommonMath.IsFloatEquals(P1.X, P2.X) && 
            CommonMath.IsFloatEquals(P1.Y, P2.Y);

        public float Length => CommonMath.Distance(P1, P2);

        public RectangleF GetBounds()
        {
            var xmin = Math.Min(P1.X, P2.X);
            var xmax = Math.Max(P1.X, P2.X);

            var ymin = Math.Min(P1.Y, P2.Y);
            var ymax = Math.Max(P1.Y, P2.Y);

            return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public IPathPrimitive Transform(ITransform[] transforms)
        {
            var seg = (Segment)Clone();

            foreach (var transform in transforms.EmptyIfNull())
            {
                seg.P1 = transform.Transform(seg.P1);
                seg.P2 = transform.Transform(seg.P2);
            }

            return seg;
        }

        public void Reverse()
        {
            (P2, P1) = (P1, P2);
        }

        public object Clone()
        {
            return new Segment(P1, P2);
        }
    }

}
