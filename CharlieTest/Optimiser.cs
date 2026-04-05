using System;
using System.Collections.Generic;

namespace CharlieTest
{
    readonly record struct OptimiserResult(int TestValue, double EloGainApproximation);

    internal class Optimiser
    {
        private const double NumericalTolerance = 1e-12;

        private readonly List<OptimiserResult> _results = [];
        private readonly int _minValue;
        private readonly int _maxValue;

        public IReadOnlyList<OptimiserResult> Results => _results;

        public Optimiser(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than or equal to maxValue.");

            _minValue = minValue;
            _maxValue = maxValue;
        }

        public void AddResult(int testValue, double eloGainApproximation)
        {
            if (testValue < _minValue || testValue > _maxValue)
                throw new ArgumentOutOfRangeException(nameof(testValue), "testValue must be within the optimiser range.");

            _results.Add(new OptimiserResult(testValue, eloGainApproximation));
        }

        public int ChooseNextValue()
        {
            if (_results.Count < 3)
                throw new InvalidOperationException("At least three results are required before choosing the next value.");

            if (!TryFitQuadratic(out QuadraticTrendline trendline))
                return ChooseBoundaryFromObservedResults();

            var minimumY = trendline.FindMinimum(_minValue, _maxValue);
            var shiftedTrendline = trendline.ShiftDown(minimumY);
            var area = shiftedTrendline.GetAreaUnderCurve(_minValue, _maxValue);

            if (area <= NumericalTolerance)
                return ChooseRandomValue();

            var probabilityDensity = shiftedTrendline.DivideBy(area);
            return SampleFromDensity(probabilityDensity);
        }

        public bool TryGetTrendline(out QuadraticTrendline trendline) => TryFitQuadratic(out trendline);

        private int SampleFromDensity(QuadraticTrendline probabilityDensity)
        {
            var maxDensity = probabilityDensity.FindMaximum(_minValue, _maxValue);

            if (maxDensity <= NumericalTolerance)
                return ChooseRandomValue();

            while (true)
            {
                var candidate = ChooseRandomValue();
                var sampleY = Random.Shared.NextDouble() * maxDensity;
                var densityAtCandidate = Math.Max(0d, probabilityDensity.Evaluate(candidate));

                if (sampleY <= densityAtCandidate)
                    return candidate;
            }
        }

        private int ChooseRandomValue() =>
            (int)Random.Shared.NextInt64(_minValue, (long)_maxValue + 1);

        private bool TryFitQuadratic(out QuadraticTrendline trendline)
        {
            double sumX = 0;
            double sumX2 = 0;
            double sumX3 = 0;
            double sumX4 = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumX2Y = 0;

            foreach (var result in _results)
            {
                var x = result.TestValue;
                var y = result.EloGainApproximation;
                var x2 = x * x;

                sumX += x;
                sumX2 += x2;
                sumX3 += x2 * x;
                sumX4 += x2 * x2;
                sumY += y;
                sumXY += x * y;
                sumX2Y += x2 * y;
            }

            var determinant = Determinant(
                sumX4, sumX3, sumX2,
                sumX3, sumX2, sumX,
                sumX2, sumX, _results.Count);

            if (Math.Abs(determinant) < double.Epsilon)
            {
                trendline = default;
                return false;
            }

            var determinantA = Determinant(
                sumX2Y, sumX3, sumX2,
                sumXY, sumX2, sumX,
                sumY, sumX, _results.Count);

            var determinantB = Determinant(
                sumX4, sumX2Y, sumX2,
                sumX3, sumXY, sumX,
                sumX2, sumY, _results.Count);

            var determinantC = Determinant(
                sumX4, sumX3, sumX2Y,
                sumX3, sumX2, sumXY,
                sumX2, sumX, sumY);

            trendline = new QuadraticTrendline(
                determinantA / determinant,
                determinantB / determinant,
                determinantC / determinant);

            return true;
        }

        private int ChooseBoundaryFromObservedResults()
        {
            var minTotal = 0d;
            var minCount = 0;
            var maxTotal = 0d;
            var maxCount = 0;

            foreach (var result in _results)
            {
                if (result.TestValue == _minValue)
                {
                    minTotal += result.EloGainApproximation;
                    minCount++;
                }

                if (result.TestValue == _maxValue)
                {
                    maxTotal += result.EloGainApproximation;
                    maxCount++;
                }
            }

            if (minCount == 0)
                return _maxValue;

            if (maxCount == 0)
                return _minValue;

            return (minTotal / minCount) >= (maxTotal / maxCount)
                ? _minValue
                : _maxValue;
        }

        private static double Determinant(
            double m11, double m12, double m13,
            double m21, double m22, double m23,
            double m31, double m32, double m33) =>
            (m11 * ((m22 * m33) - (m23 * m32)))
            - (m12 * ((m21 * m33) - (m23 * m31)))
            + (m13 * ((m21 * m32) - (m22 * m31)));

        internal readonly record struct QuadraticTrendline(double A, double B, double C)
        {
            public double Evaluate(double x) => (A * x * x) + (B * x) + C;

            public double Evaluate(int x) => Evaluate((double)x);

            public double FindMinimum(int minValue, int maxValue) =>
                FindMinimumPoint(minValue, maxValue).Y;

            public double FindMaximum(int minValue, int maxValue) =>
                FindMaximumPoint(minValue, maxValue).Y;

            public (double X, double Y) FindMinimumPoint(int minValue, int maxValue) =>
                FindExtremePoint(minValue, maxValue, findMaximum: false);

            public (double X, double Y) FindMaximumPoint(int minValue, int maxValue) =>
                FindExtremePoint(minValue, maxValue, findMaximum: true);

            public QuadraticTrendline ShiftDown(double y) => new(A, B, C - y);

            public QuadraticTrendline DivideBy(double value) => new(A / value, B / value, C / value);

            public double GetAreaUnderCurve(int minValue, int maxValue) =>
                Antiderivative(maxValue) - Antiderivative(minValue);

            private (double X, double Y) FindExtremePoint(int minValue, int maxValue, bool findMaximum)
            {
                var bestX = (double)minValue;
                var bestY = Evaluate(minValue);

                var valueAtMax = Evaluate(maxValue);
                if (IsBetter(valueAtMax, bestY, findMaximum))
                {
                    bestX = maxValue;
                    bestY = valueAtMax;
                }

                if (Math.Abs(A) <= NumericalTolerance)
                    return (bestX, bestY);

                var vertexX = -B / (2 * A);
                if (vertexX < minValue || vertexX > maxValue)
                    return (bestX, bestY);

                var vertexY = Evaluate(vertexX);
                if (IsBetter(vertexY, bestY, findMaximum))
                    return (vertexX, vertexY);

                return (bestX, bestY);
            }

            private static bool IsBetter(double candidate, double current, bool findMaximum) =>
                findMaximum ? candidate > current : candidate < current;

            private double Antiderivative(double x) =>
                ((A / 3d) * x * x * x) + ((B / 2d) * x * x) + (C * x);
        }
    }
}
