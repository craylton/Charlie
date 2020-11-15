using Charlie.BenchTest;
using Charlie.BoardRepresentation;
using Charlie.Moves;
using Charlie.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charlie
{
    public class Uci
    {
        private readonly Searcher searcher = new Searcher();
        private readonly Bench bench = new Bench();
        private Move lastBestMove;

        public void Initialise()
        {
            searcher.IterationCompleted += Searcher_IterationCompleted;
            searcher.IterationFailedHigh += Searcher_IterationFailedHigh;
            searcher.IterationFailedLow += Searcher_IterationFailedLow;
            searcher.SearchComplete += Searcher_SearchComplete;
            searcher.PerftComplete += Searcher_PerftComplete;
            bench.BenchComplete += Bench_BenchComplete;
        }

        public async Task Loop()
        {
            var boardState = new BoardState();

            while (true)
            {
                string input = Console.ReadLine();
                string[] @params = input.Split(' ');

                switch (input)
                {
                    case "uci":
                        Console.WriteLine("id name Charlie");
                        Console.WriteLine("id author Craylton");
                        Console.WriteLine("uciok");
                        break;
                    case "isready":
                        Console.WriteLine("readyok");
                        break;
                    case "stop":
                        searcher.Stop();
                        break;
                    case "quit":
                        return;
                }

                switch (@params[0])
                {
                    case "position":
                        boardState = Position(@params[1..]);
                        break;
                    case "go":
                        Go(@params[1..], boardState);
                        break;
                    case "setoption":
                        SetOption(@params[1..]);
                        break;
                    case "bench":
                        await Bench(@params[1..]);
                        break;
                    case "perft":
                        await Perft(@params[1..], boardState);
                        break;
                }
            }
        }

        private BoardState Position(string[] @params)
        {
            BoardState boardState;
            int movesIndicatorIndex;

            if (@params.Length > 0 && @params[0] == "startpos")
            {
                boardState = new BoardState();
                movesIndicatorIndex = 1;
            }
            else if (@params.Length >= 7 && @params[0] == "fen")
            {
                boardState = new BoardState(@params[1..7]);
                movesIndicatorIndex = 7;
            }
            else
            {
                throw new Exception("Invalid position specified");
            }

            if (@params.Length > movesIndicatorIndex && @params[movesIndicatorIndex] == "moves")
            {
                var generator = new MoveGenerator();

                foreach (string moveInput in @params[(movesIndicatorIndex + 1)..])
                {
                    IEnumerable<Move> moves = generator.GenerateLegalMoves(boardState);
                    Move move = Move.FromString(moves, moveInput);
                    boardState = boardState.MakeMove(move);
                }
            }

            return boardState;
        }

        private void Go(string[] @params, BoardState boardState)
        {
            SearchTime searchTime = default;
            int targetDepth = default;
            var searchType = SearchType.Infinite;

            if (@params.Length >= 2 && @params[0] == "depth")
            {
                searchType = SearchType.Depth;
                targetDepth = int.Parse(@params[1]);
            }
            else if (@params.Length >= 4 && @params[0] == "wtime" && @params[2] == "btime")
            {
                searchType = SearchType.Time;
                int ourIncrement = 0;
                int whiteTime = int.Parse(@params[1]);
                int blackTime = int.Parse(@params[3]);

                int timeAvailable = boardState.ToMove == PieceColour.White ? whiteTime : blackTime;

                if (@params.Length >= 8 && @params[4] == "winc" && @params[6] == "binc")
                {
                    searchType = SearchType.Time;
                    int whiteIncrement = int.Parse(@params[5]);
                    int blackIncrement = int.Parse(@params[7]);

                    ourIncrement = boardState.ToMove == PieceColour.White ? whiteIncrement : blackIncrement;
                }

                searchTime = new SearchTime(timeAvailable, ourIncrement);
            }

            var searchParameters = new SearchParameters(searchType, searchTime, targetDepth);
            _ = Task.Run(async () => await searcher.Start(boardState, searchParameters));
        }

        private void SetOption(string[] @params)
        {
            if (@params.Length >= 2 && @params[0] == "name")
            {
                var optionName = string.Join(' ', @params[1..]);

                if (optionName == "Clear Hash")
                    searcher.ClearHash();
            }
        }

        private async Task Bench(string[] @params)
        {
            int targetDepth = 4;

            if (@params.Length >= 2 && @params[0] == "depth")
                targetDepth = int.Parse(@params[1]);

            await bench.BenchTest(searcher, targetDepth);
        }

        private async Task Perft(string[] @params, BoardState boardState)
        {
            int targetDepth = 5;

            if (@params.Length >= 1)
                targetDepth = int.Parse(@params[0]);

            await bench.PerfTest(searcher, boardState, targetDepth);
        }

        private void Searcher_SearchComplete(object sender, SearchResults results)
        {
            Console.WriteLine("bestmove " + results.BestMove.ToString());
        }

        private void Searcher_IterationCompleted(object sender, MoveInfo moveInfo)
        {
            var sb = new StringBuilder("info");
            sb.Append(" depth " + moveInfo.Depth);
            sb.Append(" time " + moveInfo.Time);
            sb.Append(" nodes " + moveInfo.Nodes);
            sb.Append(" score " + moveInfo.Evaluation.ToString());
            sb.Append(" pv " + string.Join(' ', moveInfo.Moves.Select(mi => mi.ToString())));

            Console.WriteLine(sb.ToString());
            lastBestMove = moveInfo.Moves.First();
        }

        private void Searcher_IterationFailedLow(object sender, MoveInfo moveInfo)
        {
            var sb = new StringBuilder("info");
            sb.Append(" depth " + moveInfo.Depth);
            sb.Append(" time " + moveInfo.Time);
            sb.Append(" nodes " + moveInfo.Nodes);
            sb.Append(" score " + moveInfo.Evaluation.ToString());
            sb.Append(" lowerbound");
            sb.Append(" pv " + lastBestMove);

            Console.WriteLine(sb.ToString());
        }

        private void Searcher_IterationFailedHigh(object sender, MoveInfo moveInfo)
        {
            var sb = new StringBuilder("info");
            sb.Append(" depth " + moveInfo.Depth);
            sb.Append(" time " + moveInfo.Time);
            sb.Append(" nodes " + moveInfo.Nodes);
            sb.Append(" score " + moveInfo.Evaluation.ToString());
            sb.Append(" upperbound");
            sb.Append(" pv " + lastBestMove);

            Console.WriteLine(sb.ToString());
        }

        private void Searcher_PerftComplete(object sender, PerftResults results)
        {
            Console.WriteLine();
            Console.WriteLine("Perft test complete");
            Console.WriteLine("Result: " + results.PermutationCount);
            Console.WriteLine("Time (ms): " + results.ElapsedMilliseconds);
            Console.WriteLine("Nodes per second: " + results.NodesPerSecond);
        }

        private void Bench_BenchComplete(object sender, BenchResults results)
        {
            Console.WriteLine("Bench test complete");
            Console.WriteLine("Nodes searched: " + results.NodesSearched);
            Console.WriteLine("Time (ms): " + results.BenchTimeMs);
            Console.WriteLine("Nodes per second: " + results.NodesPerSecond);
        }
    }
}
