using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;

namespace ManuPath.DotGenerators.StrokeGenerators
{
    public abstract class StrokeDotGeneratorBase : IDotGenerator
    {
        private readonly IFigure _figure;

        public StrokeDotGeneratorBase(IFigure figure, bool transform)
        {
            if (figure.Stroke == null)
            {
                throw new ArgumentException("Figure must have Stroke");
            }

            // TODO: implement ellipse and rectangle
            _figure = (Path) figure.ToPath(transform);
            //var primitives = ((Path)_figure).Primitives;
            //for (int i = 0; i < primitives.Length; i++)
            //{ 
            //    var p = primitives[i];
            //    if (p is QuadraticBezier qb)
            //    { 
            //        primitives[i] = qb.ToCubicBezier();
            //    }
            //}
        }

        public GeneratedDots[] Generate()
        {
            Vector2[] dots;
            if (_figure is Path path)
            {
                dots = PathDivide(path);
            }
            else if (_figure is Rectangle rect)
            {
                dots = RectangleDivide(rect);
            }
            else if (_figure is Ellipse ellipse)
            {
                dots = EllipseDivide(ellipse);
            }
            else
            {
                throw new NotSupportedException($"Unknown figure type {_figure.GetType().Name}");
            }

            return new[] { new GeneratedDots()
            {
                Dots = dots,
                Color = _figure.Stroke.Color,
            }};
        }

        public Vector2[] PathDivide(Path path)
        {
            var dots = new List<Vector2>();

            Vector2? firstPoint = null;
            Vector2? lastPoint = null;

            foreach (var primitive in path.Primitives)
            {
                Vector2[] segs;

                if (primitive is Segment segment)
                {
                    segs = SegmentDivide(segment, lastPoint);
                }
                else if (primitive is CubicBezier cb)
                {
                    segs = CubicBezierToSegments(cb, lastPoint);
                }
                else if (primitive is QuadraticBezier qb)
                {
                    segs = QuadraticBezierToSegments(qb, lastPoint);
                }
                else
                {
                    throw new NotSupportedException($"Unknown primitive type {primitive.GetType().Name}");
                }

                if (segs.Length == 0)
                {
                    continue;
                }

                if (!firstPoint.HasValue)
                {
                    firstPoint = segs.First(); // cb.P1;
                }
                lastPoint = segs.Last(); // cb.P2;

                dots.AddRange(segs);
            }

            if (dots.Any())
            {
                var lastPrim = path.Primitives.Last();

                if (lastPoint.Value != lastPrim.LastPoint)
                {
                    dots.Add(lastPrim.LastPoint);
                    lastPoint = lastPrim.LastPoint;
                }
            }

            return dots.ToArray();
        }


        protected abstract Vector2[] CubicBezierToSegments(CubicBezier cubicBezier, Vector2? prevPoint = null);

        protected abstract Vector2[] QuadraticBezierToSegments(QuadraticBezier quadBezier, Vector2? prevPoint = null);

        protected abstract Vector2[] SegmentDivide(Segment segment, Vector2? prevPoint = null);

        protected abstract Vector2[] RectangleDivide(Rectangle rect, Vector2? prevPoint = null);

        protected abstract Vector2[] EllipseDivide(Ellipse ellipse, Vector2? prevPoint = null);

    }
}
