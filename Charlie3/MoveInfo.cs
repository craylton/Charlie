using System.Collections.Generic;

namespace Charlie3
{
    public readonly struct MoveInfo
    {
        public int Depth { get; }
        public IEnumerable<Move> Moves { get; }
        public int Evaluation { get; }
        public bool IsMate { get; }
        public long Time { get; }
        public ulong Nodes { get; }

        public MoveInfo(int depth, IEnumerable<Move> moves, int evaluation, bool isMate, long time, ulong nodes) =>
            (Depth, Moves, Evaluation, IsMate, Time, Nodes) = (depth, moves, evaluation, isMate, time, nodes);
    }
}
