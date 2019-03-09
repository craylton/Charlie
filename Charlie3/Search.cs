using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    public class Search
    {
        public event EventHandler<MoveInfo> MoveInfoChanged;

        private async Task<(Move Move, int Eval)> AlphaBeta(BoardState boardState, int alpha, int beta, int depth, bool isRoot = false)
        {
            if (depth == 0)
            {
                var evaluator = new Evaluator();
                return (default, evaluator.Evaluate(boardState));
            }

            if (boardState.IsThreeMoveRepetition()) return (default, 0);

            var generator = new MoveGenerator();
            var moves = generator.GenerateLegalMoves(boardState);
            var moveInfos = moves.Select(m => MetaMove.FromState(boardState, m))
                                 .OrderByDescending(mi => mi.IsCheck)
                                 .ThenByDescending(mi => mi.IsCapture).ToList();

            Move bestMove = moves.FirstOrDefault();
            bool isWhite = boardState.ToMove == PieceColour.White;

            foreach (var moveInfo in moveInfos)
            {
                var (_, eval) = await AlphaBeta(boardState.MakeMove(moveInfo.Move), alpha, beta, depth - 1);

                if (isWhite && eval >= beta) return (moveInfo.Move, beta);
                if (!isWhite && eval <= alpha) return (moveInfo.Move, alpha);

                if (isWhite && eval > alpha)
                {
                    alpha = eval;
                    bestMove = moveInfo.Move;
                    if (isRoot) MoveInfoChanged?.Invoke(this, new MoveInfo(depth, new List<Move> { moveInfo.Move }, eval));
                }

                if (!isWhite && eval < beta)
                {
                    beta = eval;
                    bestMove = moveInfo.Move;
                    if (isRoot) MoveInfoChanged?.Invoke(this, new MoveInfo(depth, new List<Move> { moveInfo.Move }, -eval));
                }
            }

            return (bestMove, isWhite ? alpha : beta);
        }

        public async Task<Move> FindBestMove(BoardState currentBoard)
        {
            var moveInfo = await AlphaBeta(currentBoard, int.MinValue, int.MaxValue, 5, true);
            return moveInfo.Move;
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
