using Charlie.BoardRepresentation;
using Charlie.Hash;
using Charlie.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Charlie.Search
{
    public class Searcher
    {
        private bool cancel;
        private readonly Timer timer = new Timer() { AutoReset = false };
        private readonly Stopwatch sw = new Stopwatch();

        private readonly Evaluator evaluator = new Evaluator();
        private readonly MoveGenerator generator = new MoveGenerator();

        private ulong nodesSearched;

        private readonly HashTable HashTable = new HashTable();

        public event EventHandler<MoveInfo> IterationCompleted;
        public event EventHandler<MoveInfo> IterationFailedHigh;
        public event EventHandler<MoveInfo> IterationFailedLow;
        public event EventHandler<SearchResults> SearchComplete;
        public event EventHandler<PerftResults> PerftComplete;

        public Searcher() => timer.Elapsed += (s, e) => cancel = true;

        public async Task Start(BoardState currentBoard, SearchParameters searchParameters)
        {
            cancel = false;
            nodesSearched = 0;
            sw.Start();

            if (searchParameters.SearchType == SearchType.Time)
            {
                timer.Interval = searchParameters.SearchTime.MaxTime;
                timer.Start();
            }

            Move bestMove = default;
            List<Move> pv;
            Move[] prevPv = Array.Empty<Move>();
            Score eval;
            Score alpha = Score.NegativeInfinity;
            Score beta = Score.Infinity;
            int depth = 1;
            int failedSearches = 0;

            while (true)
            {
                pv = new List<Move>();
                eval = await AlphaBeta(currentBoard, alpha, beta, depth, 0, pv, prevPv);
                bool isMate = eval.IsMateScore();

                // Check if a stop command has been sent
                if (cancel) break;

                // If fail high/low, reset aspiration windows and try again
                if (eval <= alpha || eval >= beta)
                {
                    failedSearches++;

                    // Extract the pv
                    prevPv = new[] { bestMove };

                    // Report the pv
                    var failedSearchInfo = new MoveInfo(depth, prevPv, eval, sw.ElapsedMilliseconds, nodesSearched);
                    if (eval <= alpha)
                    {
                        IterationFailedLow?.Invoke(this, failedSearchInfo);
                        alpha = failedSearches > 1 ? Score.NegativeInfinity : eval - 23;
                        beta = failedSearches > 1 ? Score.Infinity : eval;
                    }
                    else if (eval >= beta)
                    {
                        IterationFailedHigh?.Invoke(this, failedSearchInfo);
                        alpha = failedSearches > 1 ? Score.NegativeInfinity : eval;
                        beta = failedSearches > 1 ? Score.Infinity : eval + 23;
                    }

                    // Don't try again if we found mate because we won't find anything better
                    if (!isMate) continue;
                }

                // Extract the pv
                bool bestMoveChanged = false;
                prevPv = pv.ToArray();
                if (bestMove.IsValidMove() && prevPv[0] != bestMove) bestMoveChanged = true;
                bestMove = prevPv[0];

                // Report the pv
                var moveInfo = new MoveInfo(depth, prevPv, eval, sw.ElapsedMilliseconds, nodesSearched);
                IterationCompleted?.Invoke(this, moveInfo);

                // Set new aspiration windows
                alpha = eval - 27;
                beta = eval + 27;
                depth++;
                failedSearches = 0;

                // Check if we need to abort search
                if (!searchParameters.CanContinueSearching(depth, sw.ElapsedMilliseconds, eval, bestMoveChanged, isMate)) break;
            }

            // Stop the search and report the results
            SearchResults results = new SearchResults(bestMove, nodesSearched, sw.ElapsedMilliseconds);
            SearchComplete?.Invoke(this, results);
            Stop();
        }

        public void Stop()
        {
            timer.Stop();
            sw.Reset();
            cancel = true;
        }

        public void ClearHash() => HashTable.Clear();

        private async Task<Score> AlphaBeta(BoardState boardState, Score alpha, Score beta, int depth, int height, List<Move> pv, Move[] pvMoves)
        {
            var foundPv = false;
            var isRoot = height == 0;

            if (depth <= 0)
            {
                nodesSearched++;
                return await Quiesce(boardState, alpha, beta);
            }

            IEnumerable<Move> moves = GenerateOrderedMoves(boardState, pvMoves);

            if (!moves.Any())
            {
                nodesSearched++;

                if (boardState.IsInCheck(boardState.ToMove))
                    return height - Score.Mate;
                else return Score.Draw;
            }

            Move bestMove = default;
            bool isFirstMove = true;

            foreach (Move move in moves)
            {
                bool isPvMove = pvMoves.Length > 0 && pvMoves[0].Equals(move);
                Move[] childPvMoves = isPvMove ? pvMoves[1..] : Array.Empty<Move>();
                var pvBuffer = new List<Move>();
                var childDepth = depth - 1;

                Score eval = Score.Draw;
                BoardState newBoard = boardState.MakeMove(move);

                // Reductions and extensions
                int extension = 0;
                if (!isRoot)
                {
                    // Quiet move reduction
                    if (!isPvMove && childDepth == 1 && !move.IsCaptureOrPromotion(boardState))
                        extension--;

                    // Promotion extension
                    if (move.PromotionType != PromotionType.None)
                        extension++;

                    // PV extension
                    if (isPvMove && childDepth == 2)
                        extension++;

                    // Latter move reduction (we assume that the first move generated will be the best)
                    if (!isFirstMove && childDepth >= 2 && !move.IsCaptureOrPromotion(boardState))
                        extension--;

                    // Check extension
                    if (newBoard.IsInCheck(newBoard.ToMove))
                    {
                        extension++;

                        if (childDepth == 1)
                            extension++;
                    }

                    childDepth += extension;
                }

                if (newBoard.IsThreeMoveRepetition())
                {
                    nodesSearched++;
                    eval = Score.Draw;
                }
                // Early quiescence
                else if (childDepth == 1 && move.IsCaptureOrPromotion(boardState))
                {
                    nodesSearched++;
                    eval = -await Quiesce(newBoard, -beta, -alpha);
                }
                else if (foundPv)
                {
                    eval = -await AlphaBeta(newBoard, -alpha - 1, -alpha, childDepth, height + 1, pvBuffer, childPvMoves);

                    if (eval > alpha && eval < beta)
                        eval = -await AlphaBeta(newBoard, -beta, -alpha, childDepth, height + 1, pvBuffer, childPvMoves);
                }
                else
                {
                    eval = -await AlphaBeta(newBoard, -beta, -alpha, childDepth, height + 1, pvBuffer, childPvMoves);
                }

                if (cancel) break;

                if (eval >= beta)
                {
                    HashTable.RecordHash(boardState.HashCode, depth, move);
                    return beta;
                }

                if (eval > alpha)
                {
                    alpha = eval;
                    foundPv = true;
                    bestMove = move;

                    pv.Clear();
                    pv.Add(move);
                    pv.AddRange(pvBuffer);
                }

                isFirstMove = false;
            }

            HashTable.RecordHash(boardState.HashCode, depth, bestMove);

            return alpha;
        }

        private async Task<Score> Quiesce(BoardState boardState, Score alpha, Score beta)
        {
            Score eval = evaluator.Evaluate(boardState);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;

            IEnumerable<Move> moves = generator.GenerateQuiescenceMoves(boardState);

            foreach (Move move in moves)
            {
                BoardState newBoardState = boardState.MakeMove(move);

                eval = -await Quiesce(newBoardState, -beta, -alpha);

                if (eval >= beta) return beta;
                if (eval > alpha) alpha = eval;
            }

            return alpha;
        }

        private IEnumerable<Move> GenerateOrderedMoves(BoardState boardState, Move[] pvMoves)
        {
            Move ttBestMove = HashTable.ProbeHash(boardState.HashCode);
            List<Move> bestMoves = new List<Move>();

            if (pvMoves.Length > 0 && !pvMoves[0].Equals(ttBestMove))
                bestMoves.Add(pvMoves[0]);

            if (ttBestMove.IsValidMove())
                bestMoves.Add(ttBestMove);

            return generator.GenerateLegalMoves(boardState, bestMoves);
        }

        public async Task PerfTest(BoardState currentBoard, int rootDepth)
        {
            sw.Start();

            ulong permutationCount = await PerfTestInner(currentBoard, rootDepth);
            long timeTaken = sw.ElapsedMilliseconds;

            Stop();

            var results = new PerftResults(permutationCount, (ulong)timeTaken);
            PerftComplete?.Invoke(this, results);

            async Task<ulong> PerfTestInner(BoardState boardState, int subDepth)
            {
                ulong count = 0;

                if (subDepth == 0) return 1;

                var moves = generator.GenerateLegalMoves(boardState);

                foreach (Move move in moves)
                {
                    var newBoard = boardState.MakeMove(move);
                    var perft = await PerfTestInner(newBoard, subDepth - 1);

                    if (subDepth == rootDepth) Console.WriteLine($"{move}: {perft}");

                    count += perft;
                }

                return count;
            }
        }
    }
}
