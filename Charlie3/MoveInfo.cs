using System.Collections.Generic;

namespace Charlie3
{
    public readonly struct MoveInfo
    {
        public int Depth { get; }
        public List<Move> Moves { get; }
        public int Evaluation { get; }

        public MoveInfo(int depth, List<Move> moves, int evaluation) : this() => 
            (Depth, Moves, Evaluation) = (depth, moves, evaluation);
    }
}
