using System;

namespace Charlie
{
    public readonly struct Score
    {
        public readonly static Score Mate = new Score(1 << 20);
        public readonly static Score Infinity = new Score(1 << 24);
        public readonly static Score NegativeInfinity = -Infinity;
        public readonly static Score Draw = new Score(0);

        private readonly int score;

        public Score(int score) => this.score = score;

        public static Score operator +(Score score1, Score score2) => new Score(score1.score + score2.score);
        public static Score operator -(Score score1, Score score2) => new Score(score1.score - score2.score);
        public static Score operator *(Score score1, Score score2) => new Score(score1.score * score2.score);
        public static Score operator -(Score score1) => new Score(-score1.score);

        public static Score operator +(Score score1, int score2) => new Score(score1.score + score2);
        public static Score operator -(Score score1, int score2) => new Score(score1.score - score2);
        public static Score operator *(Score score1, int score2) => new Score(score1.score * score2);

        public static Score operator +(int score1, Score score2) => new Score(score1 + score2.score);
        public static Score operator -(int score1, Score score2) => new Score(score1 - score2.score);
        public static Score operator *(int score1, Score score2) => new Score(score1 * score2.score);

        public static bool operator >(Score score1, Score score2) => score1.score > score2.score;
        public static bool operator <(Score score1, Score score2) => score1.score < score2.score;
        public static bool operator >=(Score score1, Score score2) => score1.score >= score2.score;
        public static bool operator <=(Score score1, Score score2) => score1.score <= score2.score;

        public static bool operator >(int score1, Score score2) => score1 > score2.score;
        public static bool operator <(int score1, Score score2) => score1 < score2.score;
        public static bool operator >=(int score1, Score score2) => score1 >= score2.score;
        public static bool operator <=(int score1, Score score2) => score1 <= score2.score;

        public bool IsMateScore() => Math.Abs(score) > (Constants.MateScore - 100);
        public int PliesToMate() => (Mate - Math.Abs(score)).score;

        public bool IsPositive() => score > Draw;
        public bool IsNegative() => score < Draw;

        public override string ToString()
        {
            if (IsMateScore())
            {
                var prefix = IsNegative() ? "-" : "";
                var movesToMate = (PliesToMate() + 1) / 2;
                return "mate " + prefix + movesToMate;
            }

            return "cp " + score;
        }
    }
}
