using Charlie.BoardRepresentation;
using Charlie.Hash;
using Charlie.Moves;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace Charlie.Search;

public class Searcher
{
    private const int MaxSearchPly = 256;

    private bool cancel;
    private readonly Timer timer = new() { AutoReset = false };
    private readonly Stopwatch sw = new();

    private ulong nodesSearched;

    private readonly HashTable HashTable = new();
    private readonly UndoState[] undoStack = new UndoState[MaxSearchPly];
    private long[] repetitionHashes = [];
    private int repetitionBaseLength;

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

        SearchPosition searchPosition = currentBoard.ToSearchPosition();
        long[] rootHistory = currentBoard.GetHistoryHashes();
        repetitionBaseLength = rootHistory.Length;
        repetitionHashes = new long[repetitionBaseLength + MaxSearchPly + 1];
        Array.Copy(rootHistory, repetitionHashes, repetitionBaseLength);

        if (searchParameters.SearchType == SearchType.Time)
        {
            timer.Interval = searchParameters.SearchTime.MaxTime;
            timer.Start();
        }

        Move bestMove = default;
        List<Move> pv;
        Move[] prevPv = [];
        Score eval;
        Score alpha = Score.NegativeInfinity;
        Score beta = Score.Infinity;
        int depth = 1;
        var rootMoves = new RootMoves();
        rootMoves.Generate(currentBoard);
        rootMoves.SortByPromise();

        while (rootMoves.Count > 0)
        {
            pv = [];
            eval = await AlphaBeta(searchPosition, alpha, beta, depth, rootMoves, pv, prevPv, 0);
            rootMoves.SortByPromise();

            bool isMate = eval.IsMateScore();

            // Check if a stop command has been sent
            if (cancel) break;

            // If fail high/low, reset aspiration windows and try again
            if (eval <= alpha || eval >= beta)
            {
                // Extract the pv
                prevPv = [bestMove];

                // Report the pv
                var failedSearchInfo = new MoveInfo(depth, prevPv, eval, sw.ElapsedMilliseconds, nodesSearched);

                if (eval <= alpha) IterationFailedLow?.Invoke(this, failedSearchInfo);
                else if (eval >= beta) IterationFailedHigh?.Invoke(this, failedSearchInfo);

                alpha = Score.NegativeInfinity;
                beta = Score.Infinity;

                // Don't try again if we found mate because we won't find anything better
                if (!isMate) continue;
            }

            // Extract the pv
            bool bestMoveChanged = false;
            prevPv = [.. pv];
            if (bestMove.IsValidMove() && prevPv[0] != bestMove) bestMoveChanged = true;
            bestMove = prevPv[0];

            // Report the pv
            var moveInfo = new MoveInfo(depth, prevPv, eval, sw.ElapsedMilliseconds, nodesSearched);
            IterationCompleted?.Invoke(this, moveInfo);

            // Set new aspiration windows
            alpha = eval - 35;
            beta = eval + 30;
            depth++;

            // Check if we need to abort search
            if (!searchParameters.CanContinueSearching(
                depth,
                sw.ElapsedMilliseconds,
                eval,
                bestMoveChanged,
                rootMoves.GetConfidence(bestMove),
                isMate)) break;
        }

        // Stop the search and report the results
        var results = new SearchResults(bestMove, nodesSearched, sw.ElapsedMilliseconds);
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

