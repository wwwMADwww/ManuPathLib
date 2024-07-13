using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using DColor = System.Drawing.Color;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;
using ManuPath.DotGenerators.FillGenerators;
using ManuPath.DotGenerators.StrokeGenerators;
using ManuPath.Svg;
using ManuPath.Maths;
using RectangleF = System.Drawing.RectangleF;
using ManuPath.Extensions;

namespace ManuPathTest
{
    class Program
    {


        public static void Main(string[] args)
        {

            Console.WriteLine("Hello World!");


            #region select test image

            // var filename = (@"..\..\..\svg\test.svg");
            // var filename = (@"..\..\..\svg\bounds.svg");
            // var filename = (@"..\..\..\svg\groups and layers.svg");
            // var filename = (@"..\..\..\svg\curvature.svg");
            // var filename = (@"..\..\..\svg\curvature 2.svg");
            // var filename = (@"..\..\..\svg\loop.svg");
            // var filename = (@"..\..\..\svg\loop2.svg");
            // var filename = (@"..\..\..\svg\loop22.svg");
            var filename = (@"..\..\..\svg\polygon.svg");
            // var filename = (@"..\..\..\svg\polygon 2.svg");
            // var filename = (@"..\..\..\svg\polygon 3.svg");
            // var filename = (@"..\..\..\svg\polygon 4.svg");
            // var filename = (@"..\..\..\svg\spiral.svg");
            // var filename = (@"..\..\..\svg\hor.svg");
            // var filename = (@"..\..\..\svg\vert.svg");
            // var filename = (@"..\..\..\svg\solidsquare.svg");
            // var filename = (@"..\..\..\svg\solidsquare2.svg");
            // var filename = (@"..\..\..\svg\solidsquares.svg");
            // var filename = (@"..\..\..\svg\bubles.svg");
            // var filename = (@"..\..\..\svg\not beziers.svg");
            // var filename = (@"..\..\..\svg\stitches.svg");
            // var filename = (@"..\..\..\svg\stitches2.svg");
            // var filename = (@"..\..\..\svg\stitches3.svg");
            // var filename = (@"..\..\..\svg\stitches4.svg");
            // var filename = (@"..\..\..\svg\shades.svg");
            // var filename = (@"..\..\..\svg\random 2.svg");
            // var filename = (@"..\..\..\svg\transform.svg");
            // var filename = (@"..\..\..\svg\transform 2.svg");
            var filename = (@"..\..\..\svg\transform 3.svg");
            // var filename = (@"..\..\..\svg\transform 4.svg");
            // var filename = (@"..\..\..\svg\transform 5.svg");


            var vectorImage = new SvgImageReader().ReadSvg(filename);

            vectorImage.Figures = vectorImage.Figures;

            var figures = vectorImage.Figures
                .Select(f => f.Transform())
                .ToArray();

            #endregion


            #region scaling

            var scale = new Vector2f(1f, 1f);
            var scale2 = new Vector2(scale.X, scale.Y);

            
            void UpdateScale(Vector2 image, Vector2u boundingBox)
            {
                float widthScale = 0, heightScale = 0;
                if (image.X != 0)
                    widthScale = boundingBox.X / image.X;
                if (image.Y != 0)
                    heightScale = boundingBox.Y / image.Y;

                var scalemin = Math.Min(widthScale, heightScale);

                scale2 = new Vector2(scalemin, scalemin);
                scale = new Vector2f(scale2.X, scale2.Y);
            }

            #endregion


            #region SFML window stuff

            var resolutionX = 800;
            var resolutionY = 600; 

            var window = new RenderWindow(new VideoMode((uint) resolutionX, (uint)resolutionY), "Manual path test app");
            window.KeyReleased += (o, e) => { if (e.Code == Keyboard.Key.Escape) window.Close(); };
            window.Closed += (_, __) => window.Close();

            // window.SetVerticalSyncEnabled(true);
            // window.SetFramerateLimit(15);

            UpdateScale(vectorImage.Size, window.Size);

            window.Resized += (o, e) => UpdateScale(vectorImage.Size, new Vector2u(e.Width, e.Height));

            window.Resized += (o, e) =>
                window.SetView(new View(new Vector2f(e.Width / 2, e.Height / 2), new Vector2f(e.Width, e.Height)));

            #endregion


            #region preparing strokes

            var startFigure = figures.First();
            var sortedFigures = new List<IFigure>() { startFigure };
            var unsortedFigures = figures.Skip(1).ToList();

            while (unsortedFigures.Any())
            {
                var sortedLast = sortedFigures.Last().LastPoint;

                var ordered = unsortedFigures.Select(b => {
                    var bFirst = b.FirstPoint;
                    var bLast = b.LastPoint;

                    var distanceStraight = Math.Sqrt(Math.Pow(bFirst.X - sortedLast.X, 2) + Math.Pow(bFirst.Y - sortedLast.Y, 2));

                    if (bFirst == bLast)
                    {
                        return (path: b, distance: distanceStraight, reverse: false);
                    }

                    var distanceReverse = Math.Sqrt(Math.Pow( bLast.X - sortedLast.X, 2) + Math.Pow( bLast.Y - sortedLast.Y, 2));

                    if (distanceStraight > distanceReverse)
                    {
                        return (path: b, distance: distanceStraight, reverse: false);
                    }
                    else
                    {
                        return (path: b, distance: distanceReverse, reverse: true);
                    }
                })
                .OrderBy(x => x.distance)
                .ToArray();

                var p = ordered.First();
                if (p.reverse)
                {
                    p.path.Reverse();
                }
                var closestPath = p.path;

                sortedFigures.Add(closestPath);
                unsortedFigures.Remove(closestPath);
            }

            figures = sortedFigures.ToArray();

            //var strokeConverter = new PrimitiveToNSegmentsConverter(10);

            //var segDistance = Math.Max(6/scale.X, 6/scale.Y);
            var strokeDotsDistance = Math.Max(2/scale.X, 2/scale.Y);
            
            var figuresStrokeDots = figures
                .Where(p => p.Stroke != null)
                .SelectMany(f =>
                {
                    var strokeConverter = new EqualDistanceStrokeDotGenerator(f, false, strokeDotsDistance - strokeDotsDistance/10, strokeDotsDistance + strokeDotsDistance / 10);
                    var dots = strokeConverter.Generate();
                    return dots;
                })
                .ToArray();

            #endregion


            #region preparing fill polygons

            // // dividing all beziers to series of line segments
            // var fillConverter = new PrimitiveToEvenSegmentsConverter(4f, 5f, true);
            // var fillConverter = new PrimitiveToNSegmentsConverter(10, true);
            // 
            // var pathsFillSegs = svgImageInfo.Paths.Where(p => p.FillColor.HasValue)
            //    .Select(p =>
            //    {
            //        var sp = (Path)p.Clone();
            //        sp.Primitives = fillConverter.Convert(p.Primitives);
            //        return sp;
            //    })
            //    .ToArray();

            // just removing paths without defined fill
            var figuresWithFill = figures
                .Where(p => p.Fill != null)
                .ToArray();


            // fill generation strategy

            var fillscale = scale2;

            var figuresFills = figuresWithFill
                //.Select(p => new RandomDotsFillGenerator(1000, true, true, p).GenerateFill())
                .SelectMany(p => new IntervalFillDotGenerator(
                    p,
                    false,
                    new Vector2(0.2f, 0.2f) * fillscale,
                    new Vector2(0.05f, 0.05f) * fillscale,
                    new Vector2(0.4f, 0.4f) * fillscale,
                    new Vector2(0.1f, 0.1f) * fillscale
                    ).Generate())
                ;//.ToArray();

            #endregion


            #region debug stuff

            // bounding boxes
            // var pathBounds = pathsSegments
            var figuresBounds = figures
                .Where(f => f.Stroke != null)
                .Select(f => 
                {
                    var bounds = f.GetBounds();

                    var c = f.Stroke.Color;

                    var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height)
                    { 
                        Stroke = new Stroke() { Color = DColor.FromArgb(96, c.R, c.G, c.B) }
                    };

                    return rect;
                })
                .ToArray();

