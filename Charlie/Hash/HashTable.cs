using Charlie.Moves;
using System.Collections.Generic;

namespace Charlie.Hash;

public class HashTable
{
    private readonly Dictionary<long, HashElement> hashTable = [];

    public void Clear() => hashTable.Clear();

    public Move ProbeHash(long hash)
    {
        if (!hashTable.TryGetValue(hash, out HashElement value)) return default;
        return value.Move;
    }

    public void RecordHash(long hashKey, int depth, Move move)
    {
        if ((!hashTable.ContainsKey(hashKey) || hashTable[hashKey].Depth < depth) && move.IsValidMove())
            hashTable[hashKey] = new HashElement(depth, move);
    }
}
