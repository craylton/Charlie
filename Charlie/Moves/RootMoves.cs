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

                var scoreEstimate = 0;
                if (newBoard.IsInCheck(newBoard.ToMove))
                    scoreEstimate += 5;
                if (move.IsCaptureOrPromotion(boardState))
                    scoreEstimate += 3;
                if (move.IsCastle)
                    scoreEstimate += 1;
                if (move.IsDoublePush)
                    scoreEstimate += 1;

                Add(new EvaluatedMove(move, scoreEstimate));
            }
        }

        public void SortByStrength()
        {
            var orderedMoves = this.OrderByDescending(evaluatedMove => evaluatedMove.Score)
                .ThenByDescending(evaluatedMove => evaluatedMove.AlphaCount).ToList();

            Clear();
            foreach (var move in orderedMoves) Add(move);
        }

        public void SortByPromise()
        {
            var orderedMoves = this.OrderByDescending(evaluatedMove => evaluatedMove.ScoreEstimate)
                .ThenByDescending(evaluatedMove => evaluatedMove.Score).ToList();

            Clear();
            foreach (var move in orderedMoves) Add(move);
        }
    }
}
