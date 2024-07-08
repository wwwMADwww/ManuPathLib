using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using ManuPath.Figures.Primitives;
using ManuPath.Maths;

namespace ManuPath.Figures
{
    public enum EllipseDirection { Clockwise, Counterclockwise };

    public class Ellipse : IFigure, ICloneable
    {
        public Vector2 Center { get; set; }

        public Vector2 Radius { get; set; }

        public float StartAngle { get; set; } = 0f;

        public EllipseDirection Direction { get; private set; } = EllipseDirection.Clockwise; 


        public string Id { get; set; }
        public Fill Fill { get; set; }
        public Stroke Stroke { get; set; }

        public Vector2 FirstPoint => CommonMath.EllipseCoord(Center, Radius, StartAngle);

        public Vector2 LastPoint => FirstPoint;

        public RectangleF Bounds => new RectangleF(
            Center.X - Radius.X,
            Center.Y - Radius.Y,
            Radius.X * 2,
            Radius.Y * 2);

        public object Clone()
        {
            return new Ellipse()
            {
                Id = Id,
                Center = Center,
                Direction = Direction,
                Radius = Radius,
                StartAngle = StartAngle,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone()
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Ellipse ellipse &&
                   Center.Equals(ellipse.Center) &&
                   Radius.Equals(ellipse.Radius) &&
                   StartAngle == ellipse.StartAngle &&
                   Direction == ellipse.Direction;
        }

        public bool Equals(IPathPrimitive other) => Equals((object)other);

        public void Reverse()
        {
            Direction = Direction == EllipseDirection.Clockwise
                ? EllipseDirection.Counterclockwise
                : EllipseDirection.Clockwise;
        }

        public Path ToPath()
        {
            throw new NotImplementedException();
        }
    }
}
