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
                    promise += 15;
                if (move.IsCaptureOrPromotion(boardState))
                    promise += 9;
                if (move.IsAdvancedPawnPush(boardState))
                    promise += 3;
                if (move.IsCastle)
                    promise += 3;
                if (move.IsDoublePush)
                    promise += 3;

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

        public int GetConfidence(Move bestMove)
        {
            if (Count == 1) return 1000000;

            var bestMovePromise = this.Single(rootMove => rootMove.Move == bestMove).Promise;
            var highestPromise = this.First().Promise;
            var secondHighestPromise = this.ElementAt(1).Promise;

            return bestMovePromise == highestPromise
                ? bestMovePromise - secondHighestPromise
                : 0;
        }
    }
}
