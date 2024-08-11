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
using ManuPath.Extensions;
using RectangleF = System.Drawing.RectangleF;

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
            // var filename = (@"..\..\..\svg\ellipses.svg");
            // var filename = (@"..\..\..\svg\stitches.svg");
            // var filename = (@"..\..\..\svg\stitches2.svg");
            // var filename = (@"..\..\..\svg\stitches3.svg");
            // var filename = (@"..\..\..\svg\stitches4.svg");
            // var filename = (@"..\..\..\svg\shades.svg");
            // var filename = (@"..\..\..\svg\random 2.svg");
            // var filename = (@"..\..\..\svg\transform.svg");
            // var filename = (@"..\..\..\svg\transform 2.svg");
            // var filename = (@"..\..\..\svg\transform 3.svg");
            // var filename = (@"..\..\..\svg\transform 4.svg");
            // var filename = (@"..\..\..\svg\transform 5.svg");
            // var filename = (@"..\..\..\svg\opens.svg");
            // var filename = (@"..\..\..\svg\text.svg");
            // var filename = (@"..\..\..\svg\discontinuity.svg");
            // var filename = (@"..\..\..\svg\discontinuity2.svg");
            // var filename = (@"..\..\..\svg\beziers1.svg");
            // var filename = (@"..\..\..\svg\beziers1 2.svg");
            // var filename = (@"..\..\..\svg\beziers2.svg");
            // var filename = (@"..\..\..\svg\beziers3.svg");

            var filenameonly = System.IO.Path.GetFileName(filename);

            #endregion

            var vectorImage = SvgImageReader.ReadSvgFile(filename);

            #region SFML window stuff

            var resolutionX = 800;
            var resolutionY = 600;

            var viewZoom = new Vector2(1, 1);
            var viewPos = new Vector2(0, 0);

            var scale = new Vector2(1f, 1f);
                                    
            void UpdateScale(Vector2 image, Vector2u boundingBox)
            {
                float widthScale = 0, heightScale = 0;
                if (image.X != 0)
                    widthScale = boundingBox.X / image.X;
                if (image.Y != 0)
                    heightScale = boundingBox.Y / image.Y;

                var scalemin = Math.Min(widthScale, heightScale);

                scale = new Vector2(scalemin, scalemin);
            }

            var window = new RenderWindow(new VideoMode((uint) resolutionX, (uint)resolutionY), "Manual path test app");
            
            window.KeyReleased += (o, e) => 
            {
                if (e.Code == Keyboard.Key.Escape)
                {
                    window.Close();
                }
            };

            window.Closed += (_, __) => window.Close();

            // window.SetVerticalSyncEnabled(true);
            // window.SetFramerateLimit(15);

            UpdateScale(vectorImage.Size, window.Size);

            window.Resized += (o, e) =>
            {
                UpdateScale(vectorImage.Size, new Vector2u((uint)(window.Size.X * viewZoom.X), (uint)(window.Size.Y * viewZoom.X)));
            };

            window.Resized += (o, e) =>
            {
                window.SetView(new View(new Vector2f(e.Width / 2, e.Height / 2), new Vector2f(e.Width, e.Height)));
            };

            window.MouseWheelScrolled += (o, e) =>
            {
                viewZoom += new Vector2(e.Delta / 10f);
                UpdateScale(vectorImage.Size, new Vector2u((uint)(window.Size.X * viewZoom.X), (uint)(window.Size.Y * viewZoom.X)));
            };

            var mouseLeftClicked = false;
            Vector2 mouseLeftClickedPos = Vector2.Zero;

            window.MouseButtonPressed += (o, e) =>
            {
                if (e.Button == Mouse.Button.Left)
                {
                    mouseLeftClickedPos = new Vector2(e.X, e.Y);
                    mouseLeftClicked = true;
                }
            };

            window.MouseButtonReleased += (o, e) =>
            {
                if (e.Button == Mouse.Button.Left)
                {
                    mouseLeftClicked = false;
                }
            };

            window.MouseMoved += (o, e) =>
            {
                if (mouseLeftClicked)
                {
                    viewPos += new Vector2(
                        e.X - mouseLeftClickedPos.X, 
                        e.Y - mouseLeftClickedPos.Y
                        );
                    mouseLeftClickedPos = new Vector2(e.X, e.Y);
                }
            };

            #endregion



            var figures = vectorImage.Figures
                .Select(f => f.Transform())
                .ToArray();

            #region preparing strokes

            #region sorting figures by ends distance

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

            #endregion sorting figures by ends distance

            //figures = sortedFigures.ToArray();

            //var segDistance = Math.Max(6/scale.X, 6/scale.Y);
            var strokeDotsDistance = Math.Max(0.5f/scale.X, 0.5f/scale.Y);
            
            var figuresStrokeDots = figures
                .Where(p => p.Stroke != null)
                .SelectMany(f =>
                {
                    var strokeConverter = new EqualDistanceStrokeDotGenerator(
                        f, 
                        transform: false, 
                        strokeDotsDistance - strokeDotsDistance / 4, 
                        strokeDotsDistance + strokeDotsDistance / 4);
                    var dots = strokeConverter.Generate();
                    return dots;
                })
                .ToArray();

            #endregion

            #region preparing fill polygons


            // just removing paths without defined fill
            var figuresWithFill = figures
                .Where(p => p.Fill != null)
                .ToArray();


            // fill generation strategy

            var fillscale = Math.Max(vectorImage.Size.X, vectorImage.Size.Y) / 100f;

            var figuresFills = figuresWithFill
                .SelectMany(fgiure => new IntervalFillDotGenerator(
                    fgiure,
                    transform: false,
                    // intervalMin: new Vector2(0.2f, 0.2f) * fillscale,
                    // intervalMax: new Vector2(0.05f, 0.05f) * fillscale,
                    // randomRadiusMin: new Vector2(0.4f, 0.4f) * fillscale,
                    // randomRadiusMax: new Vector2(0.1f, 0.1f) * fillscale

                    // intervalMin: new Vector2(0.1f, 0.1f) * fillscale,
                    // intervalMax: new Vector2(0.015f, 0.015f) * fillscale

                    intervalMin: new Vector2(0.5f) * fillscale,
                    intervalMax: new Vector2(0.1f) * fillscale

                    ).Generate())
                ;//.ToArray();

            #endregion


            #region debug stuff

            #region figures bounding boxes

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

             var pathPrimsBounds = figures
                .Where(f => f.Stroke != null)
                .OfType<Path>()
                .SelectMany(path => 
                {
                    var color = DColor.FromArgb(96, path.Stroke.Color);

                    var rects = path.Primitives.Select(p => new Rectangle(p.GetBounds())
                    {
                        Stroke = new Stroke() { Color = color }
                    });

                    return rects;
                })
                .ToArray();

            #endregion figures bounding boxes

            #region beziers extremes

            var beziersExtremes = figures
                .OfType<Path>()
                .Select(path =>
                {
                    var rootPoints = new List<Vector2>();

                    foreach (var b in path.Primitives.Where(p => p is CubicBezier || p is QuadraticBezier))
                    {
                        if (b is CubicBezier cb)
                        {
                            var rootsx = BezierMath.CubicBezierQuadRoots(cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X);
                            //var rootsx = BezierMath.CubicBezierCubicRoots(cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X);

                            var rootsy = BezierMath.CubicBezierQuadRoots(cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);
                            //var rootsy = BezierMath.CubicBezierCubicRoots(cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y);

                            rootPoints.AddRange(rootsy.Select(r => BezierMath.CubicBezierCoords(r, cb.P1, cb.C1, cb.C2, cb.P2)));
                            rootPoints.AddRange(rootsx.Select(r => BezierMath.CubicBezierCoords(r, cb.P1, cb.C1, cb.C2, cb.P2)));
                        }
                        else if (b is QuadraticBezier qb)
                        {
                            var rootx = BezierMath.QuadBezierLinearRoot(qb.P1.X, qb.C.X, qb.P2.X);
                            
                            var rooty = BezierMath.QuadBezierLinearRoot(qb.P1.Y, qb.C.Y, qb.P2.Y);

                            if (rootx.HasValue) rootPoints.Add(BezierMath.QuadBezierCoords(rootx.Value, qb.P1, qb.C, qb.P2));
                            if (rooty.HasValue) rootPoints.Add(BezierMath.QuadBezierCoords(rooty.Value, qb.P1, qb.C, qb.P2));
                        }
                    }

                    var offset = 4 / scale.X;
            
                    var sp = path.CreateEmpty();
                    sp.Stroke ??= new Stroke() { Color = DColor.White };
                    var segs = rootPoints.SelectMany(rp => new[] {
                        new Segment(new Vector2(rp.X - offset, rp.Y - offset), new Vector2(rp.X + offset, rp.Y + offset)),
                        new Segment(new Vector2(rp.X + offset, rp.Y - offset), new Vector2(rp.X - offset, rp.Y + offset))
                        })
                    .ToArray();
                    sp.Primitives = segs.ToArray();
                    return sp;
                })
                .ToArray();

            #endregion beziers extremes

            #region intersections of beziers and right going ray

            var beziersRightRayIntersections = figures
                .OfType<Path>()
                .Select(path =>
                {
                    var rootPoints = new List<Vector2>();

                    foreach (var b in path.Primitives.Where(p => p is CubicBezier || p is QuadraticBezier))
                    {
                        var bounds = b.GetBounds();
                        var raysCount = 100;
                        for (int i = 0; i < raysCount; i++)
                        {
                            if (i == 89 || i == 90)
                            {
                                ;
                            }
                            var rayY = bounds.Top + ((float)bounds.Height / raysCount * i);
                            var ray = new Segment(
                                new Vector2(bounds.Left, rayY),
                                new Vector2(bounds.Right, rayY));

                            if (b is CubicBezier cb)
                            {
                                var cbr = new CubicBezier(
                                    cb.P1 - ray.P1,
                                    cb.C1 - ray.P1,
                                    cb.C2 - ray.P1,
                                    cb.P2 - ray.P1
                                    );

                                var rootsy = BezierMath.CubicBezierCubicRoots(cbr.P1.Y, cbr.C1.Y, cbr.C2.Y, cbr.P2.Y);

                                rootPoints.AddRange(
                                    rootsy.Select(r => BezierMath.CubicBezierCoords(r, cbr.P1, cbr.C1, cbr.C2, cbr.P2) + ray.P1));
                            }
                            else if (b is QuadraticBezier qb)
                            {
                                var qbr = new QuadraticBezier(
                                    qb.P1 - ray.P1,
                                    qb.C - ray.P1,
                                    qb.P2 - ray.P1
                                    );

                                var rooty = BezierMath.QuadBezierLinearRoot(qbr.P1.Y, qbr.C.Y, qbr.P2.Y);

                                if (rooty.HasValue)
                                {
                                    rootPoints.Add(BezierMath.QuadBezierCoords(rooty.Value, qbr.P1, qbr.C, qbr.P2) + ray.P1);
                                }
                            }


                        }
                    }

                    var offset = 3 / scale.X;

                    var segs = rootPoints.SelectMany(rp => new[] {
                        new Segment(new Vector2(rp.X - offset, rp.Y - offset), new Vector2(rp.X + offset, rp.Y + offset)),
                        new Segment(new Vector2(rp.X + offset, rp.Y - offset), new Vector2(rp.X - offset, rp.Y + offset))
                        })
                    .ToArray();

                    var sp = path.CreateEmpty();
                    sp.Stroke ??= new Stroke() { Color = DColor.White };
                    sp.Stroke.Color = DColor.FromArgb(255, sp.Stroke.Color.B, (byte)(128 - sp.Stroke.Color.G), sp.Stroke.Color.R);
                    sp.Primitives = segs.ToArray();

                    return sp;
                })
                .ToArray();

            #endregion intersections of cubic bezier and right going ray

            #region primitives first and last points

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
                .SelectMany(p =>
                {
                    var offset = 4 / scale.X;

                    var crossPathStart = new Path()
                    {
                        Stroke = new Stroke() { Color = DColor.FromArgb(192, 255, 128, 0) },
                        Primitives = new[] {
                            new Segment(new Vector2(p.X - (offset + 1), p.Y - offset), new Vector2(p.X + (offset + 1), p.Y + offset)),
                            new Segment(new Vector2(p.X + (offset + 1), p.Y - offset), new Vector2(p.X - (offset + 1), p.Y + offset))
                        }
                    };

                    var crossPathEnd = new Path()
                    {
                        Stroke = new Stroke() { Color = DColor.FromArgb(192, 0, 255, 128) },
                        Primitives = new[] {
                            new Segment(new Vector2(p.X - offset, p.Y - (offset + 1)), new Vector2(p.X + offset, p.Y + (offset + 1))),
                            new Segment(new Vector2(p.X + offset, p.Y - (offset + 1)), new Vector2(p.X - offset, p.Y + (offset + 1)))
                        }
                    };

                    return new[] { 
                        crossPathStart, 
                        crossPathEnd 
                    };
                })
                .ToArray();



            #endregion primitives first and last points

            #region jumps between paths

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
                Stroke = new Stroke() { Color = DColor.FromArgb(96, DColor.Red) } ,
                Primitives = pathJumpSegments
            };

            #endregion jumps between paths

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
            var vaImageStrokes = figuresStrokeDots.Select(p => Vector2ToVertexDotArray(p.Dots, p.Color)).ToArray();
            //var vaSegments = pathsFillSegs.Select(p => PathToVertexLinesArray(p)).ToArray();

            var vaBounds = Enumerable.Empty<Rectangle>()
                .Concat(figuresBounds)
                .Concat(pathPrimsBounds)
                .Select(p => PathToVertexLinesArray((Path)p.ToPath(true)))
                .ToArray();

            //var vaMarks = new Func<int, VertexArray[]>(i => pathMarks(i).Select(p => PathToVertexLinesArray(p)).ToArray());
            var vaMarks = new Path[] { }
                //.Concat(figureEndsMarkPaths)
                .Concat(beziersExtremes)
                //.Concat(beziersRightRayIntersections)
                .Select(p => PathToVertexLinesArray(p)).ToArray();

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

            var fillStopwatch = new Stopwatch();
            var fillConversionStopwatch = new Stopwatch();

            while (window.IsOpen)
            {
                window.DispatchEvents();

                window.Clear(Color.Black);


                var transform = Transform.Identity;
                transform.Translate(viewPos.X, viewPos.Y);
                transform.Scale(scale.X, scale.Y);
                var renderStates = new RenderStates(transform);


                window.Draw(va1u, renderStates);

                // foreach (var poly in vaMarks(counter++))

                // start and end marks
                foreach (var poly in vaMarks) window.Draw(poly, renderStates);

                // bounding boxes
                foreach (var poly in vaBounds) window.Draw(poly, renderStates);

                // update fill

                fillStopwatch.Start();

                var fills = figuresFills.ToArray();

                fillStopwatch.Stop();


                fillConversionStopwatch.Start();
                var vaImageFills = fills
                    .Select(p => Vector2ToVertexDotArray(p.Dots, p.Color))
                    .ToArray();
                fillConversionStopwatch.Stop();

                foreach (var dots in vaImageFills) window.Draw(dots, renderStates); // window.Draw() time is negligible



                foreach (var poly in vaImageStrokes) window.Draw(poly, renderStates);

                // jump lines between elements
                // window.Draw(vaPathJumps, renderStates);

                var mousepos = Mouse.GetPosition(window);
                var mousecoords = new Vector2(mousepos.X, mousepos.Y) / scale;

                window.Display();

                vaImageFills.Each(va => va.Dispose());

                fps++;
                fpstime2 = DateTime.Now;

                if (fpstime2 - fpstime >= TimeSpan.FromSeconds(1))
                {
                    var fillTime        = (float)fillStopwatch.ElapsedMilliseconds / (float)fps;
                    var fillConvertTime = (float)fillConversionStopwatch.ElapsedMilliseconds / (float)fps;
                    fillStopwatch.Reset();
                    fillConversionStopwatch.Reset();
                    window.SetTitle(
                        $"{filenameonly} | FPS {fps} | " +
                        $"mouse {mousecoords} | "+
                        $"fill time avg: gen {fillTime} conv {fillConvertTime} total {fillTime + fillConvertTime} | " +
                        $"view: pos {viewPos} zoom {viewZoom} | ");
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
