namespace CharlieChess.Pieces
{
    internal interface IPiece
    {
        Cell CurrentCell { get; set; }

        Colour Colour { get; set; }
    }
}
