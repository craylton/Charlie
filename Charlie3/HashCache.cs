using System.Collections.Generic;
using System.Linq;

namespace Charlie3
{
    public static class HashCache
    {
        private const int MaxEntries = 10;

        private static List<(int Hash, int Eval)> table = new List<(int, int)>();

        public static bool Exists(int hash)
        {
            for (int i = 0; i< table.Count; i++)
            {
                if (table[i].Hash == hash) return true;
            }

            return false;
        }

        public static int Get(int hash)
        {
            // Grab the eval
            var eval = table.FirstOrDefault(t => t.Hash == hash).Eval;

            // Move this entry to the front of the list
            table.RemoveAll(t => t.Hash == hash);
            table.Add((hash, eval));

            return eval;
        }

        public static void Store(int hash, int eval)
        {
            // Delete the one accessed longest ago
            if (table.Count >= MaxEntries)
                table.RemoveAt(0);

            table.Add((hash, eval));
        }
    }
}