    private async ValueTask<Score> AlphaBeta(
        SearchPosition boardState,
        Score alpha,
        Score beta,
        int depth,
        RootMoves moves,
        List<Move> pv,
        Move[] pvMoves,
        int pvIndex)
    {
        var foundPv = false;
        Move bestMove = default;

        for (int moveIndex = 0; moveIndex < moves.Count; moveIndex++)
        {
            Move move = moves[moveIndex].Move;
            bool isPvMove = pvIndex < pvMoves.Length && pvMoves[pvIndex].Equals(move);
            int childPvIndex = isPvMove ? pvIndex + 1 : pvMoves.Length;
            List<Move> pvBuffer = [];

            var childDepth = depth - 1;

            if (moves[moveIndex].Promise < Math.Sqrt(2 * childDepth))
                childDepth--;

            if (!move.IsCaptureOrPromotion(boardState))
                childDepth--;

            Score eval;
            boardState.MakeMoveInPlace(move, ref undoStack[0]);
            RecordRepetitionHash(1, boardState.HashCode);

            try
            {
                if (foundPv)
                {
                    eval = -await AlphaBetaInternal(
                        boardState,
                        -alpha - 1,
                        -alpha,
                        childDepth,
                        1,
                        1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);

                    if (eval > alpha && eval < beta)
                    {
                        eval = -await AlphaBetaInternal(
                            boardState,
                            -beta,
                            -alpha,
                            childDepth,
                            1,
                            1,
                            pvBuffer,
                            pvMoves,
                            childPvIndex);
                    }
                }
                else
                {
                    eval = -await AlphaBetaInternal(
                        boardState,
                        -beta,
                        -alpha,
                        childDepth,
                        1,
                        1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);
                }
            }
            finally
            {
                boardState.UnmakeMove(move, in undoStack[0]);
            }

            moves[moveIndex].Score = eval;

            if (cancel) break;

            if (eval >= beta)
            {
                pv.Clear();
                pv.Add(move);
                pv.AddRange(pvBuffer);
                moves[moveIndex].IncreasePromise(11);

                HashTable.RecordHash(boardState.HashCode, depth, move);
                return eval;
            }

            if (eval > alpha)
            {
                alpha = eval;
                bestMove = move;
                moves[moveIndex].IncreasePromise(7);
                foundPv = true;

                pv.Clear();
                pv.Add(move);
                pv.AddRange(pvBuffer);
            }
            else if (moveIndex > moves.Count / 3 && !foundPv)
            {
                return eval;
            }
        }

        HashTable.RecordHash(boardState.HashCode, depth, bestMove);
        return alpha;
    }

