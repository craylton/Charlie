using Charlie.Hash;

namespace Charlie.Search
{
    public readonly struct HashElement
    {
        public int Depth { get; }
        public HashType Type { get; }
        public int Evaluation { get; }

        public HashElement(int depth, HashType type, int evaluation) =>
            (Depth, Type, Evaluation) = (depth, type, evaluation);
    }
}
