using System.Collections.Generic;

namespace Charlie3
{
    public class TreeNode2
    {
        public List<TreeNode2> Children { get; set; }

        public Move Move { get; }

        public int Evaluation { get; set; }

        public TreeNode2(Move move, int evaluation)
        {
            (Move, Evaluation) = (move, evaluation);
            Children = new List<TreeNode2>();
        }
    }
}
