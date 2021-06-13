namespace Charlie.Moves
{
    public record EvaluatedMove
    {
        private readonly int basePromise;
        private int dynamicPromise = 0;

        public Move Move { get; }
        public Score Score { get; set; } = Score.Draw;
        public int Promise => dynamicPromise + basePromise;

        public EvaluatedMove(Move move, int basePromise) =>
            (Move, this.basePromise) = (move, basePromise);

        public void IncreasePromise(int amount) => dynamicPromise += amount;
    }
}