    private async ValueTask<Score> AlphaBetaInternal(
        SearchPosition boardState,
        Score alpha,
        Score beta,
        int depth,
        int height,
        int ply,
        List<Move> pv,
        Move[] pvMoves,
        int pvIndex)
    {
        var foundPv = false;

        if (depth <= 0)
        {
            nodesSearched++;
            return await Quiesce(boardState, alpha, beta, ply);
        }

        using IEnumerator<Move> moveEnumerator = GenerateOrderedMoves(boardState, pvMoves, pvIndex).GetEnumerator();

        if (!moveEnumerator.MoveNext())
        {
            nodesSearched++;

            if (boardState.IsInCheck(boardState.ToMove))
                return height - Score.Mate;
            else return Score.Draw;
        }

        Move bestMove = default;
        bool isFirstMove = true;

        do
        {
            Move move = moveEnumerator.Current;
            bool isPvMove = pvIndex < pvMoves.Length && pvMoves[pvIndex].Equals(move);
            int childPvIndex = isPvMove ? pvIndex + 1 : pvMoves.Length;
            List<Move> pvBuffer = [];
            var childDepth = depth - 1;
            bool isCaptureOrPromotion = move.IsCaptureOrPromotion(boardState);

            Score eval = Score.Draw;
            boardState.MakeMoveInPlace(move, ref undoStack[ply]);
            RecordRepetitionHash(ply + 1, boardState.HashCode);

            try
            {
                // Reductions and extensions
                int extension = 0;

                // Promotion extension
                if (move.PromotionType != PromotionType.None)
                    extension++;

                // PV extension
                if (isPvMove && childDepth == 2)
                    extension++;

                // Latter move reduction (we assume that the first move generated will be the best)
                if (!isFirstMove && !isCaptureOrPromotion)
                    extension--;

                // Check extension
                if (boardState.IsInCheck(boardState.ToMove))
                    extension++;

                childDepth += extension;

                if (IsThreeMoveRepetition(ply + 1, boardState.HashCode))
                {
                    nodesSearched++;
                    eval = Score.Draw;
                }
                // Early quiescence
                else if (childDepth == 1 && isCaptureOrPromotion)
                {
                    nodesSearched++;
                    eval = -await Quiesce(boardState, -beta, -alpha, ply + 1);
                }
                else if (foundPv)
                {
                    eval = -await AlphaBetaInternal(
                        boardState,
                        -alpha - 1,
                        -alpha,
                        childDepth,
                        height + 1,
                        ply + 1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);

                    if (eval > alpha && eval < beta)
                    {
                        eval = -await AlphaBetaInternal(
                            boardState,
                            -beta,
                            -alpha,
                            childDepth,
                            height + 1,
                            ply + 1,
                            pvBuffer,
                            pvMoves,
                            childPvIndex);
                    }
                }
                else
                {
                    eval = -await AlphaBetaInternal(
                        boardState,
                        -beta,
                        -alpha,
                        childDepth,
                        height + 1,
                        ply + 1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);
                }
            }
            finally
            {
                boardState.UnmakeMove(move, in undoStack[ply]);
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
        while (moveEnumerator.MoveNext());

        HashTable.RecordHash(boardState.HashCode, depth, bestMove);

        return alpha;
    }

    private async ValueTask<Score> Quiesce(SearchPosition boardState, Score alpha, Score beta, int ply)
    {
        Score eval = Evaluator.Evaluate(boardState);

        if (eval >= beta) return beta;
        if (eval > alpha) alpha = eval;

        IEnumerable<Move> moves = MoveGenerator.GenerateQuiescenceMoves(boardState);

        foreach (Move move in moves)
        {
            boardState.MakeMoveInPlace(move, ref undoStack[ply]);
            RecordRepetitionHash(ply + 1, boardState.HashCode);

            try
            {
                eval = -await Quiesce(boardState, -beta, -alpha, ply + 1);
            }
            finally
            {
                boardState.UnmakeMove(move, in undoStack[ply]);
            }

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;
        }

        return alpha;
    }

    private IEnumerable<Move> GenerateOrderedMoves(SearchPosition boardState, Move[] pvMoves, int pvIndex)
    {
        Move ttBestMove = HashTable.ProbeHash(boardState.HashCode);
        bool hasPvMove = pvIndex < pvMoves.Length && !pvMoves[pvIndex].Equals(ttBestMove);
        Move pvMove = hasPvMove ? pvMoves[pvIndex] : default;
        bool hasTtBestMove = ttBestMove.IsValidMove();

        return MoveGenerator.GenerateLegalMoves(boardState, pvMove, hasPvMove, ttBestMove, hasTtBestMove);
    }

    public async Task PerfTest(BoardState currentBoard, int rootDepth)
    {
        sw.Start();

        SearchPosition rootPosition = currentBoard.ToSearchPosition();
        ulong permutationCount = await PerfTestInner(rootPosition, rootDepth, 0);
        long timeTaken = sw.ElapsedMilliseconds;

        Stop();

        var results = new PerftResults(permutationCount, (ulong)timeTaken);
        PerftComplete?.Invoke(this, results);

        async ValueTask<ulong> PerfTestInner(SearchPosition boardState, int subDepth, int ply)
        {
            ulong count = 0;

            if (subDepth == 0) return 1;

            var moves = MoveGenerator.GenerateLegalMoves(boardState);

            foreach (Move move in moves)
            {
                boardState.MakeMoveInPlace(move, ref undoStack[ply]);
                var perft = await PerfTestInner(boardState, subDepth - 1, ply + 1);
                boardState.UnmakeMove(move, in undoStack[ply]);

                if (subDepth == rootDepth) Console.WriteLine($"{move}: {perft}");

                count += perft;
            }

            return count;
        }
    }

    private void RecordRepetitionHash(int ply, long hash) =>
        repetitionHashes[repetitionBaseLength - 1 + ply] = hash;

    private bool IsThreeMoveRepetition(int ply, long hash)
    {
        int count = 0;
        int upperBound = repetitionBaseLength - 1 + ply;

        for (int i = 0; i <= upperBound; i++)
        {
            if (repetitionHashes[i] == hash && ++count == 3)
                return true;
        }

        return false;
    }
}
