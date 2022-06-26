using System.Diagnostics;

namespace CharlieTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var cuteChessLocation = @"C:\Program Files (x86)\cutechess\cutechess-cli.exe";
            var openingsLocation = @"noob_2moves.pgn";
            var pgnOutputLocation = @"tournament.pgn";
            var numberOfMatches = 10;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = cuteChessLocation,
                Arguments =
                    "-engine conf=\"Charlie dev\" " +
                    "-engine conf=\"Charlie test\" " +
                    "-each tc=5+0.05 " +
                    $"-openings file=\"{openingsLocation}\" " +
                    "format=pgn " +
                    "order=random " +
                    "-games 2 " +
                    $"-rounds {numberOfMatches} " +
                    $"-pgnout \"{pgnOutputLocation}\" " +
                    "-recover " +
                    "-concurrency 10"
            };

            Process cuteChess = new Process { StartInfo = startInfo };

            cuteChess.Start();
        }
    }
}
