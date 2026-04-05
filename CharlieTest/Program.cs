using CharlieTest.Optimisation;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CharlieTest
{
    class Program
    {
        static async Task Main()
        {

            Console.WriteLine("Are you looking to run a single match or optimise a parameter?");
            Console.WriteLine("1: Optimise parameter");
            Console.WriteLine("Any other key: Run single match");

            var choice = Console.ReadKey();
            Console.WriteLine();
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (choice.KeyChar != '1')
            {
                var numberOfMatches = 100;
                var timeControlSeconds = 5;
                await RunSingleMatch(numberOfMatches, timeControlSeconds);
            }
            else
            {
                var minValue = 10;
                var maxValue = 80;
                await OptimiseParameter(minValue, maxValue);
            }

            string elapsedSecondsString = stopwatch.Elapsed.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
            Console.WriteLine($"Completed in {elapsedSecondsString} seconds.");
        }

        private static async Task RunSingleMatch(int numberOfMatches, int timeControlSeconds)
        {
            var cuteChess = new CuteChessWrapper();
            var result = await cuteChess.RunSingleMatch(numberOfMatches, timeControlSeconds, true);
            Console.WriteLine();
            Console.WriteLine(result.EloDifference.ToString(CultureInfo.InvariantCulture));
        }

        private static async Task OptimiseParameter(int minValue, int maxValue)
        {
            var cuteChess = new CuteChessWrapper();
            var optimiser = new Optimiser(minValue, maxValue);

            var midpoint = minValue + ((maxValue - minValue) / 2);

            await RunOptimisationMatch(cuteChess, optimiser, minValue);
            await RunOptimisationMatch(cuteChess, optimiser, maxValue);
            await RunOptimisationMatch(cuteChess, optimiser, midpoint);

            int numTrials = 1;

            for (var i = 0; i < numTrials; i++)
            {
                Console.WriteLine($"Running trial {i}/{numTrials}");
                var nextValue = optimiser.ChooseNextValue();
                await RunOptimisationMatch(cuteChess, optimiser, nextValue);
            }

            Console.WriteLine();

            if (!optimiser.TryGetTrendline(out var trendline))
            {
                Console.WriteLine("Unable to fit a quadratic trendline to the optimisation results.");
                return;
            }

            WriteOptimisationSummary(trendline, minValue, maxValue);
        }

        private static async Task RunOptimisationMatch(CuteChessWrapper cuteChess, Optimiser optimiser, int testValue)
        {
            var result = await cuteChess.RunSingleMatch(12, 2, false, testValue);
            optimiser.AddResult(testValue, result.EloDifference);

            string eloDifferenceString = result.EloDifference.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine();
            Console.WriteLine($"TestValue={testValue}\tResult={eloDifferenceString}");
        }

        private static void WriteOptimisationSummary(QuadraticTrendline trendline, int minValue, int maxValue)
        {
            var bestPoint = trendline.FindMaximumPoint(minValue, maxValue);
            var recommendedValue = Math.Clamp(
                (int)Math.Round(bestPoint.X, MidpointRounding.AwayFromZero),
                minValue,
                maxValue);

            string bestValueString = bestPoint.X.ToString("0.###", CultureInfo.InvariantCulture);
            string recommendedValueString = recommendedValue.ToString(CultureInfo.InvariantCulture);
            string bestPointEloString = bestPoint.Y.ToString("0.###", CultureInfo.InvariantCulture);

            Console.WriteLine("Best-fit quadratic:");
            Console.WriteLine(trendline.ToString());
            Console.WriteLine($"Best x ~= {bestValueString} (nearest test value {recommendedValueString}), expected y ~= {bestPointEloString}");
            Console.WriteLine();

            OptimisationGraphWriter.WriteTrendlineGraph(trendline, minValue, maxValue, bestPoint);
        }
    }
}
