using Charlie.Moves;
using System;
using System.Runtime.CompilerServices;

namespace Charlie.Hash;

public class HashTable
{
    private const int DefaultHashTableSize = 1 << 20;
    private const int BytesPerMegabyte = 1024 * 1024;
    private static readonly int HashSlotSize = Unsafe.SizeOf<HashSlot>();

    public const int MinimumSizeInMegabytes = 1;
    public const int MaximumSizeInMegabytes = 2048;
    public static readonly int DefaultSizeInMegabytes = DefaultHashTableSize * HashSlotSize / BytesPerMegabyte;

    private HashSlot[] hashTable = CreateTable(DefaultSizeInMegabytes);

    public int SizeInMegabytes { get; private set; } = DefaultSizeInMegabytes;

    public void Clear() => Array.Clear(hashTable);

    public void Resize(int sizeInMegabytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sizeInMegabytes, MinimumSizeInMegabytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sizeInMegabytes, MaximumSizeInMegabytes);

        hashTable = CreateTable(sizeInMegabytes);
        SizeInMegabytes = sizeInMegabytes;
    }

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

    private static HashSlot[] CreateTable(int sizeInMegabytes)
    {
        long targetBytes = (long)sizeInMegabytes * BytesPerMegabyte;
        int entryCount = Math.Max(1, (int)(targetBytes / HashSlotSize));
        return new HashSlot[entryCount];
    }

    private int GetIndex(long hashKey) => (int)((ulong)hashKey % (ulong)hashTable.Length);

    private struct HashSlot
    {
        public long HashKey;
        public HashElement Entry;
        public bool IsOccupied;
    }
}
