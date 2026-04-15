using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharlieTest
{
    internal class CuteChessWrapper
    {
        public async Task<TournamentResult> RunSingleMatch(
            int numberOfMatches,
            int timeControlSeconds,
            bool writeAllOutput,
            int? testValue = null)
        {
            var cuteChessLocation = @"C:\Program Files (x86)\cutechess\cutechess-cli.exe";
            var openingsLocation = @"noob_2moves.pgn";
            var pgnOutputLocation = @"tournament.pgn";
            var testValueArgument = testValue.HasValue ? $"option.TestValue={testValue.Value} " : "";
            var standardOutput = new StringBuilder();
            var standardError = new StringBuilder();
            var standardOutputClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var standardErrorClosed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            string arguments = $"-engine conf=\"Charlie dev\" {testValueArgument}" +
                    "-engine conf=\"Charlie test\" " +
                    $"-each tc={timeControlSeconds}+{(double)timeControlSeconds / 100} " +
                    $"-openings file=\"{openingsLocation}\" " +
                    "format=pgn " +
                    "order=random " +
                    "-games 2 " +
                    $"-rounds {numberOfMatches} " +
                    $"-pgnout \"{pgnOutputLocation}\" " +
                    "-recover " +
                    "-concurrency 10 ";

            var startInfo = new ProcessStartInfo
            {
                FileName = cuteChessLocation,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments = arguments
            };

            using var cuteChess = new Process { StartInfo = startInfo };

            cuteChess.OutputDataReceived += (_, e) =>
                HandleCuteChessOutputData(e, standardOutput, standardOutputClosed, writeAllOutput);

            cuteChess.ErrorDataReceived += (_, e) =>
                HandleCuteChessErrorData(e, standardError, standardErrorClosed);

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

        private double ExtractEloDifference(string output)
        {
            var eloGain = output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .LastOrDefault(line => line.StartsWith("Elo difference", StringComparison.OrdinalIgnoreCase))
                .Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ElementAtOrDefault(1)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (string.Equals(eloGain, "inf", StringComparison.OrdinalIgnoreCase))
                return 1000d;

            if (string.Equals(eloGain, "-inf", StringComparison.OrdinalIgnoreCase))
                return -1000d;

            if (double.TryParse(eloGain, CultureInfo.InvariantCulture, out var gain))
                return gain;

            return 1000d;
        }

        private void HandleCuteChessErrorData(
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

        private void HandleCuteChessOutputData(
            DataReceivedEventArgs e,
            StringBuilder standardOutput,
            TaskCompletionSource<bool> standardOutputClosed,
            bool writeAllOutput)
        {
            if (e.Data is null)
            {
                standardOutputClosed.TrySetResult(true);
                return;
            }

            standardOutput.AppendLine(e.Data);
            ReportProgress(e.Data, writeAllOutput);
        }

        private void ReportProgress(string outputLine, bool writeAllOutput)
        {
            if (outputLine.StartsWith("Finished game", StringComparison.OrdinalIgnoreCase))
                return;

            if (writeAllOutput)
                Console.WriteLine(outputLine);
            else
                Console.Write('.');
        }
    }

    public sealed record TournamentResult(
        int? TestValue,
        int ExitCode,
        string StandardOutput,
        string StandardError,
        double EloDifference);
}
