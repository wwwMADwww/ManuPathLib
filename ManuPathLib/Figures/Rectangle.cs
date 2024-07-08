using System;
using System.Drawing;
using System.Numerics;
using ManuPath.Figures.Primitives;

namespace ManuPath.Figures
{
    public class Rectangle : IFigure
    {
        public Rectangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
        }
        
        public Rectangle(Vector2 pos, Vector2 size)
        {
            P1 = pos;
            P2 = new Vector2(pos.X + size.X, pos.Y);
            P3 = new Vector2(pos.X + size.X, pos.Y + size.Y);
            P4 = new Vector2(pos.X, pos.Y + size.Y);
        }

        public Vector2 P1 { get; private set; }
        public Vector2 P2 { get; private set; }
        public Vector2 P3 { get; private set; }
        public Vector2 P4 { get; private set; }

        public float Width => P2.X - P1.X;

        public float Height => P2.Y - P1.Y;

        public Vector2 FirstPoint => P1;

        public Vector2 LastPoint => P4;

        public RectangleF Bounds => new RectangleF(P1.X, P1.Y, Width, Height);

        public string Id { get; set; }

        public Fill Fill { get; set; }
        public Stroke Stroke { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle &&
                   P1.Equals(rectangle.P1) &&
                   P2.Equals(rectangle.P2) &&
                   P3.Equals(rectangle.P3) &&
                   P4.Equals(rectangle.P4);
        }

        public bool Equals(IPathPrimitive other) => Equals((object)other);

        public void Reverse()
        {
            (P1, P2, P3, P4) = (P4, P3, P2, P1);
        }

        public object Clone()
        {
            return new Rectangle(P1, P2, P3, P4)
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone()
            };
        }

        public Path ToPath()
        {
            return new Path()
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone(),
                Primitives = new[] 
                {
                    new Segment(P1, P2),
                    new Segment(P2, P3),
                    new Segment(P3, P4),
                    new Segment(P4, P1)
                }
            };
        }
    }

}
