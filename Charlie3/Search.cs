using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    public class Search
    {
        public event EventHandler<MoveInfo> BestMoveChanged;
        public event EventHandler<Move> BestMoveFound;

        private async Task<(Move Move, int Eval)> AlphaBeta(TreeNode2 parent, BoardState boardState, int alpha, int beta, int depth, bool isRoot = false)
        {
            if (depth == 0)
            {
                // Evaluate this node
                var evaluator = new Evaluator();
                parent.Evaluation = evaluator.Evaluate(boardState);
                return (default, parent.Evaluation);
            }

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition()) return (default, 0);

            bool isWhite = boardState.ToMove == PieceColour.White;

            if (depth == 1 || !parent.Children.Any())
            {
                // Generate child nodes if not already there
                var generator = new MoveGenerator();
                var defaultEval = isWhite ? int.MinValue : int.MaxValue;
                parent.Children = generator.GenerateLegalMoves(boardState)
                                          .Select(m => new TreeNode2(m, defaultEval)).ToList();
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
                    return (default, isWhite ? int.MinValue : int.MaxValue);

                else return (default, 0);
            }

            Move bestMove = parent.Children.Select(c => c.Move).FirstOrDefault();

            foreach (var node in parent.Children)
            {
                var (_, eval) = await AlphaBeta(node, boardState.MakeMove(node.Move), alpha, beta, depth - 1);

                // Alpha beta cutoffs
                if (isWhite && eval >= beta)
                {
                    parent.Evaluation = beta;
                    return (node.Move, beta);
                }
                if (!isWhite && eval <= alpha)
                {
                    parent.Evaluation = alpha;
                    return (node.Move, alpha);
                }

                // Finding new best moves
                if (isWhite && eval > alpha)
                {
                    alpha = eval;
                    bestMove = node.Move;
                    if (isRoot) BestMoveChanged?.Invoke(this, new MoveInfo(depth, new List<Move> { node.Move }, eval));
                }
                if (!isWhite && eval < beta)
                {
                    beta = eval;
                    bestMove = node.Move;
                    if (isRoot) BestMoveChanged?.Invoke(this, new MoveInfo(depth, new List<Move> { node.Move }, -eval));
                }
            }

            parent.Evaluation = isWhite ? alpha : beta;
            return (bestMove, parent.Evaluation);
        }

        public async Task Start(BoardState currentBoard)
        {
            TreeNode2 root = new TreeNode2(default, 0);
            Move bestMove = default;

            for (int i = 1; i < 6; i++)
            {
                (bestMove, _) = await AlphaBeta(root, currentBoard, int.MinValue, int.MaxValue, i, true);
            }

            BestMoveFound?.Invoke(this, bestMove);
        }

        public async Task<(Move Move, int Eval)> GetTreeSearchMove(BoardState board)
        {
            TreeNode root = new TreeNode(board, default);

            int eval = 0;
            while (root.Visits < 10)
            {
                eval = await TreeSearch(root);
            }

            var strongestMove = root.GetStrongestChild()?.Move ?? default;
            return (strongestMove, eval);
        }

        private async Task<int> TreeSearch(TreeNode parent)
        {
            // Make sure this node has children
            if (!parent.HasChildren)
            {
                var generator = new MoveGenerator();
                var evaluator = new Evaluator();

                var moves = generator.GenerateLegalMoves(parent.Board);
                parent.Children = moves.Select(m => new TreeNode(parent.Board.MakeMove(m), m)).ToList();

                foreach (var child in parent.Children)
                {
                    var evaluation = evaluator.Evaluate(child.Board);
                    child.Evaluation = evaluation;
                    child.Visits++;
                }

                parent.Visits++;
                parent.UpdateEvaluation();
                return parent.Evaluation;
            }

            TreeNode bestNode = parent.Children.First();
            double bestScore = double.MinValue;

            // Pick the most promising looking child
            foreach (var node in parent.Children)
            {
                if (node.SearchScore > bestScore)
                {
                    bestScore = node.SearchScore;
                    bestNode = node;
                }
            }

            var eval = await TreeSearch(bestNode);
            parent.Visits++;
            parent.UpdateEvaluation();
            return parent.Evaluation;
        }
    }
}
