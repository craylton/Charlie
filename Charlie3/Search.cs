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
            int eval = DrawScore, depth = 0;

            while (!cancel)
            {
                pv = new Move[++depth];
                eval = await AlphaBeta(currentBoard, NegativeInfinityScore, InfinityScore, depth, pv);

                if (cancel) break;

                prevPv = pv.Reverse().TakeWhile(move => !move.Equals(default(Move))).ToArray();
                bestMove = prevPv[0];

                BestMoveChanged?.Invoke(this, new MoveInfo(depth, prevPv, eval));
            }

            BestMoveFound?.Invoke(this, bestMove);
        }

        public void Stop()
        {
            timer.Stop();
            cancel = true;
        }

        private async Task<int> AlphaBeta(BoardState boardState, int alpha, int beta, int depth, Move[] pv)
        {
            bool foundPv = false;

            if (depth == 0) return evaluator.Evaluate(boardState);
            if (boardState.IsThreeMoveRepetition()) return DrawScore;

            var moves = generator.GenerateLegalMoves(boardState);

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
                    eval = -await AlphaBeta(newBoardState, -alpha - 1, -alpha, depth - 1, localPv);

                    if (eval > alpha && eval < beta)
                        eval = -await AlphaBeta(newBoardState, -beta, -alpha, depth - 1, localPv);
                }
                else
                {
                    eval = -await AlphaBeta(newBoardState, -beta, -alpha, depth - 1, localPv);
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






        //private async Task<(int eval, bool legal)> Quiesce(TreeNode parent, BoardState boardState, int alpha, int beta, int depth)
        //{
        //    bool isWhite = boardState.ToMove == PieceColour.White;

        //    // Test if our opponent made a legal move to get here
        //    if (boardState.IsInCheck(isWhite ? PieceColour.Black : PieceColour.White))
        //    {
        //        parent.Evaluation = int.MaxValue;
        //        return (parent.Evaluation, false);
        //    }

        //    var standPat = evaluator.Evaluate(boardState);

        //    if (depth == 0)
        //    {
        //        // Evaluate this node
        //        parent.Evaluation = standPat;
        //        return (parent.Evaluation, true);
        //    }

        //    // Alpha beta cutoff
        //    if (standPat >= beta)
        //    {
        //        parent.Evaluation = standPat;
        //        return (parent.Evaluation, true);
        //    }

        //    if (standPat > alpha) alpha = standPat;

        //    // Test for 3-move repetition
        //    if (boardState.IsThreeMoveRepetition())
        //    {
        //        parent.Evaluation = 0;
        //        return (parent.Evaluation, true);
        //    }

        //    if (!parent.Children.Any())
        //    {
        //        // Generate child nodes if not already there
        //        parent.Children = generator.GeneratePseudoLegalMoves(boardState)
        //                                   .Where(move => move.IsCapture(boardState))
        //                                   .Select(m => new TreeNode(m, int.MinValue))
        //                                   .ToList();
        //    }
        //    else
        //    {
        //        // Sort the move list in order of best to worst
        //        parent.Children = parent.Children.OrderByDescending(c => c.Evaluation).ToList();
        //    }

        //    var bestChild = parent.Children.FirstOrDefault();

        //    int i = 0;
        //    while (i < parent.Children.Count)
        //    {
        //        // We are only interested in captures here
        //        if (!parent.Children[i].Move.IsCapture(boardState)) continue;

        //        var (_, legal) = await Quiesce(parent.Children[i], boardState.MakeMove(parent.Children[i].Move), alpha, beta, depth - 1);

        //        // Check if this was a legal move
        //        if (!legal)
        //        {
        //            parent.Children.RemoveAt(i);
        //            continue;
        //        }

        //        if (parent.Children[i].Evaluation >= beta)
        //        {
        //            parent.Evaluation = parent.Children[i].Evaluation;
        //            return (parent.Evaluation, true);
        //        }

        //        if (parent.Children[i].Evaluation > alpha)
        //        {
        //            alpha = parent.Children[i].Evaluation;
        //        }

        //        if (cancel) return (parent.Evaluation, true);

        //        i++;
        //    }

        //    // Test for checkmate / stalemate
        //    if (!parent.Children.Any())
        //    {
        //        if (boardState.IsInCheck(boardState.ToMove))
        //        {
        //            parent.Evaluation = int.MinValue;
        //        }
        //        else
        //        {
        //            parent.Evaluation = 0;
        //        }
        //    }
        //    else
        //    {
        //        parent.Evaluation = alpha;
        //    }

        //    return (parent.Evaluation, true);
        //}
    }
}
