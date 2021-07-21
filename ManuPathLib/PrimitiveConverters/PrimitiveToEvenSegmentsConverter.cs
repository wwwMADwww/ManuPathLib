using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ManuPath.PrimitiveConverters
{
    public class PrimitiveToEvenSegmentsConverter : PrimitiveToSegmentsConverterBase
    {

        private readonly float _pointDistanceMin;
        private readonly float _pointDistanceMax;
        private readonly float _initialDeltaT;
        private readonly float _initialDeltaTKoeff;
        private readonly float _deltaTKoeff2;

        public PrimitiveToEvenSegmentsConverter(
            float pointDistanceMin, float pointDistanceMax, bool closePath,
            float initialDeltaT = 0.1f, float initialDeltaTKoeff = 2f, float deltaTKoeff2 = 1.2f)
        {
            _pointDistanceMin = pointDistanceMin;
            _pointDistanceMax = pointDistanceMax;
            _closePath = closePath;
            _initialDeltaT = initialDeltaT;
            _initialDeltaTKoeff = initialDeltaTKoeff;
            _deltaTKoeff2 = deltaTKoeff2;
        }




        protected override IEnumerable<Segment> CubicBezierToSegments(CubicBezier cb, Vector2 prevPoint = default)
        {

            var res = new List<Segment>();

            // debug, statistics
            // var repeatEvent = 0;
            // var repeatTotal = 0;
            // var repeatMax = 0;


            // TODO: first and last points

            // res.Add(bc.P1);

            var p1 = cb.P1;

            if (prevPoint != default)
            {
                var pd = CommonMath.Distance(p1, prevPoint);

                // if (_pointDistanceMin <= pd && pd <= _pointDistanceMax)
                if (pd <= _pointDistanceMax)
                    p1 = prevPoint;
            }


            var t = 0f;
            var dt = _initialDeltaT;


            while (true)
            {

                var p2 = default(Vector2);

                var prevDiv = false;
                var prevMul = false;

                var dtkoeff2 = _deltaTKoeff2;
                var dtkoeff = _initialDeltaTKoeff;

                // var repeat = -1;

                // перебор значений t так, чтобы расстояние между точками было не менее r1 и не более r2
                while (true)
                {
                    // repeat++;

                    var t2 = t + dt;

                    if (t2 >= 1.0f)
                        t2 = 1.0f;

                    p2 = new Vector2(
                        BezierMath.BezierCoord(t2, cb.P1.X, cb.C1.X, cb.C2.X, cb.P2.X),
                        BezierMath.BezierCoord(t2, cb.P1.Y, cb.C1.Y, cb.C2.Y, cb.P2.Y));

                    var d = CommonMath.Distance(p1, p2);

                    if (d < _pointDistanceMin)
                    {
                        // если отрезок слишком короткий, но мы уже уперлись в последнюю точку, то пропускаем действие.
                        if (t2 < 1.0f)
                        {
                            if (prevDiv)
                                dtkoeff /= dtkoeff2;

                            dt *= dtkoeff;
                            prevMul = true;
                            prevDiv = false;

                            continue;
                        }
                    }
                    else if (d > _pointDistanceMax)
                    {
                        if (prevMul)
                            dtkoeff *= dtkoeff2;

                        dt /= dtkoeff;
                        prevDiv = true;
                        prevMul = false;

                        continue;
                    }

                    t = t2;
                    // Console.WriteLine($"t {t}, d {d}");

                    dtkoeff = _initialDeltaTKoeff;
                    dtkoeff2 = _deltaTKoeff2;
                    // x4 less repeatTotal and x2 less repeatEvent when disabled
                    // dt = initialDeltaT;

                    prevDiv = prevMul = false;
                    break;

                } // while repeat


                // if (repeat > 0)
                // {
                //     repeatEvent++;
                //     repeatTotal += repeat;
                //     if (repeatMax < repeat)
                //         repeatMax = repeat;
                // }

                if (t >= 1)
                    break;
                // если отрезок слишком короткий, то не добавляем его

                res.Add(new Segment(p1, p2));

                p1 = p2;

            } // while elements


            // res.Add(new Segment(p1, cb.P2));


            return res;
        }



        protected override IEnumerable<Segment> SegmentDivide(Segment segment, Vector2 prevPoint = default)
        {
            var res = new List<Segment>();

            var dx = segment.P2.X - segment.P1.X;
            var dy = segment.P2.Y - segment.P1.Y;

            Vector2 start = segment.P1;

            if (prevPoint != default)
            {
                var prevdistance = CommonMath.Distance(start, prevPoint);

                if (prevdistance < _pointDistanceMin || CommonMath.IsFloatEquals(prevdistance, _pointDistanceMin))
                {
                    var points = CommonMath.LineCircleIntersections(prevPoint, _pointDistanceMin, segment.P1, segment.P2);

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

                res.Add(new Segment(prevPoint, start));
            }

            var seg = new Segment(start, segment.P2);


            var segmentCount = (int) (segment.Length / _pointDistanceMin);

            dx = (seg.P2.X - seg.P1.X) / segmentCount;
            dy = (seg.P2.Y - seg.P1.Y) / segmentCount;


            var p1 = new Vector2(seg.P1.X, seg.P1.Y);

            for (int i = 1; i <= segmentCount; i++)
            {
                var p2 = new Vector2(seg.P1.X + dx * i, seg.P1.Y + dy * i);
                res.Add(new Segment(p1, p2));
                p1 = p2;
            }

            return res.ToArray();
        }



    }
}
