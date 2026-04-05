using System;
using System.Globalization;

namespace CharlieTest
{
    internal static class OptimisationGraphWriter
    {
        private const int GraphWidth = 64;
        private const int GraphHeight = 20;

        public static void WriteTrendlineGraph(
            Optimiser optimiser,
            Optimiser.QuadraticTrendline trendline,
            int minValue,
            int maxValue,
            (double X, double Y) bestPoint)
        {
            var yMin = double.PositiveInfinity;
            var yMax = double.NegativeInfinity;

            for (var column = 0; column < GraphWidth; column++)
            {
                var x = GetXValueForColumn(column, minValue, maxValue);
                var y = trendline.Evaluate(x);
                UpdateRange(ref yMin, ref yMax, y);
            }

            foreach (var result in optimiser.Results)
                UpdateRange(ref yMin, ref yMax, result.EloGainApproximation);

            UpdateRange(ref yMin, ref yMax, bestPoint.Y);

            if (Math.Abs(yMax - yMin) <= double.Epsilon)
            {
                yMin -= 1d;
                yMax += 1d;
            }
            else
            {
                var padding = (yMax - yMin) * 0.1d;
                yMin -= padding;
                yMax += padding;
            }

            var canvas = new char[GraphHeight][];

            for (var row = 0; row < GraphHeight; row++)
            {
                canvas[row] = new string(' ', GraphWidth).ToCharArray();
                canvas[row][0] = '|';
            }

            for (var column = 0; column < GraphWidth; column++)
                canvas[GraphHeight - 1][column] = '-';

            canvas[GraphHeight - 1][0] = '+';

            int? zeroRow = null;
            if (yMin <= 0d && yMax >= 0d)
            {
                zeroRow = MapToRow(0d, yMin, yMax);

                for (var column = 0; column < GraphWidth; column++)
                    canvas[zeroRow.Value][column] = '=';

                canvas[zeroRow.Value][0] = '+';
            }

            for (var column = 0; column < GraphWidth; column++)
            {
                var x = GetXValueForColumn(column, minValue, maxValue);
                var y = trendline.Evaluate(x);
                var row = MapToRow(y, yMin, yMax);
                canvas[row][column] = '*';
            }

            foreach (var result in optimiser.Results)
            {
                var column = MapToColumn(result.TestValue, minValue, maxValue);
                var row = MapToRow(result.EloGainApproximation, yMin, yMax);
                canvas[row][column] = 'o';
            }

            var bestColumn = MapToColumn(bestPoint.X, minValue, maxValue);
            var bestRow = MapToRow(bestPoint.Y, yMin, yMax);
            canvas[bestRow][bestColumn] = 'X';

            Console.WriteLine("y: expected Elo gain");

            for (var row = 0; row < GraphHeight; row++)
            {
                var label = GetYLabel(row, yMin, yMax, zeroRow);
                Console.WriteLine($"{label,8} {new string(canvas[row])}");
            }

            Console.WriteLine($"         {CreateXTickLine(minValue, maxValue)}");
            Console.WriteLine("         x: test value");
            Console.WriteLine("         * = trendline, o = sampled result, X = best fit peak, = = zero Elo line");
        }

        private static void UpdateRange(ref double minimum, ref double maximum, double value)
        {
            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
        }

        private static double GetXValueForColumn(int column, int minValue, int maxValue)
        {
            if (GraphWidth <= 1 || minValue == maxValue)
                return minValue;

            return minValue + ((maxValue - minValue) * column / (double)(GraphWidth - 1));
        }

        private static int MapToColumn(double x, int minValue, int maxValue)
        {
            if (GraphWidth <= 1 || minValue == maxValue)
                return 0;

            var fraction = (x - minValue) / (maxValue - minValue);
            return Math.Clamp((int)Math.Round(fraction * (GraphWidth - 1)), 0, GraphWidth - 1);
        }

        private static int MapToRow(double y, double yMin, double yMax)
        {
            if (GraphHeight <= 1 || Math.Abs(yMax - yMin) <= double.Epsilon)
                return 0;

            var fraction = (y - yMin) / (yMax - yMin);
            return Math.Clamp((int)Math.Round((1d - fraction) * (GraphHeight - 1)), 0, GraphHeight - 1);
        }

        private static string GetYLabel(int row, double yMin, double yMax, int? zeroRow)
        {
            if (row == 0)
                return yMax.ToString("0.##", CultureInfo.InvariantCulture);

            if (zeroRow.HasValue && row == zeroRow.Value)
                return 0d.ToString("0.##", CultureInfo.InvariantCulture);

            if (row == GraphHeight / 2)
                return ((yMin + yMax) / 2d).ToString("0.##", CultureInfo.InvariantCulture);

            if (row == GraphHeight - 1)
                return yMin.ToString("0.##", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        private static string CreateXTickLine(int minValue, int maxValue)
        {
            var line = new string(' ', GraphWidth).ToCharArray();
            var midpoint = minValue + ((maxValue - minValue) / 2d);
            var maxValueLabel = maxValue.ToString(CultureInfo.InvariantCulture);

            WriteText(line, 0, minValue.ToString(CultureInfo.InvariantCulture));
            WriteText(line, (GraphWidth / 2) - 2, midpoint.ToString("0.##", CultureInfo.InvariantCulture));
            WriteText(line, GraphWidth - maxValueLabel.Length, maxValueLabel);

            return new string(line);
        }

        private static void WriteText(char[] line, int startIndex, string text)
        {
            if (line.Length == 0 || string.IsNullOrEmpty(text))
                return;

            startIndex = Math.Clamp(startIndex, 0, Math.Max(0, line.Length - text.Length));

            for (var i = 0; i < text.Length && (startIndex + i) < line.Length; i++)
                line[startIndex + i] = text[i];
        }
    }
}
