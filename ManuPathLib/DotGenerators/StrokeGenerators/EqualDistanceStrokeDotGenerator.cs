using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ManuPath.Figures;
using ManuPath.Figures.PathPrimitives;
using ManuPath.Maths;

namespace ManuPath.DotGenerators.StrokeGenerators
{
    public class EqualDistanceStrokeDotGenerator : StrokeDotGeneratorBase
    {

        private readonly float _pointDistanceMin;
        private readonly float _pointDistanceMax;
        private readonly float _initialDeltaT;
        private readonly float _initialDeltaTKoeff;
        private readonly float _deltaTKoeff2;

        public EqualDistanceStrokeDotGenerator(
            IFigure figure, 
            bool transform,
            float pointDistanceMin, float pointDistanceMax,
            float initialDeltaT = 0.1f, float initialDeltaTKoeff = 2f, float deltaTKoeff2 = 1.2f)
            : base(figure, transform)
        {
            _pointDistanceMin = pointDistanceMin;
            _pointDistanceMax = pointDistanceMax;
            _initialDeltaT = initialDeltaT;
            _initialDeltaTKoeff = initialDeltaTKoeff;
            _deltaTKoeff2 = deltaTKoeff2;
        }




        protected override Vector2[] CubicBezierToSegments(CubicBezier cb, Vector2? prevPoint = null)
        {
            var dots = CommonMath.CurveToEquidistantDots(
                startT: 0f, 
                endT: 1f, 
                _pointDistanceMin, 
                _pointDistanceMax, 
                _initialDeltaT, 
                prevPoint,
                t => BezierMath.CubicBezierCoords(t, cb.P1, cb.C1, cb.C2, cb.P2),
                _initialDeltaTKoeff, 
                _deltaTKoeff2
                );
            
            return dots;
        }

        protected override Vector2[] QuadraticBezierToSegments(QuadraticBezier qb, Vector2? prevPoint = null)
        {
            var dots = CommonMath.CurveToEquidistantDots(
                startT: 0f,
                endT: 1f,
                _pointDistanceMin,
                _pointDistanceMax,
                _initialDeltaT,
                prevPoint,
                t => BezierMath.QuadBezierCoords(t, qb.P1, qb.C, qb.P2),
                _initialDeltaTKoeff,
                _deltaTKoeff2
                );

            return dots;
        }

        protected override Vector2[] SegmentDivide(Segment segment, Vector2? prevPoint = null)
        {
            var res = new List<Vector2>() ;

            var dx = segment.P2.X - segment.P1.X;
            var dy = segment.P2.Y - segment.P1.Y;

            Vector2 start = segment.P1;

            if (prevPoint.HasValue)
            {
                var prevdistance = CommonMath.Distance(start, prevPoint.Value);

                if (prevdistance < _pointDistanceMin || CommonMath.IsFloatEquals(prevdistance, _pointDistanceMin))
                {
                    var points = CommonMath.LineCircleIntersections(prevPoint.Value, _pointDistanceMin, segment.P1, segment.P2);

                    var ddx1 = float.MaxValue;

                    foreach (var p in points)
                    {
                        // linear xy relation, no need for checking both coodrinates
                        var dx2 = p.X - segment.P1.X;
                        if (Math.Sign(dx) == Math.Sign(dx2))
                        {
                            var ddx2 = Math.Abs(dx - dx2);
                            if (ddx2 < ddx1)
                            {
                                ddx1 = ddx2;
                                start = p;
                            }
                        }
                    }
                }
            }

            res.Add(start);

            var seg = new Segment(start, segment.P2);


            var segmentCount = (int)(segment.Length / _pointDistanceMin);

            dx = (seg.P2.X - seg.P1.X) / segmentCount;
            dy = (seg.P2.Y - seg.P1.Y) / segmentCount;


            var p1 = new Vector2(seg.P1.X, seg.P1.Y);

            for (int i = 1; i <= segmentCount; i++)
            {
                var p2 = new Vector2(seg.P1.X + dx * i, seg.P1.Y + dy * i);
                res.Add(p2);
            }

            return res.ToArray();
        }

        protected override Vector2[] RectangleDivide(Rectangle rect, Vector2? prevPoint = null) => throw new NotImplementedException();

        protected override Vector2[] EllipseDivide(Ellipse ellipse, Vector2? prevPoint = null) => throw new NotImplementedException();

    }
}
