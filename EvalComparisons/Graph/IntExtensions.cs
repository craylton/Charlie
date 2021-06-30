namespace EvalComparisons.Graph
{
    public static class IntExtensions
    {
        public static bool IsWithin(this int input, int lowerBound, int upperBound) =>
            input > lowerBound && input <= upperBound;
    }
}
