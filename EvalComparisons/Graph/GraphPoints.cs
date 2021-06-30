using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GraphPoint = System.Collections.Generic.KeyValuePair<string, decimal>;

namespace EvalComparisons.Graph
{
    public class GraphPoints
    {
        public IEnumerable<Point> DataPoints { get; }

        public GraphPoints(IEnumerable<Point> dataPoints) => DataPoints = dataPoints;

        public List<GraphPoint> GetGraphPoints(int[] pointsToEvaluate)
        {
            if (pointsToEvaluate.Length == 0) return new List<GraphPoint>();
            if (DataPoints.Count() == 0) return new List<GraphPoint>();

            var sortedXValues = pointsToEvaluate.OrderBy(point => point).ToList();
            return DefineGraphPoints(DataPoints, sortedXValues);
        }

        private List<GraphPoint> DefineGraphPoints(IEnumerable<Point> dataPoints, List<int> sortedXValues)
        {
            var points = new List<GraphPoint>();

            for (int i = 0; i < sortedXValues.Count() - 1; i++)
            {
                points.Add(GetGraphPoint(dataPoints, sortedXValues[i], sortedXValues[i + 1]));
            }

            return points;
        }

        private GraphPoint GetGraphPoint(IEnumerable<Point> dataPoints, int lowerBound, int upperBound)
        {
            var pointsInSection = dataPoints.Where(point => point.X.IsWithin(lowerBound, upperBound));

            decimal averageY;
            if (pointsInSection.Count() == 0) averageY = 0;
            else averageY = (decimal)pointsInSection.Average(point => point.Y);

            var label = $"{lowerBound} - {upperBound}";
            return new GraphPoint(label, averageY);
        }

        public int[] GetGraphSegments(List<Point> dataPoints, int numberOfSplits)
        {
            if (dataPoints.Count == 0) return new int[0];

            var points = new int[numberOfSplits + 1];

            var firstPoint = dataPoints.Min(point => point.X);
            var lastPoint = dataPoints.Max(point => point.X);

            points[0] = firstPoint;
            points[numberOfSplits] = lastPoint;

            for (int i = 1; i < numberOfSplits; i++)
            {
                points[i] = ((lastPoint - firstPoint) * i / numberOfSplits) + firstPoint;
            }

            return points;
        }
    }
}
