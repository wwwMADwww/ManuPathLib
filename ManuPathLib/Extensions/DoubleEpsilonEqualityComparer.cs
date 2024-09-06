using System;
using System.Collections.Generic;
using System.Text;
using ManuPath.Maths;

namespace ManuPath.Extensions
{
    public class DoubleEpsilonEqualityComparer : IEqualityComparer<double>
    {
        private readonly double _epsilon;

        public DoubleEpsilonEqualityComparer(double epsilon = CommonMath._epsilond)
        {
            _epsilon = epsilon;
        }

        public bool Equals(double x, double y)
        {
            return CommonMath.IsDoubleEquals(x, y, _epsilon);
        }

        public int GetHashCode(double obj)
        {
            return obj.GetHashCode();
        }
    }
}
