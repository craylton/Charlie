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

    private int testValue;

    private volatile bool cancel;
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

    public static int DefaultHashSizeInMegabytes => HashTable.DefaultSizeInMegabytes;
    public int HashSizeInMegabytes => HashTable.SizeInMegabytes;

    public Searcher() => timer.Elapsed += (s, e) => cancel = true;

    public void Start(BoardState currentBoard, SearchParameters searchParameters)
    {
        cancel = false;
        nodesSearched = 0;
        sw.Restart();

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
            eval = AlphaBeta(searchPosition, alpha, beta, depth, rootMoves, pv, prevPv, 0);
            rootMoves.SortByPromise();

            bool failedLow = eval <= alpha;
            bool failedHigh = eval >= beta;
            bool isMate = eval.IsMateScore();

            // Check if a stop command has been sent
            if (cancel) break;

            // If fail high/low, reset aspiration windows and try again
            if (failedLow || failedHigh)
            {
                // Extract the pv
                prevPv = bestMove.IsValidMove() ? [bestMove] : [];

                // Report the pv
                var failedSearchInfo = new MoveInfo(depth, prevPv, eval, sw.ElapsedMilliseconds, nodesSearched);

                if (failedLow) IterationFailedLow?.Invoke(this, failedSearchInfo);
                else if (failedHigh) IterationFailedHigh?.Invoke(this, failedSearchInfo);

                alpha = Score.NegativeInfinity;
                beta = Score.Infinity;

                // Don't try again if we found mate because we won't find anything better
                if (failedLow || !isMate) continue;
            }

            // Extract the pv
            prevPv = pv.Count > 0
                ? [.. pv]
                : bestMove.IsValidMove()
                    ? [bestMove]
                    : [];

            if (prevPv.Length == 0)
                break;

            Move nextBestMove = prevPv[0];
            bool bestMoveChanged = false;
            if (bestMove.IsValidMove() && nextBestMove != bestMove) bestMoveChanged = true;
            bestMove = nextBestMove;

            // Report the pv
            var moveInfo = new MoveInfo(depth, prevPv, eval, sw.ElapsedMilliseconds, nodesSearched);
            IterationCompleted?.Invoke(this, moveInfo);

            // Set new aspiration windows
            alpha = eval - 38;
            beta = eval + 38;
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

    public void SetHashSize(int sizeInMegabytes) => HashTable.Resize(sizeInMegabytes);

    private Score AlphaBeta(
        SearchPosition boardState,
        Score alpha,
        Score beta,
        int depth,
        RootMoves moves,
        List<Move> pv,
        Move[] pvMoves,
        int pvIndex)
    {
        Score originalAlpha = alpha;
        bool hasRepetitionHistory = CountRepetitions(0, boardState.HashCode) > 1;
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
                    eval = -AlphaBetaInternal(
                        boardState,
                        -alpha - 1,
                        -alpha,
                        childDepth,
                        1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);

                    if (eval > alpha && eval < beta)
                    {
                        eval = -AlphaBetaInternal(
                            boardState,
                            -beta,
                            -alpha,
                            childDepth,
                            1,
                            pvBuffer,
                            pvMoves,
                            childPvIndex);
                    }
                }
                else
                {
                    eval = -AlphaBetaInternal(
                        boardState,
                        -beta,
                        -alpha,
                        childDepth,
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
                moves[moveIndex].IncreasePromise(testValue);

                if (!hasRepetitionHistory)
                    RecordHash(boardState.HashCode, depth, eval, move, HashType.Lower, 0);

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
        }

        if (!cancel && !hasRepetitionHistory)
        {
            HashType hashType = alpha <= originalAlpha ? HashType.Upper : HashType.Exact;
            RecordHash(boardState.HashCode, depth, alpha, bestMove, hashType, 0);
        }

        return alpha;
    }

    private Score AlphaBetaInternal(
        SearchPosition boardState,
        Score alpha,
        Score beta,
        int depth,
        int height,
        List<Move> pv,
        Move[] pvMoves,
        int pvIndex)
    {
        Score originalAlpha = alpha;
        int repetitionCount = CountRepetitions(height, boardState.HashCode);

        if (repetitionCount >= 3)
        {
            nodesSearched++;
            return Score.Draw;
        }

        bool hasRepetitionHistory = repetitionCount > 1;
        var foundPv = false;

        if (depth <= 0)
        {
            nodesSearched++;
            return Quiesce(boardState, alpha, beta, height);
        }

        Move ttBestMove = default;

        if (!hasRepetitionHistory && HashTable.TryProbeHash(boardState.HashCode, out HashElement hashEntry))
        {
            ttBestMove = hashEntry.Move;
            Score hashScore = UnpackHashScore(hashEntry.Score, height);

            if (hashEntry.Depth >= depth && CanUseHashScore(hashEntry.Type, hashScore, alpha, beta))
            {
                if (hashEntry.Move.IsValidMove())
                {
                    pv.Clear();
                    pv.Add(hashEntry.Move);
                }

                return hashScore;
            }
        }

        using IEnumerator<Move> moveEnumerator = GenerateOrderedMoves(boardState, pvMoves, pvIndex, ttBestMove).GetEnumerator();

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
            boardState.MakeMoveInPlace(move, ref undoStack[height]);
            RecordRepetitionHash(height + 1, boardState.HashCode);

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

                if (CountRepetitions(height + 1, boardState.HashCode) >= 3)
                {
                    nodesSearched++;
                    eval = Score.Draw;
                }
                // Early quiescence
                else if (childDepth == 1 && isCaptureOrPromotion)
                {
                    nodesSearched++;
                    eval = -Quiesce(boardState, -beta, -alpha, height + 1);
                }
                else if (foundPv)
                {
                    eval = -AlphaBetaInternal(
                        boardState,
                        -alpha - 1,
                        -alpha,
                        childDepth,
                        height + 1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);

                    if (eval > alpha && eval < beta)
                    {
                        eval = -AlphaBetaInternal(
                            boardState,
                            -beta,
                            -alpha,
                            childDepth,
                            height + 1,
                            pvBuffer,
                            pvMoves,
                            childPvIndex);
                    }
                }
                else
                {
                    eval = -AlphaBetaInternal(
                        boardState,
                        -beta,
                        -alpha,
                        childDepth,
                        height + 1,
                        pvBuffer,
                        pvMoves,
                        childPvIndex);
                }
            }
            finally
            {
                boardState.UnmakeMove(move, in undoStack[height]);
            }

            if (cancel) break;

            if (eval >= beta)
            {
                if (!hasRepetitionHistory)
                    RecordHash(boardState.HashCode, depth, eval, move, HashType.Lower, height);

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

        if (!cancel && !hasRepetitionHistory)
        {
            HashType hashType = alpha <= originalAlpha ? HashType.Upper : HashType.Exact;
            RecordHash(boardState.HashCode, depth, alpha, bestMove, hashType, height);
        }

        return alpha;
    }

    private Score Quiesce(SearchPosition boardState, Score alpha, Score beta, int ply)
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
                eval = -Quiesce(boardState, -beta, -alpha, ply + 1);
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

    private IEnumerable<Move> GenerateOrderedMoves(SearchPosition boardState, Move[] pvMoves, int pvIndex, Move ttBestMove = default)
    {
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

    private void RecordHash(long hashKey, int depth, Score score, Move move, HashType type, int ply) =>
        HashTable.RecordHash(hashKey, depth, PackHashScore(score, ply), move, type);

    private static bool CanUseHashScore(HashType type, Score score, Score alpha, Score beta) =>
        type switch
        {
            HashType.Exact => true,
            HashType.Lower => score >= beta,
            HashType.Upper => score <= alpha,
            _ => false,
        };

    private static Score PackHashScore(Score score, int ply)
    {
        if (!score.IsMateScore()) return score;

        int rawScore = (int)score;
        return score.IsPositive()
            ? new Score(rawScore + ply)
            : new Score(rawScore - ply);
    }

    private static Score UnpackHashScore(Score score, int ply)
    {
        if (!score.IsMateScore()) return score;

        int rawScore = (int)score;
        return score.IsPositive()
            ? new Score(rawScore - ply)
            : new Score(rawScore + ply);
    }

    private void RecordRepetitionHash(int ply, long hash) =>
        repetitionHashes[repetitionBaseLength - 1 + ply] = hash;

    private int CountRepetitions(int ply, long hash)
    {
        int count = 0;
        int upperBound = repetitionBaseLength - 1 + ply;

        for (int i = 0; i <= upperBound; i++)
        {
            if (repetitionHashes[i] == hash)
                count++;
        }

        return count;
    }

    internal void SetTestValue(int testValue) => this.testValue = testValue;
}
