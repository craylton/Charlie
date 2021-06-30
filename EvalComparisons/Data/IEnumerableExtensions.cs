using System;
using System.Collections.Generic;

namespace EvalComparisons.Data
{
    public static class IEnumerableExtensions
    {
        public static long SumLong<T>(this IEnumerable<T> list, Func<T, long> selector)
        {
            long sum = 0;

            foreach (var item in list)
            {
                sum += selector(item);
            }

            return sum;
        }

        public static long SumLong(this IEnumerable<int> list) =>
            list.SumLong(item => item);
    }
}
