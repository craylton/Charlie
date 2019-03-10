using System.Collections.Generic;

namespace Charlie3
{
    public class TreeNode
    {
        public List<TreeNode> Children { get; set; }
        public Move Move { get; }
        public int Evaluation { get; set; }

        public TreeNode(Move move, int evaluation)
        {
            (Move, Evaluation) = (move, evaluation);
            Children = new List<TreeNode>();
        }

        public void Deconstruct(out List<TreeNode> children, out Move move, out int evaluation) =>
            (children, move, evaluation) = (Children, Move, Evaluation);
    }
}
