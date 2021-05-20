using System;
using System.Diagnostics;

namespace CharlieTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var cuteChessLocation = @"C:\Program Files (x86)\cutechess\cutechess-cli.exe";
            var openingsLocation = @"F:\Documents\Simon\chess\Engines\Charlie\openings.pgn";
            var pgnOutputLocation = @"F:\Documents\Simon\chess\Engines\Charlie\tournament.pgn";
            var numberOfMatches = 50;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = cuteChessLocation,
                Arguments =
                    "-engine conf=\"Charlie dev\" " +
                    "-engine conf=\"Charlie test\" " +
                    "-each tc=10+0.1 " +
                    $"-openings file=\"{openingsLocation}\" " +
                    "format=pgn " +
                    "order=random " +
                    "-games 2 " +
                    $"-rounds {numberOfMatches} " +
                    $"-pgnout \"{pgnOutputLocation}\" " +
                    "-recover " +
                    "-concurrency 8"
            };

            Process cuteChess = new Process { StartInfo = startInfo };

            cuteChess.Start();
        }
    }
}
