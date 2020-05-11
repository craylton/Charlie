using System.Collections.Generic;

namespace Charlie3
{
    public readonly struct MoveInfo
    {
        public int Depth { get; }
        public IEnumerable<Move> Moves { get; }
        public int Evaluation { get; }
        public bool IsMate { get; }

        public MoveInfo(int depth, IEnumerable<Move> moves, int evaluation) :
            this(depth, moves, evaluation, false)
        { }

        public MoveInfo(int depth, IEnumerable<Move> moves, int evaluation, bool isMate) : this() =>
            (Depth, Moves, Evaluation, IsMate) = (depth, moves, evaluation, isMate);
    }
}
