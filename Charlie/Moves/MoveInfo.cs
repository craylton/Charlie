using System.Collections.Generic;

namespace Charlie.Moves
{
    public readonly struct MoveInfo
    {
        public int Depth { get; }
        public IEnumerable<Move> Moves { get; }
        public Score Evaluation { get; }
        public long Time { get; }
        public ulong Nodes { get; }

        public MoveInfo(int depth, IEnumerable<Move> moves, Score evaluation, long time, ulong nodes) =>
            (Depth, Moves, Evaluation, Time, Nodes) = (depth, moves, evaluation, time, nodes);
    }
}
