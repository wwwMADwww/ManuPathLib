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

        public ManuPathImage ReadSvg(string filename)
        {

            var figures = new List<IFigure>();

            var svg = SvgDocument.Open(filename);
            
            var svgElements = svg.Children.FindSvgElementsOf<SvgGroup>() // layers, groups
                .SelectMany(g => g.Children)
                .Where(x => ! (x is SvgGroup))
                .ToArray();

            foreach (var svgElement in svgElements)
            {
                IFigure figure;

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
                else
                {
                    Console.WriteLine($"Element id '{svgElement.ID}': type '{svgElement.GetType().Name}' not supported, skipped");
                    continue;
                }

                SetCommonProperties(figure, svgElement);
                figures.Add(figure);
            }


            return new ManuPathImage() 
            {
                Figures = figures.ToArray(), 
                Size = new Vector2(svg.ViewBox.Width, svg.ViewBox.Height)
            };
        }

        private void SetCommonProperties(IFigure figure, SvgElement svgElement)
        {
            figure.Id = svgElement.ID;

            var fillColor = ((SvgColourServer)svgElement.Fill)?.Colour ?? Color.Transparent;
            var strokeColor = ((SvgColourServer)svgElement.Stroke)?.Colour ?? Color.Transparent;

            figure.Fill = svgElement.Fill == SvgPaintServer.None ? null : new Fill()
            {
                Color = Color.FromArgb((byte)(255 * svgElement.FillOpacity), fillColor.R, fillColor.G, fillColor.B),
                Rule = svgElement.FillRule == SvgFillRule.EvenOdd
                    ? FillRule.EvenOdd
                    : FillRule.NonZeroWinding
            };

            figure.Stroke = svgElement.Stroke == SvgPaintServer.None ? null : new Stroke()
            {
                Color = Color.FromArgb((byte)(255 * svgElement.StrokeOpacity), strokeColor.R, strokeColor.G, strokeColor.B)
            };

            var transforms = svgElement.ParentsAndSelf
                .OfType<ISvgTransformable>()
                .Where(st => st.Transforms.IsNotNullOrEmpty())
                .Select(st => st.Transforms.AsEnumerable())
                .Reverse()
                .SelectMany(ts => ts)
                .Select(ConvertTransform)
                .ToArray();

            figure.Transforms = transforms;
        }

        private IFigure ConvertPath(SvgPath svgPath)
        {
            var primitives = new List<IPathPrimitive>();

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
                else if (svgPathSegment is SvgLineSegment line)
                {
                    var p = new Segment(new Vector2(line.Start.X, line.Start.Y), new Vector2(line.End.X, line.End.Y));
                    primitives.Add(p);
                }
                else if (svgPathSegment is SvgMoveToSegment move)
                {
                    // skip
                }
                else if (svgPathSegment is SvgClosePathSegment closePath)
                {
                    if (!primitives.Any()) // occurs when path is not line or bezier
                        continue;
                    var p = new Segment(primitives.Last().LastPoint, primitives.First().FirstPoint);
                    primitives.Add(p);
                }
                else if (svgPathSegment is SvgArcSegment arc)
                {
                    // TODO: arcs
                    Console.WriteLine($"Path id '{svgPath.ID}': Arcs not implemented, skipped");
                }
                else
                {
                    Console.WriteLine($"Path id '{svgPath.ID}': element type '{svgPathSegment.GetType().Name}' not supported, skipped");
                }
            }

            var newPath = new Path()
            {
                Id = svgPath.ID,

                Primitives = primitives.ToArray(),
            };

            SetCommonProperties(newPath, svgPath);

            return newPath;

        }

        static ITransform ConvertTransform(SvgTransform svgTransform)
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