            // cubic bezier extremes
            // var pathMarks = svgImageInfo.Paths
            //     .SelectMany(p =>
            //         p.Primitives.OfType<CubicBezier>().Select(cb => 
            //         {
            //             var sp = (Path)p.Clone();
            //             sp.StrokeColor = DColor.FromArgb(255, sp.StrokeColor.Value.B, (byte) (128 - sp.StrokeColor.Value.G), sp.StrokeColor.Value.R);
            // 
            //             var rootPoints = new List<Vector2>();
            // 
            //             //var xroots = CommonMath.GetCubicRoots(cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X);
            //             // rootPoints.AddRange(xroots.Select(r => BezierMath.BezierCoord(r, cb)));
            //             var xroots = CommonMath.SolveQuadratic(cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X);
            // 
            //             //var yroots = CommonMath.GetCubicRoots(cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);
            //             // rootPoints.AddRange(yroots.Select(r => BezierMath.BezierCoord(r, cb)));
            // 
            //             var yroots = CommonMath.SolveQuadratic(cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);
            // 
            //             rootPoints.Add(new Vector2(xroots.solution1 ?? 0f, yroots.solution1 ?? 0f));
            // 
            //             rootPoints.Add(new Vector2(xroots.solution2 ?? 0f, yroots.solution2 ?? 0f));
            // 
            //             var offset = 5 / scale.X;
            // 
            //             var segs = rootPoints.Select(rp => new Segment(new Vector2(rp.X-offset, rp.Y-offset), new Vector2(rp.X + offset, rp.Y + offset))).ToList();
            //             segs.AddRange(rootPoints.Select(rp => new Segment(new Vector2(rp.X+offset, rp.Y-offset), new Vector2(rp.X - offset, rp.Y + offset))));
            //             sp.Primitives = segs.ToArray();
            //             return sp;
            //         })
            //         )
            //     .ToArray(); 

