using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    public class Search
    {
        private async Task<(Move Move, int Eval)> AlphaBeta(BoardState boardState, int depth)
        {
            var evaluator = new Evaluator();
            var generator = new MoveGenerator();

            var moves = generator.GenerateLegalMoves(boardState).ToList();

            if (depth == 0) return (default, evaluator.Evaluate(boardState));

            bool isWhite = boardState.ToMove == PieceColour.White;
            Move bestMove = default;
            int bestEval = isWhite ? int.MinValue : int.MaxValue;

            foreach (var move in moves)
            {
                var (_, eval) = await AlphaBeta(boardState.MakeMove(move), depth - 1);

                if ((isWhite && eval >= bestEval) || (!isWhite && eval <= bestEval))
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }

            return (bestMove, bestEval);
        }

        public async Task<Move> FindBestMove(BoardState currentBoard)
        {
            var moveInfo = await AlphaBeta(currentBoard, 3);
            return moveInfo.Move;
        }
    }
}
