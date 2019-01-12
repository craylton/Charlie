namespace CharlieChess
{
    internal class Charlie
    {
        public Position CurrentPosition { get; private set; }
        public Colour Colour { get; private set; }

        public Move StartMatch(Move firstMove)
        {
            Colour = firstMove.Exists ? Colour.Black : Colour.White;

            CurrentPosition = Position.GetStartingPosition();

            if (Colour == Colour.Black)
                CurrentPosition.ApplyMove(firstMove);

            return GetMove(CurrentPosition, Colour);
        }

        public Move GetMove(Position position, Colour colour)
        {
            return new Move(false);
        }
    }
}
