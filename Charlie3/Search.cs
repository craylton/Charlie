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

        public event EventHandler<MoveInfo> BestMoveChanged;
        public event EventHandler<Move> BestMoveFound;

        private async Task<TreeNode> AlphaBeta(TreeNode parent, BoardState boardState, int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                // Evaluate this node
                var evaluator = new Evaluator();
                parent.Evaluation = evaluator.Evaluate(boardState);
                return new TreeNode(default, parent.Evaluation);
            }

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition())
            {
                parent.Evaluation = 0;
                return new TreeNode(default, parent.Evaluation);
            }

            bool isWhite = boardState.ToMove == PieceColour.White;

            if (depth == 1 || !parent.Children.Any())
            {
                // Generate child nodes if not already there
                var generator = new MoveGenerator();
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
                    return new TreeNode(default, parent.Evaluation);
                }
                else
                {
                    parent.Evaluation = 0;
                    return new TreeNode(default, parent.Evaluation);
                }
            }

            Move bestMove = parent.Children.FirstOrDefault().Move;

            foreach (var node in parent.Children)
            {
                (_, _, int eval) = await AlphaBeta(node, boardState.MakeMove(node.Move), alpha, beta, depth - 1);

                if (cancel) break;

                // Alpha beta cutoffs
                if (isWhite && eval >= beta)
                {
                    parent.Evaluation = beta;
                    return new TreeNode(node.Move, beta);
                }
                if (!isWhite && eval <= alpha)
                {
                    parent.Evaluation = alpha;
                    return new TreeNode(node.Move, alpha);
                }

                // Finding new best moves
                if (isWhite && eval > alpha)
                {
                    alpha = eval;
                    bestMove = node.Move;
                }
                if (!isWhite && eval < beta)
                {
                    beta = eval;
                    bestMove = node.Move;
                }
            }

            parent.Evaluation = isWhite ? alpha : beta;
            return new TreeNode(bestMove, parent.Evaluation);
        }

        public Search()
        {
            timer = new Timer() { AutoReset = false };
            timer.Elapsed += Timer_Elapsed;
        }

        public async Task Start(BoardState currentBoard, int timeMs)
        {
            cancel = false;

            timer.Interval = timeMs;
            timer.Start();

            TreeNode root = new TreeNode(default, 0);
            TreeNode bestNode = new TreeNode(default, default);
            var isWhite = currentBoard.ToMove == PieceColour.White;

            int i = 1;
            while (!cancel)
            {
                bestNode = await AlphaBeta(root, currentBoard, int.MinValue, int.MaxValue, i++);

                var eval = isWhite ? bestNode.Evaluation : -bestNode.Evaluation;
                BestMoveChanged?.Invoke(this, new MoveInfo(i, new List<Move> { bestNode.Move }, eval));
            }

            BestMoveFound?.Invoke(this, bestNode.Move);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) => 
            cancel = true;
    }
}
