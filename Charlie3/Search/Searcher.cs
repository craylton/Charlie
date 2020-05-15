using Charlie.Board;
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

        private readonly Evaluator evaluator = new Evaluator();
        private readonly MoveGenerator generator = new MoveGenerator();

        private ulong nodesSearched;

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
            Move[] pv, prevPv = new Move[0];
            int eval = DrawScore, depth = 1;
            int alpha = NegativeInfinityScore, beta = InfinityScore;

            while (true)
            {
                pv = new Move[depth];
                eval = await AlphaBeta(currentBoard, alpha, beta, depth, 0, pv, prevPv);

                if (cancel) break;

                if (eval <= alpha)
                {
                    alpha -= 100;
                    continue;
                }
                else if (eval >= beta)
                {
                    beta += 100;
                    continue;
                }

                prevPv = pv.Reverse().TakeWhile(move => move.IsValid()).ToArray();
                bestMove = prevPv[0];
                bool isMate = Math.Abs(eval) > MateScore - 100;

                var moveInfo = new MoveInfo(depth, prevPv, eval, isMate, sw.ElapsedMilliseconds, nodesSearched);
                BestMoveChanged?.Invoke(this, moveInfo);

                alpha = eval - 30;
                beta = eval + 30;
                depth++;

                if (searchParameters.SearchType == SearchType.Time)
                {
                    if (sw.ElapsedMilliseconds * 4 > searchParameters.SearchTime.IdealTime) break;
                    if (isMate) break;
                }

                if (searchParameters.SearchType == SearchType.Depth
                    && depth > searchParameters.DepthLimit)
                    break;

                if (cancel)
                    break;
            }

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

        private async Task<int> AlphaBeta(BoardState boardState, int alpha, int beta, int depth, int height, Move[] pv, Move[] pvMoves)
        {
            var foundPv = false;

            if (depth == 0)
            {
                nodesSearched++;
                return await Quiesce(boardState, alpha, beta);
            }

            if (boardState.IsThreeMoveRepetition())
            {
                nodesSearched++;
                return DrawScore;
            }

            List<Move> moves = generator.GenerateLegalMoves(boardState);
            if (pvMoves.Length > 0)
                moves.MoveToFront(pvMoves[0]);

            if (!moves.Any())
            {
                nodesSearched++;

                if (boardState.IsInCheck(boardState.ToMove))
                    return -MateScore + height;
                else return DrawScore;
            }

            foreach (Move move in moves)
            {
                bool isPvMove = pvMoves.Length > 0 && pvMoves[0].Equals(move);
                Move[] childPvMoves = isPvMove ? pvMoves[1..] : new Move[0];
                var pvBuffer = new Move[depth - 1];

                int eval = DrawScore;
                BoardState newBoard = boardState.MakeMove(move);

                if (foundPv)
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

                if (eval >= beta) return beta;

                if (eval > alpha)
                {
                    alpha = eval;
                    foundPv = true;
                    pvBuffer.CopyTo(pv, 0);
                    pv[depth - 1] = move;
                }
            }

            return alpha;
        }

        private async Task<int> Quiesce(BoardState boardState, int alpha, int beta)
        {
            int eval = evaluator.Evaluate(boardState);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;

            IEnumerable<Move> moves = generator.GenerateLegalMoves(boardState)
                                               .Where(move => move.IsCapture(boardState));

            foreach (Move move in moves)
            {
                BoardState newBoardState = boardState.MakeMove(move); ;

                eval = -await Quiesce(newBoardState, -beta, -alpha);

                if (eval >= beta) return beta;
                if (eval > alpha) alpha = eval;
            }

            return alpha;
        }
    }
}
