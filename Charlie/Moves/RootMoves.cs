using Charlie.BoardRepresentation;
using System.Collections.Generic;
using System.Linq;

namespace Charlie.Moves
{
    public class RootMoves : List<EvaluatedMove>
    {
        public void Generate(BoardState boardState, MoveGenerator moveGenerator)
        {
            var moves = moveGenerator.GenerateLegalMoves(boardState);

            foreach (Move move in moves)
            {
                var newBoard = boardState.MakeMove(move);

                var promise = 0;
                if (newBoard.IsInCheck(newBoard.ToMove))
                    promise += 5;
                if (move.IsCaptureOrPromotion(boardState))
                    promise += 3;
                if (move.IsCastle)
                    promise += 1;
                if (move.IsDoublePush)
                    promise += 1;

                Add(new EvaluatedMove(move, promise));
            }
        }

        public void SortByPromise()
        {
            var orderedMoves = this.OrderByDescending(evaluatedMove => evaluatedMove.Promise)
                .ThenByDescending(evaluatedMove => evaluatedMove.Score).ToList();

            Clear();
            foreach (var move in orderedMoves) Add(move);
        }
    }
}
