using System.Collections.Generic;
using System.Drawing;
using GraphPoint = System.Collections.Generic.KeyValuePair<string, decimal>;

namespace EvalComparisons.Graph
{
    public class GraphEquation
    {
        private readonly LinearEquation equation;

        public GraphEquation(IEnumerable<Point> dataPoints) =>
            equation = new LinearEquation(dataPoints);

        public List<GraphPoint> GetGraphPoints(int[] pointsToEvaluate)
        {
            var points = new List<GraphPoint>();

            foreach (var point in pointsToEvaluate)
            {
                points.Add(new GraphPoint(point.ToString(), equation.EvaluateAt(point)));
            }

            return points;
        }
    }
}
