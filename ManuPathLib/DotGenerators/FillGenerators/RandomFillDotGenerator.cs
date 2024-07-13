//using System;
//using System.Collections.Generic;
//using System.Numerics;
//using System.Linq;
//using System.Text;
//using ManuPath.Maths;
//using ManuPath.Figures;
//using ManuPath.Figures.PathPrimitives;
//using System.Drawing;

//namespace ManuPath.DotGenerators.FillGenerators
//{
//    public class RandomFillDotGenerator : IDotGenerator
//    {
//        private readonly int _dotCount;
//        private readonly bool _dotCountExactly;
//        private readonly bool _allowIntersections;
//        private readonly IFigure _figure;
//        private readonly RectangleF _bounds;



//        public RandomFillDotGenerator(IFigure figure, int dotCount, bool dotCountExactly, bool allowIntersections)
//        {
//            // if (poly.Primitives.Any(p => !(p is Segment)))
//            //     throw new ArgumentException("path must contain only Segments");

//            _dotCount = dotCount;
//            _dotCountExactly = dotCountExactly;
//            _allowIntersections = allowIntersections;
//            _figure = figure;
//            _bounds = _figure.GetBounds(true);
//        }



//        public GeneratedDots[] Generate()
//        {
//            // this is sooo old

//            throw new NotImplementedException();

//            // var random = new Random(DateTime.Now.Millisecond);
//            // 
//            // Vector2? p1 = null;
//            // 
//            // var segments = _poly.Primitives.OfType<Segment>().ToArray();
//            // 
//            // var res = new List<Vector2>();
//            // 
//            // for (int i = 0; i < _dotCount; i++)
//            // {
//            //     var p = new Vector2();
//            // 
//            //     do
//            //     {
//            //         p.X = _bounds.X + (float)random.NextDouble() * _bounds.Width;
//            //         p.Y = _bounds.Y + (float)random.NextDouble() * _bounds.Height;
//            // 
//            //         if (PathMath.IsPointInPath(_poly, p))
//            //         {
//            //             p1 ??= p;
//            // 
//            //             if (_allowIntersections)
//            //             {
//            //                 res.Add(p);
//            //                 p1 = p;
//            //                 break;
//            //             }
//            //             else
//            //             {
//            //                 if (!PathMath.IsSegmentIntersectsWithPoly(segments, new Segment(p1.Value, p)))
//            //                 {
//            //                     res.Add(p);
//            //                     p1 = p;
//            //                     break;
//            //                 }
//            //                 else
//            //                 {
//            //                     continue;
//            //                 }
//            //             }
//            // 
//            //         }
//            // 
//            //     } while (_dotCountExactly);
//            // 
//            // }
//            // 
//            // 
//            // return new[] { new GeneratedDots()
//            // {
//            //     Color = _figure.Fill.Color,
//            //     Dots = res.ToArray()
//            // }};

//        }

//    }
//}
