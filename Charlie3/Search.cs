using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    public class Search
    {
        private async Task<(Move Move, int Eval)> MiniMax(BoardState boardState, int depth)
        {
            if (depth == 0)
            {
                var evaluator = new Evaluator();
                return (default, evaluator.Evaluate(boardState));
            }

            var generator = new MoveGenerator();
            var moves = generator.GenerateLegalMoves(boardState).ToList();

            bool isWhite = boardState.ToMove == PieceColour.White;
            Move bestMove = default;
            int bestEval = isWhite ? int.MinValue : int.MaxValue;

            foreach (var move in moves)
            {
                var (_, eval) = await MiniMax(boardState.MakeMove(move), depth - 1);

                if ((isWhite && eval >= bestEval) || (!isWhite && eval <= bestEval))
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }

            return (bestMove, bestEval);
        }
                
        private async Task<(Move Move, int Eval)> AlphaBeta(BoardState boardState, int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                var evaluator = new Evaluator();
                return (default, evaluator.Evaluate(boardState));
            }

            var generator = new MoveGenerator();
            var moves = generator.GenerateLegalMoves(boardState).ToList();

            bool isWhite = boardState.ToMove == PieceColour.White;
            Move bestMove = default;
            int bestEval = isWhite ? int.MinValue : int.MaxValue;

            foreach (var move in moves)
            {
                var (_, eval) = await AlphaBeta(boardState.MakeMove(move), alpha, beta, depth - 1);

                if ((isWhite && eval >= bestEval) || (!isWhite && eval <= bestEval))
                {
                    bestEval = eval;
                    bestMove = move;
                }

                if (isWhite && eval > alpha) alpha = eval;
                if (!isWhite && eval < beta) beta = eval;

                if (alpha >= beta) break;
            }

            return (bestMove, bestEval);
        }

        public async Task<Move> FindBestMove(BoardState currentBoard)
        {
            var moveInfo = await MiniMax(currentBoard, 4);
            //var moveInfo = await AlphaBeta(currentBoard, int.MinValue, int.MaxValue, 4);
            return moveInfo.Move;
        }
    }
}