            // intersections of cubic bezier and right going ray
            // var pathMarks = new Func<int, Path[]>(i => svgImageInfo.Paths
            //     .SelectMany(p =>
            //         p.Primitives.OfType<CubicBezier>().Select(cb =>
            //         {
            //             var sp = (Path)p.Clone();
            //             sp.StrokeColor = DColor.FromArgb(255, sp.StrokeColor.Value.B, (byte)(128 - sp.StrokeColor.Value.G), sp.StrokeColor.Value.R);
            // 
            //             var rootPoints = new List<Vector2>();
            // 
            //             var bounds = cb.Bounds;
            // 
            //             //var rayY = bounds.Top + bounds.Height / 2;
            //             var rayY = bounds.Top + (i% bounds.Height);
            //             var ray = new Segment(
            //                 // new Vector2(bounds.Left + bounds.Width/1.2f, rayY),
            //                 // new Vector2(bounds.Left, rayY),
            //                 new Vector2(0, rayY),
            //                 new Vector2(bounds.Right, rayY));
            // 
            //             var cbr = new CubicBezier(
            //                 cb.P1 - ray.P1,
            //                 cb.P2 - ray.P1,
            //                 cb.C1 - ray.P1,
            //                 cb.C2 - ray.P1
            //                 );
            // 
            //             var rootsy = CommonMath.GetCubicRoots(cbr.P1.Y, cbr.C1.Y, cbr.C2.Y, cbr.P2.Y);
            // 
            // 
            //             rootPoints = rootsy //rootsx.Concat(rootsy)
            //                 .Select(r => BezierMath.BezierCoord(r, cbr) + ray.P1)
            //                 .ToList();
            // 
            //             var offset = 5 / scale.X;
            // 
            //             var segs = rootPoints.Select(rp => new Segment(new Vector2(rp.X - offset, rp.Y - offset), new Vector2(rp.X + offset, rp.Y + offset))).ToList();
            //             segs.AddRange(rootPoints.Select(rp => new Segment(new Vector2(rp.X + offset, rp.Y - offset), new Vector2(rp.X - offset, rp.Y + offset))));
            //             segs.Add(ray);
            //             sp.Primitives = segs.ToArray();
            //             return sp;
            //         })
            //         )
            //     .ToArray()
            //     );


