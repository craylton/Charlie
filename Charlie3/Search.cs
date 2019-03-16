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
            var isWhite = currentBoard.ToMove == PieceColour.White;

            int depth = 1;
            while (!cancel)
            {
                await AlphaBeta(root, currentBoard, int.MinValue, int.MaxValue, depth++, true);

                var eval = isWhite ? bestNode.Evaluation : -bestNode.Evaluation;
                BestMoveChanged?.Invoke(this, new MoveInfo(bestNode.Depth, new List<Move> { bestNode.Move }, eval));

                if (bestNode.IsMate) cancel = true;
            }

            BestMoveFound?.Invoke(this, bestNode.Move);
        }

        public void Stop()
        {
            timer.Stop();
            cancel = true;
        }

        private async Task AlphaBeta(TreeNode parent, BoardState boardState, int alpha, int beta, int depth, bool isRoot = false)
        {
            if (depth == 0)
            {
                // Evaluate this node
                parent.Evaluation = evaluator.Evaluate(boardState);
                return;
            }

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition())
            {
                parent.Evaluation = 0;
                return;
            }

            bool isWhite = boardState.ToMove == PieceColour.White;

            if (!parent.Children.Any())
            {
                // Generate child nodes if not already there
                var defaultEval = isWhite ? int.MinValue : int.MaxValue;
                parent.Children = generator.GenerateLegalMoves(boardState)
                                           .Select(m => new TreeNode(m, defaultEval)).ToList();
            }
            else
            {
                // Sort the move list in order of best to worst
                if (isWhite)
                    parent.Children = parent.Children.OrderByDescending(c => c.Evaluation).ToList();
                else
                    parent.Children = parent.Children.OrderBy(c => c.Evaluation).ToList();
            }

            // Test for checkmate / stalemate
            if (!parent.Children.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                {
                    parent.Evaluation = isWhite ? int.MinValue : int.MaxValue;
                    parent.IsMate = true;
                    return;
                }
                else
                {
                    parent.Evaluation = 0;
                    return;
                }
            }

            var bestChild = parent.Children.FirstOrDefault();
            if (isRoot) bestNode = bestChild;

            foreach (var node in parent.Children)
            {
                await AlphaBeta(node, boardState.MakeMove(node.Move), alpha, beta, depth - 1);

                // Alpha beta cutoffs
                if (isWhite && node.Evaluation >= beta)
                {
                    parent.Evaluation = beta;
                    parent.Depth = node.Depth + 1;
                    parent.IsMate = node.IsMate;
                    return;
                }
                if (!isWhite && node.Evaluation <= alpha)
                {
                    parent.Evaluation = alpha;
                    parent.Depth = node.Depth + 1;
                    parent.IsMate = node.IsMate;
                    return;
                }

                if (cancel) return;

                // Finding new best moves
                if (isWhite && node.Evaluation > alpha)
                {
                    alpha = node.Evaluation;
                    bestChild = node;
                    if (isRoot) bestNode = node;
                }
                if (!isWhite && node.Evaluation < beta)
                {
                    beta = node.Evaluation;
                    bestChild = node;
                    if (isRoot) bestNode = node;
                }
            }

            parent.Evaluation = isWhite ? alpha : beta;
            parent.Depth = bestChild.Depth + 1;
            parent.IsMate = bestChild.IsMate;
        }
    }
}
