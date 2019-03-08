namespace Charlie3
{
    public class MetaMove
    {
        public Move Move { get; }
        public bool IsCheck { get; set; }
        public bool IsCapture { get; set; }

        public MetaMove(Move move) => Move = move;

        public static MetaMove FromState(BoardState board, Move move)
        {
            var isWhite = board.ToMove == PieceColour.White;
            bool isCapture, isCheck;
            if (isWhite)
            {
                isCapture = (board.BitBoard.BlackPieces & move.ToCell) != 0;
                isCheck = board.IsInCheck(PieceColour.Black);
            }
            else
            {
                isCapture = (board.BitBoard.WhitePieces & move.ToCell) != 0;
                isCheck = board.IsInCheck(PieceColour.White);
            }

            return new MetaMove(move, isCheck, isCapture);
        }

        public MetaMove(Move move, bool isCheck, bool isCapture) =>
            (Move, IsCheck, IsCapture) = (move, isCheck, isCapture);
    }
}
