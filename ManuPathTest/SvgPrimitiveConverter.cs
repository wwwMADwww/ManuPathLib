using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Svg;
using Svg.Pathing;
using ManuPath;

namespace ManuPathTest
{

    public class ImageInfo
    {
        public IEnumerable<Path> Paths{ get; set; }
        public Vector2 Size { get; set; }
    }

    public static class SvgPrimitiveConverter
    {

        public static ImageInfo ReadSvg(string filename)
        {

            var paths = new List<Path>();

            var svg = SvgDocument.Open(filename);


            var elements = svg.Children.FindSvgElementsOf<SvgGroup>() // layers, groups
                .SelectMany(g => g.Children.FindSvgElementsOf<SvgPath>()) // paths
                .ToArray();


            foreach (var e in elements)
            {
                var primitives = new List<IPathPrimitive>();

                foreach (var path in e.PathData)
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

                var fillColor = ((SvgColourServer)e.Fill)?.Colour ?? Color.Transparent;
                var strokeColor = ((SvgColourServer)e.Stroke)?.Colour ?? Color.Transparent;

                paths.Add(new Path() 
                { 
                    Id = e.ID,

                    Primitives = primitives, 

                    FillRule = e.FillRule == SvgFillRule.EvenOdd 
                        ? PathFillRule.EvenOdd 
                        : PathFillRule.NonZeroWinding,

                    FillColor = e.Fill != SvgPaintServer.None
                        ? Color.FromArgb((byte) (255 * e.FillOpacity), fillColor.R, fillColor.G, fillColor.B)
                        : (Color?) null,

                    StrokeColor = e.Stroke != SvgPaintServer.None 
                        ? Color.FromArgb((byte)(255 * e.StrokeOpacity), strokeColor.R, strokeColor.G, strokeColor.B)
                        : (Color?)null,

                });

            }


            return new ImageInfo() {
                Paths = paths, 
                Size = new Vector2(svg.ViewBox.Width, svg.ViewBox.Height)
            };
        }

    }
}
