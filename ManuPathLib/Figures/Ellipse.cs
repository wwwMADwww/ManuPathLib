using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Extensions;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Maths;
using ManuPath.Transforms;

namespace ManuPath.Figures
{
    // public enum EllipseArcType { Open, Closed };

    public class Ellipse : IFigure, ICloneable
    {
        public Vector2 Center { get; set; }

        public Vector2 Radius { get; set; }

        // public float ArcStartAngle { get; set; } = 0f;
        // public float ArcEndAngle { get; set; } = (float) (2f * Math.PI);
        // public EllipseArcType ArcType { get; } = EllipseArcType.NotConnected; 

        public string Id { get; set; }
        public Fill Fill { get; set; }
        public Stroke Stroke { get; set; }

        public Vector2 FirstPoint => CommonMath.EllipseCoord(Center, Radius, 0);

        public Vector2 LastPoint => FirstPoint;

        public RectangleF GetBounds()
        {
            return new RectangleF(
                Center.X - Radius.X,
                Center.Y - Radius.Y,
                Radius.X * 2,
                Radius.Y * 2);
        }

        public ITransform[] Transforms { get; set; }

        public IFigure Transform()
        {
            return ToPath(true);
        }

        public object Clone()
        {
            return new Ellipse()
            {
                Id = Id,
                Center = Center,
                Radius = Radius,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone(),
                Transforms = Transforms
            };
        }

        public void Reverse() { }

        public IFigure ToPath(bool transformed)
        {
            var bezierCoords = BezierMath.CubicBeziersFromEllipse(Center, Radius);

            var beziers = bezierCoords.Select(b => (IPathPrimitive) new CubicBezier(b.p1, b.c1, b.c2, b.p2));

            if (transformed && Transforms.IsNotNullOrEmpty())
            {
                beziers = beziers.Select(f => f.Transform(Transforms));
            }

            return new Path()
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone(),
                Primitives = beziers.ToArray(),
                Transforms = transformed ? null : Transforms
            };
        }
    }
}
