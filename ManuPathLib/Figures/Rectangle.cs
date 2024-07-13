using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using ManuPath.Extensions;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Maths;
using ManuPath.Transforms;

namespace ManuPath.Figures
{
    public class Rectangle : IFigure
    {
        public Rectangle() { }

        public Rectangle(float x, float y, float width, float height)
        {
            Pos = new Vector2(x, y);
            Size = new Vector2(width, height);
        }

        public Vector2 Pos { get; private set; }

        public Vector2 Size { get; set; }

        public float Left => Pos.X;
        public float Top => Pos.Y;
        public float Right => Pos.X + Size.X;
        public float Bottom => Pos.Y + Size.Y;
        public float Width => Size.X;
        public float Height => Size.Y;

        public Vector2 FirstPoint => Pos;

        public Vector2 LastPoint => Pos;

        public RectangleF GetBounds()
        {
            return new RectangleF(Pos.X, Pos.Y, Size.X, Size.Y);
        }

        public ITransform[] Transforms { get; set; }

        public IFigure Transform()
        {
            if (Transforms?.Any(t => t is MatrixTransform || t is RotateTransform) == true)
            {
                return ToPath(true);
            }
            else
            {
                var rect = (Rectangle)Clone();
                foreach (var trans in Transforms.EmptyIfNull())
                {
                    var pivot = rect.Pos;
                    rect.Pos = trans.Transform(pivot, rect.Pos);
                    rect.Size = trans.Transform(pivot, rect.Size);
                }
                rect.Transforms = null;
                return rect;
            }
        }

        public string Id { get; set; }

        public Fill Fill { get; set; }
        public Stroke Stroke { get; set; }

        public void Reverse() { }

        public object Clone()
        {
            return new Rectangle()
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone(),
                Pos = Pos,
                Size = Size
            };
        }

        public IFigure ToPath(bool transformed)
        {
            var segments = CommonMath.GetRectangleSegments(new RectangleF(Pos.X, Pos.Y, Size.X, Size.Y))
                .Select(s => new Segment(s.p1, s.p2))
                .ToArray();

            var path = new Path()
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone(),
                Primitives = segments,
                Transforms = Transforms
            };

            if (transformed && Transforms.IsNotNullOrEmpty())
            {
                path = (Path) path.Transform();
            }

            return path;
        }
    }

}
