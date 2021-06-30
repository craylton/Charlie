using System;

namespace EvalComparisons.Data
{
    public class ImprovementStatistics
    {
        public decimal PercentImproved { get; private set; }
        public decimal ImprovedMaterial { get; private set; }
        public decimal ImprovedScore { get; private set; }

        public decimal PercentWorsened { get; private set; }
        public decimal WorsenedMaterial { get; private set; }
        public decimal WorsenedScore { get; private set; }

        public decimal PercentUnchanged { get; private set; }
        public decimal UnchangedMaterial { get; private set; }
        public decimal UnchangedScore { get; private set; }

        public void GenerateStatistics(AnalysedFenData[] truths, int[] improvements)
        {
            var n = improvements.Length;
            int[] counts = new int[3], materials = new int[3], scores = new int[3];

            for (int i = 0; i < n; i++)
            {
                int index = GetImprovementCategory(improvements[i]);

                counts[index]++;
                materials[index] += truths[i].TotalMaterialOnBoard;
                scores[index] += Math.Abs(truths[i].AnalysisScore);
            }

            PercentImproved = GetProportion(counts[0], n) * 100;
            ImprovedMaterial = GetProportion(materials[0], counts[0]);
            ImprovedScore = GetProportion(scores[0], counts[0]);

            PercentWorsened = GetProportion(counts[1], n) * 100;
            WorsenedMaterial = GetProportion(materials[1], counts[1]);
            WorsenedScore = GetProportion(scores[1], counts[1]);

            PercentUnchanged = GetProportion(counts[2], n) * 100;
            UnchangedMaterial = GetProportion(materials[2], counts[2]);
            UnchangedScore = GetProportion(scores[2], counts[2]);
        }

        /// <summary>
        /// 0 = we are better, 1 = we are worse, 2 = they are the same
        /// </summary>
        private int GetImprovementCategory(int improvement) =>
            (improvement > 0) ? 0 : (improvement < 0) ? 1 : 2;

        private decimal GetProportion(int numerator, int denominator) =>
            denominator == 0 ? 0 : (decimal)numerator / denominator;
    }
}
