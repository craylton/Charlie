using Charlie3.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Charlie3
{
    public class Search
    {
        private readonly Timer timer;
        private bool cancel;
        private TreeNode bestNode = new TreeNode(default, default);
        private readonly Evaluator evaluator = new Evaluator();
        private readonly MoveGenerator generator = new MoveGenerator();

        public event EventHandler<MoveInfo> BestMoveChanged;
        public event EventHandler<Move> BestMoveFound;

        public Search()
        {
            timer = new Timer() { AutoReset = false };
            timer.Elapsed += (s, e) => cancel = true;
        }

        public async Task Start(BoardState currentBoard, int timeMs)
        {
            cancel = false;

            // Negative time means infinite search
            if (timeMs >= 0)
            {
                timer.Interval = timeMs;
                timer.Start();
            }

            var root = new TreeNode(default, default);

            Move bestMove = default;
            Move[] pv, prevPv;
            int eval = 0, depth = 0;

            while (!cancel)
            {
                depth++;
                pv = new Move[depth];

                //var (pv, _) = await AlphaBeta(root, currentBoard, int.MinValue, int.MaxValue, depth, true);
                eval = await AlphaBeta2(currentBoard, int.MinValue + 10, int.MaxValue - 10, depth, pv);

                if (cancel) break;

                prevPv = pv.Reverse().ToArray();
                bestMove = prevPv[0];
                var bestMoveInfo = new MoveInfo(depth, prevPv.ToList(), eval);
                BestMoveChanged?.Invoke(this, bestMoveInfo);


                //if (pv.Any())
                //{
                //    //var eval = bestNode.Evaluation;
                //    BestMoveChanged?.Invoke(this, new MoveInfo(bestNode.Depth, pv, eval, bestNode.IsMate));
                //}

                //if (bestNode.IsMate)
                //{
                //    cancel = true;
                //    BestMoveChanged?.Invoke(this, new MoveInfo(bestNode.Depth, pv, 0, true));
                //}
            }

            BestMoveFound?.Invoke(this, bestMove);
        }

        public void Stop()
        {
            timer.Stop();
            cancel = true;
        }

        private async Task<int> AlphaBeta2(BoardState boardState, int alpha, int beta, int depth, Move[] pv)
        {
            bool foundPv = false;

            if (depth == 0) return evaluator.Evaluate(boardState);
            if (boardState.IsThreeMoveRepetition()) return 0;

            var moves = generator.GenerateLegalMoves(boardState);

            if (!moves.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                    return int.MinValue + 10;
                else return 0;
            }

            foreach (var move in moves)
            {
                Move[] localPv = new Move[depth-1];

                int eval = 0;

                if (foundPv)
                {
                    eval = -await AlphaBeta2(boardState.MakeMove(move), -alpha - 1, -alpha, depth - 1, localPv);
                    if (eval > alpha && eval < beta)
                        eval = -await AlphaBeta2(boardState.MakeMove(move), -beta, -alpha, depth - 1, localPv);
                }
                else
                {
                    eval = -await AlphaBeta2(boardState.MakeMove(move), -beta, -alpha, depth - 1, localPv);
                }

                if (eval >= beta)
                {
                    return beta;
                }
                if (eval > alpha)
                {
                    alpha = eval;
                    foundPv = true;
                    localPv.CopyTo(pv, 0);
                    pv[depth - 1] = move;
                }

                if (cancel) break;
            }

            return alpha;
        }







        private async Task<(List<Move> PV, bool IsLegal)> AlphaBeta(
            TreeNode parent,
            BoardState boardState,
            int alpha,
            int beta,
            int depth,
            bool isRoot = false)
        {
            bool isWhite = boardState.ToMove == PieceColour.White;

            // Test if our opponent made a legal move to get here
            if (boardState.IsInCheck(isWhite ? PieceColour.Black : PieceColour.White))
            {
                parent.Evaluation = int.MaxValue;
                return (new List<Move>(), false);
            }

            if (depth == 0)
            {
                // Evaluate this node
                parent.Evaluation = evaluator.Evaluate(boardState);
                //(parent.Evaluation, _) = await Quiesce(parent, boardState.MakeMove(parent.Move), alpha, beta, 3);
                return (new List<Move>(), true);
            }

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition())
            {
                parent.Evaluation = 0;
                return (new List<Move>(), true);
            }

            if (!parent.Children.Any())
            {
                // Generate child nodes if not already there
                parent.Children = generator.GeneratePseudoLegalMoves(boardState)
                                           .Select(m => new TreeNode(m, int.MinValue))
                                           .ToList();
            }
            else
            {
                // Sort the move list in order of best to worst
                parent.Children = parent.Children.OrderByDescending(c => c.Evaluation).ToList();
            }

            var bestChild = parent.Children.FirstOrDefault();
            var bestPv = new List<Move>();
            if (isRoot) bestNode = bestChild;

            int i = 0;
            while (i < parent.Children.Count)
            {
                var (pv, legal) = await AlphaBeta(parent.Children[i], boardState.MakeMove(parent.Children[i].Move), alpha, beta, depth - 1);

                // Check if this was a legal move
                if (!legal)
                {
                    parent.Children.RemoveAt(i);
                    continue;
                }

                // Alpha beta cutoffs
                if (parent.Children[i].Evaluation >= beta)
                {
                    parent.Evaluation = beta;
                    parent.Depth = parent.Children[i].Depth + 1;
                    parent.IsMate = parent.Children[i].IsMate;
                    return (bestPv, true);
                }

                if (cancel) return (bestPv, true);

                // Finding new best moves
                if (parent.Children[i].Evaluation > alpha)
                {
                    alpha = parent.Children[i].Evaluation;
                    bestChild = parent.Children[i];
                    bestPv = pv;
                    bestPv.Insert(0, parent.Children[i].Move);
                    if (isRoot) bestNode = parent.Children[i];
                }

                i++;
            }

            // Test for checkmate / stalemate
            if (!parent.Children.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                {
                    parent.Evaluation = int.MinValue;
                    parent.IsMate = true;
                }
                else
                {
                    parent.Evaluation = 0;
                }
            }
            else
            {
                parent.Evaluation = alpha;
                parent.Depth = bestChild.Depth + 1;
                parent.IsMate = bestChild.IsMate;
            }

            return (bestPv, true);
        }





        private async Task<(int eval, bool legal)> Quiesce(TreeNode parent, BoardState boardState, int alpha, int beta, int depth)
        {
            bool isWhite = boardState.ToMove == PieceColour.White;

            // Test if our opponent made a legal move to get here
            if (boardState.IsInCheck(isWhite ? PieceColour.Black : PieceColour.White))
            {
                parent.Evaluation = int.MaxValue;
                return (parent.Evaluation, false);
            }

            var standPat = evaluator.Evaluate(boardState);

            if (depth == 0)
            {
                // Evaluate this node
                parent.Evaluation = standPat;
                return (parent.Evaluation, true);
            }

            // Alpha beta cutoff
            if (standPat >= beta)
            {
                parent.Evaluation = standPat;
                return (parent.Evaluation, true);
            }

            if (standPat > alpha) alpha = standPat;

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition())
            {
                parent.Evaluation = 0;
                return (parent.Evaluation, true);
            }

            if (!parent.Children.Any())
            {
                // Generate child nodes if not already there
                parent.Children = generator.GeneratePseudoLegalMoves(boardState)
                                           .Where(move => move.IsCapture(boardState))
                                           .Select(m => new TreeNode(m, int.MinValue))
                                           .ToList();
            }
            else
            {
                // Sort the move list in order of best to worst
                parent.Children = parent.Children.OrderByDescending(c => c.Evaluation).ToList();
            }

            var bestChild = parent.Children.FirstOrDefault();

            int i = 0;
            while (i < parent.Children.Count)
            {
                // We are only interested in captures here
                if (!parent.Children[i].Move.IsCapture(boardState)) continue;

                var (_, legal) = await Quiesce(parent.Children[i], boardState.MakeMove(parent.Children[i].Move), alpha, beta, depth - 1);

                // Check if this was a legal move
                if (!legal)
                {
                    parent.Children.RemoveAt(i);
                    continue;
                }

                if (parent.Children[i].Evaluation >= beta)
                {
                    parent.Evaluation = parent.Children[i].Evaluation;
                    return (parent.Evaluation, true);
                }

                if (parent.Children[i].Evaluation > alpha)
                {
                    alpha = parent.Children[i].Evaluation;
                }

                if (cancel) return (parent.Evaluation, true);

                i++;
            }

            // Test for checkmate / stalemate
            if (!parent.Children.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                {
                    parent.Evaluation = int.MinValue;
                }
                else
                {
                    parent.Evaluation = 0;
                }
            }
            else
            {
                parent.Evaluation = alpha;
            }

            return (parent.Evaluation, true);
        }
    }
}
