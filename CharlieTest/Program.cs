using CharlieTest.Optimisation;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CharlieTest
{
    class Program
    {
        private const int OptimisationMatchCount = 12;
        private const int OptimisationTimeControlSeconds = 2;

        static async Task Main()
        {
            Console.WriteLine("Are you looking to run a single match or optimise a parameter?");
            Console.WriteLine("1: Optimise parameter");
            Console.WriteLine("Any other key: Run single match");

            var choice = Console.ReadKey();
            Console.WriteLine();

            if (choice.KeyChar != '1')
            {
                var numberOfMatches = 100;
                var timeControlSeconds = 5;
                await RunSingleMatch(numberOfMatches, timeControlSeconds);
            }
            else
            {
                var minValue = 5;
                var maxValue = 40;
                await OptimiseParameter(minValue, maxValue);
            }
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
            var stopwatch = Stopwatch.StartNew();
            var cuteChess = new CuteChessWrapper();
            var optimiser = new Optimiser(minValue, maxValue);

            await RunPreliminaryTrials(cuteChess, optimiser, minValue, maxValue);

            const int numTrials = 200;

            for (var i = 0; i < numTrials; i++)
            {
                var nextValue = optimiser.ChooseNextValue();
                Console.WriteLine($"Running trial: {i + 1}/{numTrials}\tTestValue:{nextValue}");
                await RunOptimisationMatch(cuteChess, optimiser, nextValue);

                if (i % 10 == 0 && i != 0)
                {
                    var timeRemainingSeconds = GetTimeRemainingSeconds(stopwatch.Elapsed.TotalSeconds, numTrials, i);
                    Console.WriteLine($"Estimated time remaining: {timeRemainingSeconds} seconds.");
                }

                Console.WriteLine();

                if (!optimiser.TryGetTrendline(out var trendline))
                {
                    Console.WriteLine("Unable to fit a quadratic trendline to the optimisation results.");
                    return;
                }

                WriteOptimisationSummary(trendline, minValue, maxValue);
                var elapsedSecondsString = stopwatch.Elapsed.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture);
                Console.WriteLine($"Completed in {elapsedSecondsString} seconds.");
            }
        }

        private static async Task RunPreliminaryTrials(CuteChessWrapper cuteChess, Optimiser optimiser, int minValue, int maxValue)
        {
            var midpoint = minValue + ((maxValue - minValue) / 2);

            Console.WriteLine($"Running preliminary trial 1\tTestValue:{minValue}");
            await RunOptimisationMatch(cuteChess, optimiser, minValue);
            Console.WriteLine($"Running preliminary trial 2\tTestValue:{maxValue}");
            await RunOptimisationMatch(cuteChess, optimiser, maxValue);
            Console.WriteLine($"Running preliminary trial 3\tTestValue:{midpoint}");
            await RunOptimisationMatch(cuteChess, optimiser, midpoint);
        }

        private static double GetTimeRemainingSeconds(double timeElapsedSeconds, int numTrials, int trialIndex)
        {
            var trialsPerSecond = (trialIndex + 1) / timeElapsedSeconds;
            var trialsRemaining = numTrials - (trialIndex + 1);
            return trialsRemaining / trialsPerSecond;
        }

        private static async Task RunOptimisationMatch(CuteChessWrapper cuteChess, Optimiser optimiser, int testValue)
        {
            var result = await cuteChess.RunSingleMatch(OptimisationMatchCount, OptimisationTimeControlSeconds, false, testValue);
            optimiser.AddResult(testValue, result.EloDifference);

            var eloDifferenceString = result.EloDifference.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine($"Elo diff={eloDifferenceString}");
        }

        private static void WriteOptimisationSummary(QuadraticTrendline trendline, int minValue, int maxValue)
        {
            var bestPoint = trendline.FindMaximumPoint(minValue, maxValue);
            var recommendedValue = Math.Clamp(
                (int)Math.Round(bestPoint.X, MidpointRounding.AwayFromZero),
                minValue,
                maxValue);

            var bestValueString = bestPoint.X.ToString("0.###", CultureInfo.InvariantCulture);
            var recommendedValueString = recommendedValue.ToString(CultureInfo.InvariantCulture);
            var bestPointEloString = bestPoint.Y.ToString("0.###", CultureInfo.InvariantCulture);

            Console.WriteLine("Best-fit quadratic:");
            Console.WriteLine(trendline.ToString());
            Console.WriteLine($"Best x ~= {bestValueString} (nearest test value {recommendedValueString}), expected y ~= {bestPointEloString}");
            Console.WriteLine();

            OptimisationGraphWriter.WriteTrendlineGraph(trendline, minValue, maxValue, bestPoint);
        }
    }
}
