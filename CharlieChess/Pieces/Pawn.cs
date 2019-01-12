namespace CharlieChess.Pieces
{
    internal class Pawn : IPiece
    {
        public Cell CurrentCell { get; set; }

        public Colour Colour { get; set; }
    }
}
