using System.Collections.Generic;

namespace EvalComparisons.Stockfish
{
    public class Analysis
    {
        public string Fen { get; }

        public int Depth { get; }

        public SearchType SearchType { get; set; }

        public int CurrentScore { get; set; }

        public List<int> EvalScores { get; set; }

        public string BestMove { get; set; }

        public bool IsComplete { get; set; }

        public bool IsMate { get; set; }

        public Analysis(string fen, int depth) =>
            (Fen, Depth) = (fen, depth);
    }
}
