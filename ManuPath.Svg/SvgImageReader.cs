using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;
using Svg.Transforms;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Transforms;
using ManuPath.Extensions;
using Rectangle = ManuPath.Figures.Rectangle;

namespace ManuPath.Svg
{

    public class SvgImageReader
    {

        public static ManuPathImage ReadSvgFile(string filename)
        {
            var svg = SvgDocument.Open(filename);

            var svgElements = svg.Children.FindSvgElementsOf<SvgGroup>() // layers, groups
                .SelectMany(g => g.Children)
                .Where(x => !(x is SvgGroup))
                .ToArray();

            var figures = ConvertSvgElementsToFigures(svgElements);

            return new ManuPathImage()
            {
                Figures = figures,
                // Deliberately ignoring measurement units.
                // If size is not right, check ViewBox.
                Size = new Vector2(svg.Width.Value, svg.Height.Value)
            };
        }

        public static IFigure[] ConvertSvgElementsToFigures(SvgElement[] svgElements)
        {
            var figures = new List<IFigure>();

            foreach (var svgElement in svgElements)
            {
                IFigure? figure;

                if (svgElement is SvgPath path)
                {
                    figure = ConvertPath(path);
                }
                else if (svgElement is SvgRectangle rect)
                {
                    figure = new Rectangle(rect.X.Value, rect.Y.Value, rect.Width.Value, rect.Height.Value);
                }
                else if (svgElement is SvgCircle circle)
                {
                    figure = new Ellipse()
                    {
                        Center = new Vector2(circle.CenterX.Value, circle.CenterY.Value),
                        Radius = new Vector2(circle.Radius.Value)
                    };
                }
                else if (svgElement is SvgEllipse ellipse)
                {
                    figure = new Ellipse()
                    {
                        Center = new Vector2(ellipse.CenterX.Value, ellipse.CenterY.Value),
                        Radius = new Vector2(ellipse.RadiusX.Value, ellipse.RadiusY.Value)
                    };
                }
                else if (svgElement is SvgText text)
                {
                    Console.WriteLine($"Element id '{svgElement.ID}': type '{svgElement.GetType().Name}' not supported, skipped");
                    continue;
                }
                else
                {
                    Console.WriteLine($"Element id '{svgElement.ID}': type '{svgElement.GetType().Name}' not supported, skipped");
                    continue;
                }

                if (figure == null) continue;

                SetCommonProperties(figure, svgElement);
                figures.Add(figure);
            }

            return figures.ToArray();
        }

        public static void SetCommonProperties(IFigure figure, SvgElement svgElement)
        {
            figure.Id = svgElement.ID;

            var fillColor = ((SvgColourServer)svgElement.Fill)?.Colour ?? Color.Transparent;

            figure.Fill = svgElement.Fill == SvgPaintServer.None ? null : new Fill()
            {
                Color = Color.FromArgb((byte)(255 * svgElement.FillOpacity), fillColor.R, fillColor.G, fillColor.B),
                Rule = svgElement.FillRule == SvgFillRule.EvenOdd
                    ? FillRule.EvenOdd
                    : FillRule.NonZeroWinding
            };

            var strokeColor = ((SvgColourServer)svgElement.Stroke)?.Colour ?? Color.Transparent;

            figure.Stroke = svgElement.Stroke == SvgPaintServer.None ? null : new Stroke()
            {
                Color = Color.FromArgb((byte)(255 * svgElement.StrokeOpacity), strokeColor.R, strokeColor.G, strokeColor.B)
            };

            var transforms = svgElement.ParentsAndSelf
                .OfType<ISvgTransformable>()
                .Where(st => st.Transforms.IsNotNullOrEmpty())
                .Select(st => st.Transforms.AsEnumerable())
                //.Reverse()
                .SelectMany(ts => ts)
                .Select(ConvertTransform)
                .ToArray();

            figure.Transforms = transforms;
        }

        public static IFigure? ConvertPath(SvgPath svgPath)
        {
            var primitives = new List<IPathPrimitive>();

            Vector2? contourStart = null;

            foreach (var svgPathSegment in svgPath.PathData)
            {
                if (svgPathSegment is SvgCubicCurveSegment cb)
                {
                    var p = new CubicBezier(
                        new Vector2(cb.Start.X, cb.Start.Y),
                        new Vector2(cb.FirstControlPoint.X, cb.FirstControlPoint.Y),
                        new Vector2(cb.SecondControlPoint.X, cb.SecondControlPoint.Y),
                        new Vector2(cb.End.X, cb.End.Y)
                        );

                    primitives.Add(p);
                }
                else if (svgPathSegment is SvgQuadraticCurveSegment qb)
                {
                    var p = new QuadraticBezier(
                        new Vector2(qb.Start.X, qb.Start.Y),
                        new Vector2(qb.ControlPoint.X, qb.ControlPoint.Y),
                        new Vector2(qb.End.X, qb.End.Y)
                        )
                        //.ToCubicBezier()
                        ;
                    primitives.Add(p);
                }
                else if (svgPathSegment is SvgLineSegment line)
                {
                    var p = new Segment(new Vector2(line.Start.X, line.Start.Y), new Vector2(line.End.X, line.End.Y));
                    primitives.Add(p);
                }
                else if (svgPathSegment is SvgArcSegment arc)
                {
                    // TODO: arcs
                    Console.WriteLine($"Path id '{svgPath.ID}': Arcs not implemented, skipped");
                }
                else if (svgPathSegment is SvgClosePathSegment closePath)
                {
                    if (!primitives.Any())
                    {
                        contourStart = null;
                        continue;
                    }

                    var lastPoint = primitives.Last().LastPoint;
                    if (lastPoint != contourStart)
                    {
                        var p = new Segment(primitives.Last().LastPoint, contourStart.Value);
                        primitives.Add(p);
                    }
                    contourStart = null;
                }
                else if (svgPathSegment is SvgMoveToSegment move)
                {
                    contourStart = new Vector2(move.Start.X, move.Start.Y);
                }
                else
                {
                    Console.WriteLine($"Path id '{svgPath.ID}': element type '{svgPathSegment.GetType().Name}' not supported, skipped");
                }
            }

            if (primitives.Count == 0)
            {
                return null;
            }

            var newPath = new Path()
            {
                Id = svgPath.ID,
                Primitives = primitives.ToArray(),
            };

            return newPath;
        }

        public static ITransform ConvertTransform(SvgTransform svgTransform)
        {
            if (svgTransform is SvgTranslate trans)
            {
                var t = new TranslateTransform(trans.X, trans.Y);
                return t;
            }
            else if (svgTransform is SvgScale scale)
            {
                var t = new ScaleTransform(scale.X, scale.Y);
                return t;
            }
            else if (svgTransform is SvgRotate rotate)
            {
                var t = new RotateTransform(new Vector2(rotate.CenterX, rotate.CenterY), rotate.Angle);
                return t;
            }
            // else if (svgTransform is SvgSkew skew)
            // { 
            //     // TODO
            // }
            else
            {
                // any transform could be represented as matrix transform

                var m = svgTransform.Matrix.Elements;

                var matrix = new Matrix3x2(
                    m[0], m[1], m[2], 
                    m[3], m[4], m[5]
                );

                var t = new MatrixTransform(matrix);
                return t;
            }
        }

    }
}
