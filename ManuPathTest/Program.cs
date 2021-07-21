using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using DColor = System.Drawing.Color;
using ManuPath;
using ManuPath.PrimitiveConverters;
using ManuPath.FillGenerators;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
            var svgImageInfo = SvgPrimitiveConverter.ReadSvg(filename);

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

            UpdateScale(svgImageInfo.Size, window.Size);

            window.Resized += (o, e) => UpdateScale(svgImageInfo.Size, new Vector2u(e.Width, e.Height));

            window.Resized += (o, e) =>
                window.SetView(new View(new Vector2f(e.Width / 2, e.Height / 2), new Vector2f(e.Width, e.Height)));

            #endregion


            #region preparing strokes

            // var strokeConverter = new PrimitiveToNSegmentsConverter(10);

            var segDistance = Math.Max(6/scale.X, 6/scale.Y);
            var strokeConverter = new PrimitiveToEvenSegmentsConverter(segDistance - segDistance/10, segDistance + segDistance / 10, false);
            

            var pathsSegments = svgImageInfo.Paths.Where(p => p.StrokeColor.HasValue)
                .Select(p =>
                {
                    var sp = (Path)p.Clone();
                    sp.Primitives = strokeConverter.Convert(p.Primitives);
                    return sp;
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
            var pathsFillSegs = svgImageInfo.Paths.Where(p => p.FillColor.HasValue)
                .Where(p => p.FillColor.HasValue)
                .ToArray();


            // fill generation strategy

            var fillscale = scale2;

            var pathFills = pathsFillSegs
                //.Select(p => new RandomDotsFillGenerator(1000, true, true, p).GenerateFill())
                .Select(p => new IntervalDotsFillGenerator(p,
                    new Vector2(0.1f, 0.1f) * fillscale,
                    new Vector2(0.1f, 0.1f) * fillscale,
                    new Vector2(0.3f, 0.3f) * fillscale
                    ).GenerateFill())
                ;//.ToArray();

            #endregion


            #region debug stuff

            // bounding boxes
            // var pathBounds = pathsSegments
            var pathBounds = svgImageInfo.Paths
                .SelectMany(p => p.Primitives.Select(pr => 
                {
                    var bounds = pr.Bounds;
            
                    var sp = (Path)p.Clone();
            
                    sp.Primitives = new Segment[]
                    {
                        new Segment(new Vector2(bounds.Left , bounds.Top   ), new Vector2(bounds.Right, bounds.Top   )),
                        new Segment(new Vector2(bounds.Right, bounds.Top   ), new Vector2(bounds.Right, bounds.Bottom)),
                        new Segment(new Vector2(bounds.Right, bounds.Bottom), new Vector2(bounds.Left , bounds.Bottom)),
                        new Segment(new Vector2(bounds.Left , bounds.Bottom), new Vector2(bounds.Left , bounds.Top   )),
                    };
            
                    return sp;
                }))
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
            var pathMarks = svgImageInfo.Paths
                .SelectMany(path =>
                    path.Primitives.Select(p => 
                    {
                        var sp = (Path)path.Clone();
                        var color = sp.StrokeColor ?? DColor.Black;
                        sp.StrokeColor = DColor.FromArgb(65, color.B, (byte) (128 - color.G), color.R);
            
                        var points = new List<Vector2>();

                        points.Add(p.FirstPoint);
                        points.Add(p.LastPoint);

                        var offset = 5 / scale.X;
            
                        var segs = points.Select(rp => new Segment(new Vector2(rp.X-offset, rp.Y-offset), new Vector2(rp.X + offset, rp.Y + offset))).ToList();
                        segs.AddRange(points.Select(rp => new Segment(new Vector2(rp.X+offset, rp.Y-offset), new Vector2(rp.X - offset, rp.Y + offset))));
                        sp.Primitives = segs.ToArray();
                        return sp;
                    })
                    )
                .ToArray();

            #endregion


            #region convert everything to SFML entities

            Color ToSfmlColor(DColor? c) => c.HasValue 
                ? new Color(c.Value.R, c.Value.G, c.Value.B, c.Value.A) 
                : Color.Transparent;

            VertexArray PathToVertexLinesArray(Path path)
            {
                var arr = new VertexArray(PrimitiveType.Lines);
                var color = ToSfmlColor(path.StrokeColor);
                foreach (var seg in path.Primitives.Select(x => (Segment)x))
                {
                    arr.Append(new Vertex(new Vector2f(seg.P1.X, seg.P1.Y), color));
                    arr.Append(new Vertex(new Vector2f(seg.P2.X, seg.P2.Y), color));
                    //arr.Append(new Vertex(new Vector2f(seg.P2.X, seg.P2.Y), new Color(0xFFFFFF00 ^ color.ToInteger())));
                }
                return arr;
            }

            VertexArray PathToVertexDotArray(Path path)
            {
                var arr = new VertexArray(PrimitiveType.Points);
                var color = ToSfmlColor(path.StrokeColor);
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


            var vaSegments = pathsSegments.Select(p => PathToVertexLinesArray(p)).ToArray();
            //var vaSegments = pathsSegments.Select(p => PathToVertexDotArray(p)).ToArray();
            //var vaSegments = pathsFillSegs.Select(p => PathToVertexLinesArray(p)).ToArray();

            var vaBounds = pathBounds.Select(p => PathToVertexLinesArray(p)).ToArray();

            // var vaMarks = new Func<int, VertexArray[]>(i => pathMarks(i).Select(p => PathToVertexLinesArray(p)).ToArray());
            var vaMarks = pathMarks.Select(p => PathToVertexLinesArray(p)).ToArray();

            // var vaFills = pathFills.Select(p => PathToVertexDotArray(p)).ToArray();


            // 1 uint square for debugging
            var va1u = new VertexArray(PrimitiveType.Lines);
            va1u.Append(new Vertex(new Vector2f(0, 0), Color.Red));
            va1u.Append(new Vertex(new Vector2f(1, 0), Color.Red));

            va1u.Append(new Vertex(new Vector2f(1, 0), Color.Red));
            va1u.Append(new Vertex(new Vector2f(1, 1), Color.Red));

            va1u.Append(new Vertex(new Vector2f(1, 1), Color.Red));
            va1u.Append(new Vertex(new Vector2f(0, 1), Color.Red));

            va1u.Append(new Vertex(new Vector2f(0, 1), Color.Red));
            va1u.Append(new Vertex(new Vector2f(0, 0), Color.Red));

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


                fillsw.Start();
                var vaFills = pathFills.Select(p => PathToVertexDotArray(p)).ToArray();
                fillsw.Stop();

                // foreach (var poly in vaMarks(counter++))
                //foreach (var poly in vaMarks)
                //    window.Draw(poly, renderStates);

                // foreach (var poly in vaBounds)
                //     window.Draw(poly, renderStates);


                foreach (var dots in vaFills)
                    window.Draw(dots, renderStates);

                foreach (var poly in vaSegments)
                    window.Draw(poly, renderStates);



                var mousepos = Mouse.GetPosition(window);
                var mousecoords = new Vector2(mousepos.X, mousepos.Y) / scale2;


                window.Display();

                vaFills.ToList().ForEach(va => va.Dispose());

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
