using Charlie.Moves;

namespace Charlie.Hash;

public enum HashType
{
    Exact,
    Lower,
    Upper,
}

public readonly record struct HashElement(int Depth, Score Score, Move Move, HashType Type);
