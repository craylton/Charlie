using CsvHelper;
using System;
using System.IO;
using System.Linq;

namespace EvalComparisons.Data
{
    public class DataRetriever
    {
        public AnalysedFenData[] Truths { get; private set; }
        public int[] Improvements { get; private set; }

        public bool HasData => Truths.Length > 0 && Improvements.Length > 0;

        public void GetTruthData(string fileLocation)
        {
            using var reader = new StreamReader(fileLocation);
            using var csv = new CsvReader(reader);
            Truths = csv.GetRecords<AnalysedFenData>().ToArray();
        }

        public void GenerateImprovementData(string compareFilename, string toCompareAgainstFilename)
        {
            if (Truths is null) throw new ArgumentNullException(nameof(Truths));

            GenerateImprovementData(Truths, compareFilename, toCompareAgainstFilename);
        }

        public void GenerateImprovementData(AnalysedFenData[] truths, string compareFilename, string toCompareAgainstFilename)
        {
            var file1Scores = compareFilename.ReadIntsFromFile();
            var file2Scores = toCompareAgainstFilename.ReadIntsFromFile();
            Improvements = GetImprovementList(truths, file1Scores, file2Scores);
        }

        private static int[] GetImprovementList(AnalysedFenData[] truths, int[] compareeScores, int[] comparisonScores)
        {
            var improvements = new int[truths.Length];

            for (int i = 0; i < truths.Length; i++)
            {
                var file1Error = Math.Abs(truths[i].AnalysisScore - comparisonScores[i]);
                var file2Error = Math.Abs(truths[i].AnalysisScore - compareeScores[i]);

                improvements[i] = file1Error - file2Error;
            }

            return improvements;
        }
    }
}
