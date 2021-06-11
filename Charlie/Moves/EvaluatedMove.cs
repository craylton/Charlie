namespace Charlie.Moves
{
    public record EvaluatedMove
    {
        private readonly int initialMoveScoreEstimate;
        public Move Move { get; }
        public Score Score { get; set; } = Score.Draw;
        public int AlphaCount { get; set; } = 0;
        public int ScoreEstimate => AlphaCount * 2 + initialMoveScoreEstimate;

        public EvaluatedMove(Move move, int scoreEstimate)
        {
            Move = move;
            initialMoveScoreEstimate = scoreEstimate;
        }

        //public int CompareTo(EvaluatedMove other)
        //{
        //    var scoreComparsion = Score.CompareTo(other.Score);
        //    var alphaComparison = AlphaCount.CompareTo(other.AlphaCount);
        //    if (alphaComparison == 0)
        //        return scoreComparsion;
        //    else
        //        return alphaComparison;
        //}
    }
}
