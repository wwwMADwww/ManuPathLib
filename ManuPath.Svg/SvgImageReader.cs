using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;
using ManuPath.Figures;
using ManuPath.Figures.Primitives;
using ManuPath;
using Rectangle = ManuPath.Figures.Rectangle;

namespace ManuPath.Svg
{

    public class SvgImageReader
    {

        public ManuPathImage ReadSvg(string filename)
        {

            var figures = new List<IFigure>();

            var svg = SvgDocument.Open(filename);


            var elements = svg.Children.FindSvgElementsOf<SvgGroup>() // layers, groups
                .SelectMany(g => g.Children)
                .ToArray();



            foreach (var element in elements)
            {
                IFigure figure;

                if (element is SvgPath path)
                {
                    figure = ConvertPath(path);
                }
                else if (element is SvgRectangle rect)
                {
                    figure = new Rectangle(
                        new Vector2(rect.X.Value, rect.Y.Value),
                        new Vector2(rect.Width.Value, rect.Height.Value));
                }
                else if (element is SvgCircle circle)
                {
                    figure = new Ellipse()
                    {
                        Center = new Vector2(circle.CenterX.Value, circle.CenterY.Value),
                        Radius = new Vector2(circle.Radius.Value)
                    };
                }
                else if (element is SvgEllipse ellipse)
                {
                    figure = new Ellipse()
                    {
                        Center = new Vector2(ellipse.CenterX.Value, ellipse.CenterY.Value),
                        Radius = new Vector2(ellipse.RadiusX.Value, ellipse.RadiusY.Value)
                    };
                }
                else
                {
                    Console.WriteLine($"Element type '{element.GetType().Name}' not supported");
                    continue;
                }

                SetCommonProperties(figure, element);
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
        }

        private IFigure ConvertPath(SvgPath svgPath)
        {
            var primitives = new List<IPathPrimitive>();

            foreach (var path in svgPath.PathData)
            {
                if (path is SvgCubicCurveSegment cb)
                {
                    var p = new CubicBezier(
                        new Vector2(cb.Start.X, cb.Start.Y),
                        new Vector2(cb.End.X, cb.End.Y),
                        new Vector2(cb.FirstControlPoint.X, cb.FirstControlPoint.Y),
                        new Vector2(cb.SecondControlPoint.X, cb.SecondControlPoint.Y)
                        );

                    primitives.Add(p);
                }
                else if (path is SvgLineSegment line)
                {
                    var p = new Segment(new Vector2(line.Start.X, line.Start.Y), new Vector2(line.End.X, line.End.Y));
                    primitives.Add(p);
                }
                else if (path is SvgMoveToSegment move)
                {
                    // skip
                }
                else if (path is SvgClosePathSegment closePath)
                {
                    if (!primitives.Any()) // occurs when path is not line or bezier
                        continue;
                    var p = new Segment(primitives.Last().LastPoint, primitives.First().FirstPoint);
                    primitives.Add(p);
                }
                else
                {
                    Console.WriteLine($"path type '{path.GetType().Name}' not supported");
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

    }
}
