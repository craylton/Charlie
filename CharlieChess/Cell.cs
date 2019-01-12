namespace CharlieChess
{
    public class Cell
    {
        public byte File { get; }
        public byte Rank { get; }

        public Cell(byte file, byte rank) => (File, Rank) = (file, rank);
    }
}
