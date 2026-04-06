using System;
using System.Globalization;

namespace CharlieTest.Optimisation
{
    internal static class OptimisationGraphWriter
    {
        private static int GraphWidth = 64;
        private static int GraphHeight = 20;
        private const int AxisLabelWidth = 8;
        private const int AxisColumn = 0;
        private const double VerticalPaddingRatio = 0.1d;
        private const char EmptyCharacter = ' ';
        private const char YAxisCharacter = '|';
        private const char XAxisCharacter = '=';
        private const char ZeroAxisCharacter = '-';
        private const char AxisIntersectionCharacter = '+';
        private const char TrendlineCharacter = '*';
        private const char BestPointCharacter = 'O';

        public static void WriteTrendlineGraph(
            QuadraticTrendline trendline,
            int minValue,
            int maxValue,
            (double X, double Y) bestPoint,
            bool isMiniGraph = false)
        {
            if (isMiniGraph)
            {
                GraphWidth = 32;
                GraphHeight = 8;
            }
            var (minimumY, maximumY) = GetGraphRange(trendline, minValue, maxValue, bestPoint.Y);
            var canvas = CreateCanvas();
            var zeroRow = TryGetZeroRow(minimumY, maximumY);

            DrawXAxis(canvas);

            if (zeroRow.HasValue)
                DrawHorizontalLine(canvas, zeroRow.Value, ZeroAxisCharacter);

            PlotTrendline(canvas, trendline, minValue, maxValue, minimumY, maximumY);
            PlotBestPoint(canvas, bestPoint, minValue, maxValue, minimumY, maximumY);
            WriteGraph(canvas, minValue, maxValue, minimumY, maximumY, zeroRow, isMiniGraph);

            GraphWidth = 64;
            GraphHeight = 20;
        }

        private static (double Min, double Max) GetGraphRange(
            QuadraticTrendline trendline,
            int minValue,
            int maxValue,
            double bestY)
        {
            var yMin = double.PositiveInfinity;
            var yMax = double.NegativeInfinity;

            for (var column = 0; column < GraphWidth; column++)
            {
                var x = GetXValueForColumn(column, minValue, maxValue);
                var y = trendline.Evaluate(x);
                UpdateRange(ref yMin, ref yMax, y);
            }

            UpdateRange(ref yMin, ref yMax, bestY);
            AddVerticalPadding(ref yMin, ref yMax);

            return (yMin, yMax);
        }

        private static void AddVerticalPadding(ref double yMin, ref double yMax)
        {
            if (Math.Abs(yMax - yMin) <= double.Epsilon)
            {
                yMin -= 1d;
                yMax += 1d;
                return;
            }

            var padding = (yMax - yMin) * VerticalPaddingRatio;
            yMin -= padding;
            yMax += padding;
        }

        private static char[][] CreateCanvas()
        {
            var canvas = new char[GraphHeight][];

            for (var row = 0; row < GraphHeight; row++)
            {
                canvas[row] = new string(EmptyCharacter, GraphWidth).ToCharArray();
                canvas[row][AxisColumn] = YAxisCharacter;
            }

            return canvas;
        }

        private static int? TryGetZeroRow(double minimumY, double maximumY)
        {
            if (minimumY > 0d || maximumY < 0d)
                return null;

            return MapToRow(0d, minimumY, maximumY);
        }

        private static void DrawXAxis(char[][] canvas)
        {
            DrawHorizontalLine(canvas, GraphHeight - 1, XAxisCharacter);
        }

        private static void DrawHorizontalLine(char[][] canvas, int row, char lineCharacter)
        {
            for (var column = 0; column < GraphWidth; column++)
                canvas[row][column] = lineCharacter;

            canvas[row][AxisColumn] = AxisIntersectionCharacter;
        }

        private static void PlotTrendline(
            char[][] canvas,
            QuadraticTrendline trendline,
            int minValue,
            int maxValue,
            double minimumY,
            double maximumY)
        {
            for (var column = 0; column < GraphWidth; column++)
            {
                var x = GetXValueForColumn(column, minValue, maxValue);
                var y = trendline.Evaluate(x);
                var row = MapToRow(y, minimumY, maximumY);
                canvas[row][column] = TrendlineCharacter;
            }
        }

        private static void PlotBestPoint(
            char[][] canvas,
            (double X, double Y) bestPoint,
            int minValue,
            int maxValue,
            double minimumY,
            double maximumY)
        {
            var column = MapToColumn(bestPoint.X, minValue, maxValue);
            var row = MapToRow(bestPoint.Y, minimumY, maximumY);
            canvas[row][column] = BestPointCharacter;
        }

        private static void WriteGraph(
            char[][] canvas,
            int minValue,
            int maxValue,
            double minimumY,
            double maximumY,
            int? zeroRow,
            bool isMiniGraph)
        {
            if (!isMiniGraph)
                Console.WriteLine("y: expected Elo gain");

            for (var row = 0; row < GraphHeight; row++)
            {
                var label = GetYLabel(row, minimumY, maximumY, zeroRow);
                Console.WriteLine($"{label,AxisLabelWidth} {new string(canvas[row])}");
            }

            Console.WriteLine($"{string.Empty,AxisLabelWidth} {CreateXTickLine(minValue, maxValue)}");

            if (!isMiniGraph)
                Console.WriteLine($"{string.Empty,AxisLabelWidth} x: test value");
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
                return FormatNumber(yMax);

            if (zeroRow.HasValue && row == zeroRow.Value)
                return FormatNumber(0d);

            if (row == GraphHeight / 2)
                return FormatNumber((yMin + yMax) / 2d);

            if (row == GraphHeight - 1)
                return FormatNumber(yMin);

            return string.Empty;
        }

        private static string CreateXTickLine(int minValue, int maxValue)
        {
            var line = new string(EmptyCharacter, GraphWidth).ToCharArray();
            var midpoint = minValue + ((maxValue - minValue) / 2d);
            var maxValueLabel = FormatNumber(maxValue);

            WriteText(line, 0, FormatNumber(minValue));
            WriteText(line, (GraphWidth / 2) - 2, FormatNumber(midpoint));
            WriteText(line, GraphWidth - maxValueLabel.Length, maxValueLabel);

            return new string(line);
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
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
