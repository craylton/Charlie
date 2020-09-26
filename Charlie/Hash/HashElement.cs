using Charlie.Moves;

namespace Charlie.Hash
{
    public readonly struct HashElement
    {
        public int Depth { get; }
        public Move Move { get; }
        public HashElement(int depth, Move move) => (Depth, Move) = (depth, move);
    }
}
