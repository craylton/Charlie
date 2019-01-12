namespace CharlieChess
{
    public class Move
    {
        public Cell From { get; }
        public Cell To { get; }

        public bool Exists { get; }

        public Move(Cell from, Cell to) : this(from, to, true) { }

        public Move(Cell from, Cell to, bool exists) => (From, To, Exists) = (from, to, exists);

        public Move(bool exists) : this(new Cell(0, 0), new Cell(0, 0), exists) { }
    }
}
