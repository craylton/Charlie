using EvalComparisons.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace EvalComparisons.Stockfish
{
    public class BenchAnalysis
    {
        public StockfishType StockfishType { get; }

        public BenchAnalysis(StockfishType stockfishType) =>
            StockfishType = stockfishType;

        public void PerformStaticBench(string outputFilename, AnalysedFenData[] truths)
        {
            var tempFileName = "temptemptemppemtmt.txt";

            string[] fens = truths.Select(fenData => fenData.Fen).ToArray();
            File.WriteAllLines(tempFileName, fens);

            List<int> scores;
            using (StockfishWrapper stockfish = new StockfishWrapper(StockfishType))
            {
                stockfish.Start();
                scores = GetStaticBenchAnalysis(stockfish, tempFileName);
            }

            // score should be relative to side to move
            for (int i = 0; i < fens.Length; i++)
            {
                if (!fens[i].IsWhiteToMove()) scores[i] *= -1;
            }

            File.WriteAllLines(outputFilename, scores.Select(s => s.ToString()));
            File.Delete(tempFileName);
        }

        private List<int> GetStaticBenchAnalysis(StockfishWrapper stockfish, string fullFenFilename)
        {
            var analysis = new Analysis(string.Empty, 0);
            stockfish.EvaluateFensFromFile(analysis, fullFenFilename);

            while (!analysis.IsComplete)
            {
                Thread.Sleep(25);
            }

            return analysis.EvalScores;
        }
    }
}
