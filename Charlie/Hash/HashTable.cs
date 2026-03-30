using Charlie.Moves;
using System;

namespace Charlie.Hash;

public class HashTable
{
    private const int HashTableSize = 1 << 19;
    private const ulong HashTableMask = HashTableSize - 1;

    private readonly HashSlot[] hashTable = new HashSlot[HashTableSize];

    public void Clear() => Array.Clear(hashTable);

    public bool TryProbeHash(long hash, out HashElement value)
    {
        ref HashSlot slot = ref hashTable[GetIndex(hash)];

        if (!slot.IsOccupied || slot.HashKey != hash)
        {
            value = default;
            return false;
        }

        value = slot.Entry;
        return true;
    }

    public void RecordHash(long hashKey, int depth, Score score, Move move, HashType type)
    {
        ref HashSlot slot = ref hashTable[GetIndex(hashKey)];

        if (!slot.IsOccupied
            || slot.HashKey != hashKey
            || slot.Entry.Depth < depth
            || slot.Entry.Depth == depth && type == HashType.Exact && slot.Entry.Type != HashType.Exact)
        {
            slot = new HashSlot
            {
                HashKey = hashKey,
                Entry = new HashElement(depth, score, move, type),
                IsOccupied = true,
            };
        }
    }

    private static int GetIndex(long hashKey) => (int)((ulong)hashKey & HashTableMask);

    private struct HashSlot
    {
        public long HashKey;
        public HashElement Entry;
        public bool IsOccupied;
    }
}
