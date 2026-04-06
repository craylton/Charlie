using System;
using System.Globalization;

namespace CharlieTest.Optimisation;

internal readonly record struct QuadraticTrendline(double A, double B, double C)
{
    private const double NumericalTolerance = 1e-12;

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

    public override string ToString() => 
        $"y = {A.ToString("0.000000", CultureInfo.InvariantCulture)}x^2 {FormatSignedTerm(B, "x")} {FormatSignedTerm(C, string.Empty)}";

    private static string FormatSignedTerm(double coefficient, string suffix)
    {
        var magnitude = Math.Abs(coefficient).ToString("0.000000", CultureInfo.InvariantCulture);
        return coefficient < 0d
            ? $"- {magnitude}{suffix}"
            : $"+ {magnitude}{suffix}";
    }
}