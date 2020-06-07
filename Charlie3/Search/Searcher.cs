using Charlie.Board;
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

        private const int InfinityScore = 1 << 24;
        private const int NegativeInfinityScore = -InfinityScore;
        private const int MateScore = 1 << 20;
        private const int DrawScore = 0;
        private const int UnknownScore = 1 << 21;

        private readonly Evaluator evaluator = new Evaluator();
        private readonly MoveGenerator generator = new MoveGenerator();

        private ulong nodesSearched;

        private readonly Dictionary<long, HashElement> HashTable = new Dictionary<long, HashElement>();

        public event EventHandler<MoveInfo> BestMoveChanged;
        public event EventHandler<SearchResults> SearchComplete;

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
            Move[] prevPv = new Move[0];
            int eval, depth = 1;
            int alpha = NegativeInfinityScore, beta = InfinityScore;

            while (true)
            {
                pv = new List<Move>();
                eval = await AlphaBeta(currentBoard, alpha, beta, depth, 0, pv, prevPv);
                bool isMate = IsMateScore(eval);

                // Check if a stop command has been sent
                if (cancel) break;

                // If fail high/low, reset aspiration windows and try again
                if (eval <= alpha)
                {
                    alpha = NegativeInfinityScore;
                    // Don't try again if we found mate because we won't find anything better
                    if (!isMate) continue;
                }
                else if (eval >= beta)
                {
                    beta = InfinityScore;
                    // Don't try again if we found mate because we won't find anything better
                    if (!isMate) continue;
                }

                // Extract the pv
                prevPv = pv.ToArray();
                bestMove = prevPv[0];

                // Report the pv
                var moveInfo = new MoveInfo(depth, prevPv, eval, isMate, sw.ElapsedMilliseconds, nodesSearched);
                BestMoveChanged?.Invoke(this, moveInfo);

                // Set new aspiration windows
                alpha = eval - 30;
                beta = eval + 30;
                depth++;

                // Check if we need to abort search
                if (searchParameters.SearchType == SearchType.Time)
                {
                    if (sw.ElapsedMilliseconds * 4 > searchParameters.SearchTime.IdealTime + searchParameters.SearchTime.Increment) break;
                    if (isMate) break;
                }

                if (searchParameters.SearchType == SearchType.Depth
                    && depth > searchParameters.DepthLimit)
                    break;
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

        private int ProbeHash(long hash, int depth, int alpha, int beta)
        {
            if (!HashTable.ContainsKey(hash)) return UnknownScore;

            HashElement hashElement = HashTable[hash];

            if (hashElement.Depth < depth) return UnknownScore;

            return hashElement.Type switch
            {
                HashType.Exact => hashElement.Evaluation,
                HashType.Alpha when hashElement.Evaluation <= alpha => alpha,
                HashType.Beta when hashElement.Evaluation >= beta => beta,
                _ => UnknownScore
            };
        }

        private void RecordHash(long hashKey, int depth, HashType type, int evaluation)
        {
            if (!HashTable.ContainsKey(hashKey) || HashTable[hashKey].Depth < depth)
                HashTable[hashKey] = new HashElement(depth, type, evaluation);
        }

        private async Task<int> AlphaBeta(BoardState boardState, int alpha, int beta, int depth, int height, List<Move> pv, Move[] pvMoves)
        {
            var foundPv = false;
            var isRoot = height == 0;
            HashType hashType = HashType.Alpha;
            var ttValue = ProbeHash(boardState.HashCode, depth, alpha, beta);

            if (!isRoot && ttValue != UnknownScore)
            {
                if (IsMateScore(ttValue))
                    return (MateScore - (height + MatePlies(ttValue) - 1)) * (ttValue > 0 ? 1 : -1);

                return ttValue;
            }

            if (depth == 0)
            {
                nodesSearched++;
                var eval = await Quiesce(boardState, alpha, beta);
                RecordHash(boardState.HashCode, depth, HashType.Exact, eval);
                return eval;
            }

            // Check extension - ~200 elo
            if (boardState.IsInCheck(boardState.ToMove) && !isRoot)
                depth++;

            IEnumerable<Move> moves = generator.GenerateLegalMoves(boardState);
            if (pvMoves.Length > 0)
                moves = moves.ToList().MoveToFront(pvMoves[0]);

            if (!moves.Any())
            {
                nodesSearched++;
                int eval;

                if (boardState.IsInCheck(boardState.ToMove))
                    eval = (-MateScore) + height;
                else eval = DrawScore;

                RecordHash(boardState.HashCode, depth, HashType.Exact, eval);
                return eval;
            }

            foreach (Move move in moves)
            {
                bool isPvMove = pvMoves.Length > 0 && pvMoves[0].Equals(move);
                Move[] childPvMoves = isPvMove ? pvMoves[1..] : new Move[0];
                var pvBuffer = new List<Move>();

                int eval = DrawScore;
                BoardState newBoard = boardState.MakeMove(move);

                if (newBoard.IsThreeMoveRepetition())
                {
                    nodesSearched++;
                    eval = DrawScore;
                    RecordHash(newBoard.HashCode, depth - 1, HashType.Exact, eval);
                }
                // Early quiescence - ~50 elo
                else if (depth == 2 && move.IsCaptureOrPromotion(boardState))
                {
                    nodesSearched++;
                    eval = -await Quiesce(newBoard, -beta, -alpha);
                }
                else if (foundPv)
                {
                    eval = -await AlphaBeta(newBoard, -alpha - 1, -alpha, depth - 1, height + 1, pvBuffer, childPvMoves);

                    if (eval > alpha && eval < beta)
                        eval = -await AlphaBeta(newBoard, -beta, -alpha, depth - 1, height + 1, pvBuffer, childPvMoves);
                }
                else
                {
                    eval = -await AlphaBeta(newBoard, -beta, -alpha, depth - 1, height + 1, pvBuffer, childPvMoves);
                }

                if (cancel) break;

                if (eval >= beta)
                {
                    RecordHash(boardState.HashCode, depth, HashType.Beta, beta);
                    return beta;
                }

                if (eval > alpha)
                {
                    alpha = eval;
                    foundPv = true;
                    hashType = HashType.Exact;

                    pv.Clear();
                    pv.Add(move);
                    pv.AddRange(pvBuffer);
                }
            }

            if (!cancel) RecordHash(boardState.HashCode, depth, hashType, alpha);

            return alpha;
        }

        private async Task<int> Quiesce(BoardState boardState, int alpha, int beta)
        {
            int eval = evaluator.Evaluate(boardState);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;

            IEnumerable<Move> moves = generator.GenerateLegalMoves(boardState)
                                               .Where(move => move.IsCaptureOrPromotion(boardState));

            foreach (Move move in moves)
            {
                BoardState newBoardState = boardState.MakeMove(move); ;

                eval = -await Quiesce(newBoardState, -beta, -alpha);

                if (eval >= beta) return beta;
                if (eval > alpha) alpha = eval;
            }

            return alpha;
        }

        private bool IsMateScore(int eval) => Math.Abs(eval) > (MateScore - 100);

        private int MatePlies(int mateScore) => MateScore - Math.Abs(mateScore);
    }
}