            // primitives first and last points
            var figureEndsMarkPaths = figures
                .SelectMany(figure => 
                {
                    if (figure is Path path)
                    {
                        return path.Primitives.SelectMany(p => new[] { p.FirstPoint, p.LastPoint });
                    }
                    else
                    {
                        return new[] { figure.FirstPoint, figure.LastPoint };
                    }
                })
                .Distinct()
                .Select(p =>
                {
                    var crossPath = new Path();

                    crossPath.Stroke = new Stroke() { Color = DColor.FromArgb(192, 255, 128, 0) };

                    var points = new List<Vector2>();

                    var offset = 5 / scale.X;

                    crossPath.Primitives = new[] {
                        new Segment(new Vector2(p.X - offset, p.Y - offset), new Vector2(p.X + offset, p.Y + offset)),
                        new Segment(new Vector2(p.X + offset, p.Y - offset), new Vector2(p.X - offset, p.Y + offset))
                    };

                    return crossPath;
                })
                .ToArray();



            var prevLast = new Vector2();

            var pathJumpSegments = figures
                .Where(p => p.Stroke != null)
                .SelectMany(figure =>
                {
                    if (figure is Path path)
                    {
                        var res = path.Primitives.Select(p =>
                        {
                            var s = new Segment(prevLast, p.FirstPoint);
                            prevLast = p.LastPoint;
                            return s;
                        });
                        return res;
                    }
                    else
                    {
                        var res = new[] { new Segment(prevLast, figure.FirstPoint) };
                        prevLast = figure.LastPoint;
                        return res;
                    }
                })
                .ToArray();

            var pathJumpPaths = new Path() 
            {
                Stroke = new Stroke() { Color = DColor.Red } ,
                Primitives = pathJumpSegments
            };

            #endregion


            #region convert everything to SFML entities

            Color ToSfmlColor(DColor? c) => c.HasValue 
                ? new Color(c.Value.R, c.Value.G, c.Value.B, c.Value.A) 
                : Color.Transparent;

            VertexArray PathToVertexLinesArray(Path path)
            {
                var arr = new VertexArray(PrimitiveType.Lines);
                var color = ToSfmlColor(path.Stroke.Color);
                foreach (var seg in path.Primitives.Select(x => (Segment)x))
                {
                    arr.Append(new Vertex(new Vector2f(seg.P1.X, seg.P1.Y), color));
                    //arr.Append(new Vertex(new Vector2f(seg.P2.X, seg.P2.Y), color));
                    arr.Append(new Vertex(new Vector2f(seg.P2.X, seg.P2.Y), new Color(0xFFFFFF00 ^ color.ToInteger())));
                }
                return arr;
            }

            VertexArray PathToVertexDotArray(Path path)
            {
                var arr = new VertexArray(PrimitiveType.Points);
                var color = ToSfmlColor(path.Stroke.Color);
                foreach (var prim in path.Primitives)
                {
                    if (prim is Dot dot)
                    {
                        arr.Append(new Vertex(new Vector2f(dot.Pos.X, dot.Pos.Y), color));
                    }
                    else if (prim is Segment seg)
                    {
                        arr.Append(new Vertex(new Vector2f(seg.P1.X, seg.P1.Y), color));
                        arr.Append(new Vertex(new Vector2f(seg.P2.X, seg.P2.Y), color));
                    }
                    else
                        throw new ArgumentException();
                }
                return arr;
            }

