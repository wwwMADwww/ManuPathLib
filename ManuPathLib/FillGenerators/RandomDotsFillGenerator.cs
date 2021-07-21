using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;

namespace ManuPath.FillGenerators
{
    public class RandomDotsFillGenerator : IPrimitiveFillGenerator
    {
        private readonly int _dotCount;
        private readonly bool _dotCountExactly;
        private readonly bool _allowIntersections;
        private readonly Path _poly;



        public RandomDotsFillGenerator(int dotCount, bool dotCountExactly, bool allowIntersections, Path poly)
        {
            // if (poly.Primitives.Any(p => !(p is Segment)))
            //     throw new ArgumentException("path must contain only Segments");

            _dotCount = dotCount;
            _dotCountExactly = dotCountExactly;
            _allowIntersections = allowIntersections;
            _poly = poly;
        }



        public Path GenerateFill()
        {

            var res = new List<Vector2>();

            var random = new Random(DateTime.Now.Millisecond);

            Vector2? p1 = null;

            int c = 0;


            var bounds = _poly.Bounds;
            var segments = _poly.Primitives.OfType<Segment>().ToArray();

            for (int i = 0; i < _dotCount; i++)
            {
                var p = new Vector2();

                do
                {
                    p.X = bounds.X + (float)random.NextDouble() * bounds.Width;
                    p.Y = bounds.Y + (float)random.NextDouble() * bounds.Height;

                    if (PathMath.IsPointInPath(_poly, p))
                    {
                        if (!p1.HasValue)
                            p1 = p;


                        if (_allowIntersections)
                        {
                            c++;
                            res.Add(p);
                            p1 = p;
                            break;
                        }
                        else
                        {
                            if (!PathMath.IsSegmentIntersectsWithPoly(segments, new Segment(p1.Value, p)))
                            {
                                c++;
                                res.Add(p);
                                p1 = p;
                                break;
                            }
                            else
                                continue;
                        }

                    }

                } while (_dotCountExactly);

            }


            return new Path()
            {
                StrokeColor = _poly.FillColor,
                Primitives = res.Select(v => new Dot(v)).ToArray()
            };

        }

    }
}
