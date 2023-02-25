using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public static class DoubleExtend
    {
        public static bool CloseEnough(this double x, double y, double precision)
            => (Math.Abs(y - x) <= precision);

        public static double Median(this IEnumerable<double> numbers)
        {
            var _numCount = numbers.Count();
            var _halfIndex = _numCount / 2;
            var _sorted = numbers.OrderBy(n => n);
            return (_numCount % 2) == 1
                ? _sorted.ElementAt(_halfIndex)
                : _sorted.ElementAt(_halfIndex - 1);
        }

        /// <summary>Inclusive between</summary>
        public static bool Between(this double val, double src, double trg)
            => ((src <= val) && (val <= trg)) || ((trg <= val) && (val <= src));
    }
}