            VertexArray Vector2ToVertexDotArray(Vector2[] dots, DColor color)
            {
                var arr = new VertexArray(PrimitiveType.Points);
                var sfmlColor = ToSfmlColor(color);

                foreach (var dot in dots)
                {
                    arr.Append(new Vertex(new Vector2f(dot.X, dot.Y), sfmlColor));
                }

                return arr;
            }


            //var vaSegments = pathsSegments.Select(p => PathToVertexLinesArray(p)).ToArray();
            var vaSegments = figuresStrokeDots.Select(p => Vector2ToVertexDotArray(p.Dots, p.Color)).ToArray();
            //var vaSegments = pathsFillSegs.Select(p => PathToVertexLinesArray(p)).ToArray();

            var vaBounds = figuresBounds.Select(p => PathToVertexLinesArray((Path)p.ToPath(true))).ToArray();

            // var vaMarks = new Func<int, VertexArray[]>(i => pathMarks(i).Select(p => PathToVertexLinesArray(p)).ToArray());
            var vaMarks = figureEndsMarkPaths.Select(p => PathToVertexLinesArray(p)).ToArray();

            // var vaFills = pathFills.Select(p => PathToVertexDotArray(p)).ToArray();

            var vaPathJumps = PathToVertexLinesArray(pathJumpPaths);


            // 1 uint square for debugging
            var va1u = new VertexArray(PrimitiveType.Lines);

            var unitSquare = CommonMath.GetRectangleSegments(new RectangleF(0, 0, 1, 1))
                .Each(s =>
                {
                    va1u.Append(new Vertex(new Vector2f(s.p1.X, s.p1.Y), Color.Red));
                    va1u.Append(new Vertex(new Vector2f(s.p2.X, s.p2.Y), Color.Red));
                });

            #endregion convert everything to SFML entities


            #region output

            int fps = 0;
            var fpstime = DateTime.Now;
            var fpstime2 = DateTime.Now;

            int counter = 0;

            var fillsw = new Stopwatch();

            while (window.IsOpen)
            {
                window.DispatchEvents();

                window.Clear(Color.Black);


                var transform = Transform.Identity;
                transform.Scale(scale);
                var renderStates = new RenderStates(transform);


                window.Draw(va1u, renderStates);

                // foreach (var poly in vaMarks(counter++))

                // start and end marks
                foreach (var poly in vaMarks) window.Draw(poly, renderStates);

                // bounding boxes
                foreach (var poly in vaBounds) window.Draw(poly, renderStates);

                // update fill
                fillsw.Start();

                var vaFills = figuresFills
                    .Select(p => Vector2ToVertexDotArray(p.Dots, p.Color))
                    .ToArray();

                foreach (var dots in vaFills) window.Draw(dots, renderStates); // window.Draw() time is negligible

                fillsw.Stop();


                foreach (var poly in vaSegments) window.Draw(poly, renderStates);

                // jump lines between elements
                // window.Draw(vaPathJumps, renderStates);

                var mousepos = Mouse.GetPosition(window);
                var mousecoords = new Vector2(mousepos.X, mousepos.Y) / scale2;

                window.Display();

                vaFills.Each(va => va.Dispose());

                fps++;
                fpstime2 = DateTime.Now;

                if (fpstime2 - fpstime >= TimeSpan.FromSeconds(1))
                {
                    var filltime = (float)fillsw.ElapsedMilliseconds / (float)fps;
                    fillsw.Reset();
                    window.SetTitle($"{filename} | FPS (roughly): {fps} | fill time: {filltime} | mouse {mousecoords}");
                    fps = 0;
                    fpstime = DateTime.Now;
                }

            }

            #endregion


            if (window.IsOpen)
            {
                window.SetActive(false);
                window.Close();
                window.Dispose();
            }


            Console.WriteLine("end");

        }


    }
}
