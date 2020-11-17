using System.Collections.Generic;

namespace Charlie.Moves
{
    public record MoveInfo(int Depth, IEnumerable<Move> Moves, Score Evaluation, long Time, ulong Nodes);
}
