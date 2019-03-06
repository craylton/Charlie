using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Charlie3
{
    public class Search
    {
        public event EventHandler<MoveInfo> MoveInfoChanged;

        private async Task<(Move Move, int Eval)> AlphaBeta(BoardState boardState, int alpha, int beta, int depth, bool isRoot = false)
        {
            if (depth == 0)
            {
                var evaluator = new Evaluator();
                return (default, evaluator.Evaluate(boardState));
            }

            if (boardState.IsThreeMoveRepetition()) return (default, 0);

            var generator = new MoveGenerator();
            var moves = generator.GenerateLegalMoves(boardState);

            Move bestMove = moves.FirstOrDefault();
            bool isWhite = boardState.ToMove == PieceColour.White;

            foreach (var move in moves)
            {
                var (_, eval) = await AlphaBeta(boardState.MakeMove(move), alpha, beta, depth - 1);

                if (isWhite && eval >= beta) return (move, beta);
                if (!isWhite && eval <= alpha) return (move, alpha);

                if (isWhite && eval > alpha)
                {
                    alpha = eval;
                    bestMove = move;
                    if (isRoot) MoveInfoChanged?.Invoke(this, new MoveInfo(depth, new List<Move> { move }, eval));
                }

                if (!isWhite && eval < beta)
                {
                    beta = eval;
                    bestMove = move;
                    if (isRoot) MoveInfoChanged?.Invoke(this, new MoveInfo(depth, new List<Move> { move }, -eval));
                }
            }

            return (bestMove, isWhite ? alpha : beta);
        }

        public async Task<Move> FindBestMove(BoardState currentBoard)
        {
            var moveInfo = await AlphaBeta(currentBoard, int.MinValue, int.MaxValue, 5, true);
            return moveInfo.Move;
        }
    }
}
