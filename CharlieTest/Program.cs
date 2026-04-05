using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
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
                var numberOfMatches = 100;
                var timeControlSeconds = 5;
                var result = await RunSingleMatch(numberOfMatches, timeControlSeconds);
                Console.WriteLine(result.EloDifference.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await OptimiseParameter();
            }
        }

        private static async Task<TournamentResult> RunSingleMatch(int numberOfMatches, int timeControlSeconds, int? testValue = null)
        {
            var cuteChessLocation = @"C:\Program Files (x86)\cutechess\cutechess-cli.exe";
            var openingsLocation = @"noob_2moves.pgn";
            var pgnOutputLocation = @"tournament.pgn";
            var testValueArgument = testValue.HasValue ? $"option.TestValue=\"{testValue.Value}\"" : "";
            var standardOutput = new StringBuilder();
            var standardError = new StringBuilder();
            var standardOutputClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var standardErrorClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var startInfo = new ProcessStartInfo
            {
                FileName = cuteChessLocation,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments =
                    $"-engine conf=\"Charlie dev\" {testValueArgument} " +
                    "-engine conf=\"Charlie test\" " +
                    $"-each tc={timeControlSeconds}+{(double)timeControlSeconds / 100} " +
                    $"-openings file=\"{openingsLocation}\" " +
                    "format=pgn " +
                    "order=random " +
                    "-games 2 " +
                    $"-rounds {numberOfMatches} " +
                    $"-pgnout \"{pgnOutputLocation}\" " +
                    "-recover " +
                    "-concurrency 10 "
            };

            using var cuteChess = new Process { StartInfo = startInfo };

            cuteChess.OutputDataReceived += (_, e) =>
                HandleCuteChessOutputData(testValue, e, standardOutput, standardOutputClosed);

            cuteChess.ErrorDataReceived += (_, e) =>
                HandleCuteChessErrorData(e, standardError, standardErrorClosed);

            Console.WriteLine($"Starting tournament for TestValue={testValue?.ToString() ?? "default"}");

            cuteChess.Start();
            cuteChess.BeginOutputReadLine();
            cuteChess.BeginErrorReadLine();

            await cuteChess.WaitForExitAsync();
            await Task.WhenAll(standardOutputClosed.Task, standardErrorClosed.Task);

            return new TournamentResult(
                testValue,
                cuteChess.ExitCode,
                standardOutput.ToString(),
                standardError.ToString(),
                ExtractEloDifference(standardOutput.ToString()));
        }

        private static async Task OptimiseParameter()
        {
            var firstResult = await RunSingleMatch(10, 1, 0);
            Console.WriteLine(firstResult.EloDifference.ToString(CultureInfo.InvariantCulture));

            var nextValue = ChooseNextValue(firstResult);
            var secondResult = await RunSingleMatch(10, 1, nextValue);
            Console.WriteLine(secondResult.EloDifference.ToString(CultureInfo.InvariantCulture));
        }

        private static int ChooseNextValue(TournamentResult previousResult) =>
            previousResult.TestValue.GetValueOrDefault() + 50;

        private static double ExtractEloDifference(string output)
        {
            var eloGain = output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .LastOrDefault(line => line.StartsWith("Elo difference", StringComparison.OrdinalIgnoreCase))
                .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .LastOrDefault()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            return double.Parse(eloGain, CultureInfo.InvariantCulture);
        }

        private static void HandleCuteChessErrorData(
            DataReceivedEventArgs e,
            StringBuilder standardError,
            TaskCompletionSource<bool> standardErrorClosed)
        {
            if (e.Data is null)
            {
                standardErrorClosed.TrySetResult(true);
                return;
            }

            standardError.AppendLine(e.Data);
        }

        private static void HandleCuteChessOutputData(
            int? testValue,
            DataReceivedEventArgs e,
            StringBuilder standardOutput,
            TaskCompletionSource<bool> standardOutputClosed)
        {
            if (e.Data is null)
            {
                standardOutputClosed.TrySetResult(true);
                return;
            }

            standardOutput.AppendLine(e.Data);
            ReportProgress(e.Data, testValue);
        }

        private static void ReportProgress(string outputLine, int? testValue)
        {
            if (outputLine.StartsWith("Finished game", StringComparison.OrdinalIgnoreCase))
                return;

            Console.WriteLine($"[TestValue={testValue?.ToString() ?? "default"}] {outputLine}");
        }

        private sealed record TournamentResult(
            int? TestValue,
            int ExitCode,
            string StandardOutput,
            string StandardError,
            double EloDifference);
    }
}
