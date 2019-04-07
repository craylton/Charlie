using System.Collections.Generic;

namespace Charlie3
{
    public class TreeNode
    {
        public List<TreeNode> Children { get; set; }
        public Move Move { get; }
        public int Evaluation { get; set; }
        public int Depth { get; set; }
        public bool IsMate { get; set; }

        public TreeNode(Move move, int evaluation) : this(move, evaluation, false) { }

        public TreeNode(Move move, int evaluation, bool isMate)
        {
            (Move, Evaluation, IsMate) = (move, evaluation, isMate);
            Depth = 1;
            Children = new List<TreeNode>();
        }
        public override string ToString()
        {
            string score = Evaluation.ToString();
            if (IsMate) score = (Evaluation < 0 ? "-M" : "M") + (Depth + 1) / 2;

            return $"{Move.ToString()} ({score})";
        }
    }
}
