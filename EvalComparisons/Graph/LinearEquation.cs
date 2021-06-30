using EvalComparisons.Data;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EvalComparisons.Graph
{
    class LinearEquation
    {
        public decimal Gradient { get; }
        public decimal Offset { get; }

        public LinearEquation(IEnumerable<Point> dataPoints)
        {
            int n = dataPoints.Count();

            long sumX = dataPoints.Select(dp => dp.X).SumLong();
            long sumXSquared = dataPoints.Select(dp => dp.X * dp.X).SumLong();
            long sumY = dataPoints.Select(dp => dp.Y).SumLong();
            long sumXY = dataPoints.Select(dp => dp.X * dp.Y).SumLong();

            Gradient = (decimal)(n * sumXY - sumX * sumY) / (n * sumXSquared - sumX * sumX);
            Offset = (decimal)sumY / n - Gradient * sumX / n;
        }

        public decimal EvaluateAt(decimal x) => Gradient * x + Offset;
    }
}
