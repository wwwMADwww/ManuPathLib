using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath
{

    public class Path : ICloneable
    {
        public string Id { get; set; }

        public IEnumerable<IPathPrimitive> Primitives { get; set; }

        public Color? FillColor { get; set; }
        public PathFillRule FillRule { get; set; }

        public Color? StrokeColor { get; set; }

        public RectangleF Bounds
        {
            get
            {
                var allbounds = Primitives.Select(p => p.Bounds).DefaultIfEmpty(RectangleF.Empty).ToArray();
                var (xmin, ymin) = (allbounds.Min(r => r.Left), allbounds.Min(r => r.Top));
                var (xmax, ymax) = (allbounds.Max(r => r.Right), allbounds.Max(r => r.Bottom));

                return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
            }
        }

        public void Reverse()
        {
            var reversed = Primitives.ToList();
            reversed.ForEach(p => p.Reverse());
            reversed.Reverse();
            Primitives = reversed;
        }

        public object Clone()
        {
            return new Path()
            {
                Id = Id,

                Primitives = Primitives,

                FillColor = FillColor,
                FillRule = FillRule,

                StrokeColor = StrokeColor,
            };
        }
    }

    public enum PathFillRule { EvenOdd, NonZeroWinding }



    public interface IPathPrimitive: IEquatable<IPathPrimitive>
    {
        Vector2 FirstPoint { get; }
        Vector2 LastPoint { get; }
        RectangleF Bounds { get; }

        void Reverse();
    }



    // TODO: make all this immutable?

    public class Dot : IPathPrimitive
    {
        public Dot(Vector2 pos)
        {
            Pos = pos;
        }

        public Vector2 Pos { get; set; }

        public Vector2 FirstPoint => Pos;
        public Vector2 LastPoint => Pos;

        public RectangleF Bounds => new RectangleF(Pos.X, Pos.Y, 0, 0);


        public void Reverse() { }


        public override bool Equals(object obj)
        {
            return obj is Dot dot &&
                   Pos.Equals(dot.Pos);
        }

        public bool Equals(IPathPrimitive other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            return 1731973265 + Pos.GetHashCode();
        }

    }



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
            get {
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
