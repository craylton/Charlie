using System;

namespace EvalComparisons.Filters
{
    public class Filter
    {
        public FilterSourceType FilterType { get; set; }
        public FilterComparisonType Comparison { get; set; }
        public int Value { get; set; }

        public bool DoesPass(int truthScore, int improvementScore, int material) =>
            Compare(GetAppliedValue(truthScore, improvementScore, material), Value);

        public int GetAppliedValue(int truthScore, int improvementScore, int material) => FilterType switch
        {
            FilterSourceType.Improvement => improvementScore,
            FilterSourceType.Material => material,
            FilterSourceType.Score => truthScore,
            _ => throw new Exception("Invalid filter type"),
        };

        private bool Compare(int value1, int value2) => Comparison switch
        {
            FilterComparisonType.LessThan => value1 < value2,
            FilterComparisonType.LessThanOrEqual => value1 <= value2,
            FilterComparisonType.Equal => value1 == value2,
            FilterComparisonType.GreaterThanOrEqual => value1 >= value2,
            FilterComparisonType.GreaterThan => value1 > value2,
            FilterComparisonType.NotEqual => value1 != value2,
            _ => throw new Exception("Invalid filter comparison type"),
        };
    }
}
