using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Extensions;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Transforms;

namespace ManuPath.Figures
{

    public class Path : IFigure
    {
        public string Id { get; set; }

        public IPathPrimitive[] Primitives { get; set; }

        public Fill Fill { get; set; }

        public Stroke Stroke { get; set; }

        public RectangleF GetBounds()
        {
            var allbounds = Primitives
                .Select(p => p.GetBounds())
                .DefaultIfEmpty(RectangleF.Empty)
                .ToArray();

            var (xmin, ymin) = (allbounds.Min(r => r.Left) , allbounds.Min(r => r.Top));
            var (xmax, ymax) = (allbounds.Max(r => r.Right), allbounds.Max(r => r.Bottom));

            return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public ITransform[] Transforms { get; set; }

        public IFigure Transform()
        {
            var path = CreateEmpty();

            path.Primitives = Primitives
                .Select(p => p.Transform(Transforms.EmptyIfNull()) )
                .ToArray();

            return path;
        }

        public Vector2 FirstPoint => Primitives.First().FirstPoint;

        public Vector2 LastPoint => Primitives.Last().LastPoint;

        public void Reverse()
        {
            var reversed = Primitives
                .Each(p => p.Reverse())
                .Reverse()
                .ToArray();

            Primitives = reversed;
        }

        public Path CreateEmpty()
        {
            return new Path()
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone()
            };
        }

        public object Clone() => ToPath(false);

        public IFigure ToPath(bool transformed)
        {
            return new Path()
            {
                Id = Id,
                Fill = Fill?.Clone(),
                Stroke = Stroke?.Clone(),
                Primitives = Primitives
                    .Select(p => (transformed && Transforms.IsNotNullOrEmpty()) 
                        ? p.Transform(Transforms) 
                        : p.Clone())
                    .Cast<IPathPrimitive>()
                    .ToArray(),
                Transforms = transformed ? null : Transforms
            };
        }
    }

}
