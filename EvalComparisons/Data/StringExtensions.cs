using System.IO;
using System.Linq;

namespace EvalComparisons.Data
{
    public static class StringExtensions
    {
        public static bool IsWhiteToMove(this string fen) => fen.Split(" ")[1] == "w";

        public static int[] ReadIntsFromFile(this string filename) =>
            File.ReadAllLines(filename).Select(line => int.Parse(line)).ToArray();
    }
}
