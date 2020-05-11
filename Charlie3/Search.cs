using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Charlie3
{
    public class Search
    {
        private bool cancel;
        private readonly Timer timer = new Timer() { AutoReset = false };

        private const int InfinityScore = 1 << 24;
        private const int NegativeInfinityScore = -InfinityScore;
        private const int MateScore = 1 << 20;
        private const int DrawScore = 0;

        private readonly Evaluator evaluator = new Evaluator();
        private readonly MoveGenerator generator = new MoveGenerator();

        public event EventHandler<MoveInfo> BestMoveChanged;
        public event EventHandler<Move> BestMoveFound;

        public Search() => timer.Elapsed += (s, e) => cancel = true;

        public async Task Start(BoardState currentBoard, int timeMs)
        {
            cancel = false;

            // Negative time means infinite search
            if (timeMs >= 0)
            {
                timer.Interval = timeMs;
                timer.Start();
            }

            Move bestMove = default;
            Move[] pv, prevPv;
            int eval = DrawScore, depth = 1;
            int alpha = NegativeInfinityScore, beta = InfinityScore;

            while (!cancel)
            {
                pv = new Move[depth];
                eval = await AlphaBeta(currentBoard, alpha, beta, depth, pv, bestMove);

                if (cancel) break;

                if (eval <= alpha)
                {
                    alpha = NegativeInfinityScore;
                    continue;
                }
                else if (eval >= beta)
                {
                    beta = InfinityScore;
                    continue;
                }

                prevPv = pv.Reverse().TakeWhile(move => !move.Equals(default(Move))).ToArray();
                bestMove = prevPv[0];

                BestMoveChanged?.Invoke(this, new MoveInfo(depth, prevPv, eval));

                alpha = eval - 100;
                beta = eval + 100;
                depth++;
            }

            BestMoveFound?.Invoke(this, bestMove);
        }

        public void Stop()
        {
            timer.Stop();
            cancel = true;
        }

        private async Task<int> AlphaBeta(BoardState boardState, int alpha, int beta, int depth, Move[] pv, Move pvMove)
        {
            bool foundPv = false;

            if (depth == 0) return await Quiesce(boardState, alpha, beta);
            if (boardState.IsThreeMoveRepetition()) return DrawScore;

            var moves = generator.GenerateLegalMoves(boardState);
            moves.MoveToFront(pvMove);

            if (!moves.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                    return -MateScore;
                else return DrawScore;
            }

            foreach (var move in moves)
            {
                Move[] localPv = new Move[depth - 1];
                int eval = DrawScore;
                var newBoardState = boardState.MakeMove(move);

                if (foundPv)
                {
                    eval = -await AlphaBeta(newBoardState, -alpha - 1, -alpha, depth - 1, localPv, default);

                    if (eval > alpha && eval < beta)
                        eval = -await AlphaBeta(newBoardState, -beta, -alpha, depth - 1, localPv, default);
                }
                else
                {
                    eval = -await AlphaBeta(newBoardState, -beta, -alpha, depth - 1, localPv, default);
                }

                if (cancel) break;

                if (eval >= beta)
                {
                    localPv.CopyTo(pv, 0);
                    pv[depth - 1] = move;
                    return beta;
                }

                if (eval > alpha)
                {
                    alpha = eval;
                    foundPv = true;
                    localPv.CopyTo(pv, 0);
                    pv[depth - 1] = move;
                }
            }

            return alpha;
        }

        private async Task<int> Quiesce(BoardState boardState, int alpha, int beta)
        {
            var eval = evaluator.Evaluate(boardState);

            if (eval >= beta) return beta;
            if (eval > alpha) alpha = eval;

            var moves = generator.GenerateLegalMoves(boardState)
                                 .Where(move => move.IsCapture(boardState));

            foreach (var move in moves)
            {
                var newBoardState = boardState.MakeMove(move); ;

                eval = -await Quiesce(newBoardState, -beta, -alpha);

                if (eval >= beta) return beta;
                if (eval > alpha) alpha = eval;
            }

            return alpha;
        }
    }
}
