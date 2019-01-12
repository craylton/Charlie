using CharlieChess.Pieces;
using System.Collections.Generic;

namespace CharlieChess
{
    internal class Position
    {
        public IEnumerable<IPiece> Pieces { get; private set; }

        public Colour ColourToMove { get; set; }

        public List<Position> PreviousPositions { get; }

        private Position(IEnumerable<IPiece> pieces) => Pieces = pieces;

        public static Position GetStartingPosition() => new Position(new List<IPiece>
            {
                new Pawn { CurrentCell = new Cell(3, 1), Colour = Colour.White },
                new Pawn { CurrentCell = new Cell(3, 6), Colour = Colour.Black },

                new King { CurrentCell = new Cell(4, 0), Colour = Colour.White },
                new King { CurrentCell = new Cell(4, 7), Colour = Colour.Black },
            });

        public void ApplyMove(Move move)
        {
            foreach (var piece in Pieces)
            {
                if (piece.CurrentCell == move.From)
                {
                    piece.CurrentCell = move.To;
                    break;
                }
            }
        }
    }
}
