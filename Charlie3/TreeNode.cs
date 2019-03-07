using System.Collections.Generic;
using System.Linq;

namespace Charlie3
{
    public class TreeNode
    {
        private int _leafEvaluation;
        private int _computedEvaluation;

        public List<TreeNode> Children { get; set; } = new List<TreeNode>();

        public BoardState Board { get; }

        public Move Move { get; }

        public int Evaluation
        {
            get => Visits == 1 ? _leafEvaluation : _computedEvaluation;
            set => _leafEvaluation = value;
        }

        public int Visits { get; set; }

        public double SearchScore
        {
            get
            {
                var isWhite = Board.ToMove == PieceColour.White;
                var eval = (double)Evaluation * Evaluation / Visits;
                return isWhite ? -eval : eval;
            }
        }

        public bool HasChildren => Children.Any();

        public TreeNode(BoardState board, Move move)
        {
            (Board, Move) = (board, move);
            var defaultScore = board.ToMove == PieceColour.White ? int.MinValue : int.MaxValue;
            _leafEvaluation = _computedEvaluation = defaultScore;
        }

        public void UpdateEvaluation()
        {
            var strongestChild = GetStrongestChild();

            // If I have no moves, assume my opponent is mated
            if (strongestChild is null)
            {
                _computedEvaluation = Board.ToMove == PieceColour.White ? int.MinValue: int.MaxValue;
                return;
            }

            _computedEvaluation = strongestChild.Evaluation;
        }

        public TreeNode GetStrongestChild()
        {
            int bestEval = Board.ToMove == PieceColour.White ? int.MinValue : int.MaxValue;
            TreeNode bestNode = Children.FirstOrDefault();

            foreach (var node in Children)
            {
                if ((Board.ToMove == PieceColour.White && node.Evaluation > bestEval) ||
                    (Board.ToMove == PieceColour.Black && node.Evaluation < bestEval))
                {
                    bestEval = node.Evaluation;
                    bestNode = node;
                }
            }

            return bestNode;
        }
    }
}
