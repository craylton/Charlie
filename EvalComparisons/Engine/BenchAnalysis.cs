using Charlie.BoardRepresentation;
using Charlie.Hash;
using EvalComparisons.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EvalComparisons.Stockfish
{
    public class BenchAnalysis
    {
        public static void PerformStaticBench(string outputFilename, AnalysedFenData[] truths)
        {
            var tempFileName = "temptemptemppemtmt.txt";

            string[] fens = truths.Select(fenData => fenData.Fen).ToArray();
            File.WriteAllLines(tempFileName, fens);

            var scores = new List<int>();
            Zobrist.Initialise();
            Magics.Initialise();
            var evaluator = new Evaluator();

            foreach (var fen in fens)
            {
                var boardState = new BoardState(fen.Split(' '));
                var score = evaluator.Evaluate(boardState);
                scores.Add((int)score);
            }

            // score should be relative to side to move
            for (int i = 0; i < fens.Length; i++)
            {
                if (!fens[i].IsWhiteToMove()) scores[i] *= -1;
            }

            File.WriteAllLines(outputFilename, scores.Select(s => s.ToString()));
            File.Delete(tempFileName);
        }
    }
}
