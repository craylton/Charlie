using System;
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
            if (choice.KeyChar != '1')
            {
                var numberOfMatches = 10;
                var timeControlSeconds = 5;
                await RunSingleMatch(numberOfMatches, timeControlSeconds);
            }
            else
            {
                var minValue = 10;
                var maxValue = 90;
                await OptimiseParameter(minValue, maxValue);
            }
        }

        private static async Task RunSingleMatch(int numberOfMatches, int timeControlSeconds)
        {
            var cuteChess = new CuteChessWrapper();
            var result = await cuteChess.RunSingleMatch(numberOfMatches, timeControlSeconds);
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

            for (var i = 0; i < 10; i++)
            {
                var nextValue = optimiser.ChooseNextValue();
                Console.WriteLine($"Next TestValue={nextValue}");
                await RunOptimisationMatch(cuteChess, optimiser, nextValue);
            }

            var bestValue = optimiser.ChooseNextValue();
            Console.WriteLine(bestValue.ToString(CultureInfo.InvariantCulture));
        }

        private static async Task RunOptimisationMatch(CuteChessWrapper cuteChess, Optimiser optimiser, int testValue)
        {
            var result = await cuteChess.RunSingleMatch(10, 2, testValue);
            optimiser.AddResult(testValue, result.EloDifference);
            Console.WriteLine(result.EloDifference.ToString(CultureInfo.InvariantCulture));
        }
    }
}
