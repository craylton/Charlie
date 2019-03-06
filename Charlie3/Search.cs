using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    public class Search
    {
        private async Task<(Move Move, int Eval)> AlphaBetaWhite(BoardState boardState, int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                var evaluator = new Evaluator();
                return (default, evaluator.Evaluate(boardState));
            }

            var generator = new MoveGenerator();
            var moves = generator.GenerateLegalMoves(boardState).ToList();

            Move bestMove = moves.FirstOrDefault();

            foreach (var move in moves)
            {
                var (_, eval) = await AlphaBetaBlack(boardState.MakeMove(move), alpha, beta, depth - 1);

                if (eval >= beta) return (move, beta);
                if (eval > alpha)
                {
                    alpha = eval;
                    bestMove = move;
                }
            }

            return (bestMove, alpha);
        }

        private async Task<(Move Move, int Eval)> AlphaBetaBlack(BoardState boardState, int alpha, int beta, int depth)
        {
            if (depth == 0)
            {
                var evaluator = new Evaluator();
                return (default, evaluator.Evaluate(boardState));
            }

            var generator = new MoveGenerator();
            var moves = generator.GenerateLegalMoves(boardState).ToList();

            Move bestMove = moves.FirstOrDefault();

            foreach (var move in moves)
            {
                var (_, eval) = await AlphaBetaWhite(boardState.MakeMove(move), alpha, beta, depth - 1);

                if (eval <= alpha) return (move, alpha);
                if (eval < beta)
                {
                    beta = eval;
                    bestMove = move;
                }
            }

            return (bestMove, beta);
        }

        public async Task<Move> FindBestMove(BoardState currentBoard)
        {
            (Move Move, int Eval) moveInfo;
            if (currentBoard.ToMove == PieceColour.White)
                moveInfo = await AlphaBetaWhite(currentBoard, int.MinValue, int.MaxValue, 5);
            else
                moveInfo = await AlphaBetaBlack(currentBoard, int.MinValue, int.MaxValue, 5);

            return moveInfo.Move;
        }
    }
}
