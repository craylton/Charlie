using Charlie3.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Charlie3
{
    public class Search
    {
        private readonly Timer timer;
        private bool cancel;
        private TreeNode bestNode = new TreeNode(default, default);
        private readonly Evaluator evaluator = new Evaluator();
        private readonly MoveGenerator generator = new MoveGenerator();

        public event EventHandler<MoveInfo> BestMoveChanged;
        public event EventHandler<Move> BestMoveFound;

        public Search()
        {
            timer = new Timer() { AutoReset = false };
            timer.Elapsed += (s, e) => cancel = true;
        }

        public async Task Start(BoardState currentBoard, int timeMs)
        {
            cancel = false;

            // Negative time means infinite search
            if (timeMs >= 0)
            {
                timer.Interval = timeMs;
                timer.Start();
            }

            var root = new TreeNode(default, default);
            var isWhite = currentBoard.ToMove == PieceColour.White;

            int depth = 1;
            while (!cancel)
            {
                var (pv, _) = await AlphaBeta(root, currentBoard, int.MinValue, int.MaxValue, depth, true);

                if (pv.Any())
                {
                    var eval = isWhite ? bestNode.Evaluation : -bestNode.Evaluation;
                    BestMoveChanged?.Invoke(this, new MoveInfo(bestNode.Depth, pv, eval, bestNode.IsMate));
                }

                if (bestNode.IsMate)
                {
                    cancel = true;
                    BestMoveChanged?.Invoke(this, new MoveInfo(bestNode.Depth, pv, 0, true));
                }

                depth++;
            }

            BestMoveFound?.Invoke(this, bestNode.Move);
        }

        public void Stop()
        {
            timer.Stop();
            cancel = true;
        }

        private async Task<(List<Move> PV, bool IsLegal)> AlphaBeta(TreeNode parent, BoardState boardState, int alpha, int beta, int depth, bool isRoot = false)
        {
            bool isWhite = boardState.ToMove == PieceColour.White;

            // Test if our opponent made a legal move to get here
            if (boardState.IsInCheck(isWhite ? PieceColour.Black : PieceColour.White))
            {
                parent.Evaluation = isWhite ? int.MaxValue : int.MinValue;
                return (new List<Move>(), false);
            }

            if (depth == 0)
            {
                // Evaluate this node
                parent.Evaluation = evaluator.Evaluate(boardState);
                return (new List<Move>(), true);
            }

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition())
            {
                parent.Evaluation = 0;
                return (new List<Move>(), true);
            }

            if (!parent.Children.Any())
            {
                // Generate child nodes if not already there
                var defaultEval = isWhite ? int.MinValue : int.MaxValue;
                parent.Children = generator.GeneratePseudoLegalMoves(boardState)
                                           .Select(m => new TreeNode(m, defaultEval)).ToList();
            }
            else
            {
                // Sort the move list in order of best to worst
                if (isWhite)
                    parent.Children = parent.Children.OrderByDescending(c => c.Evaluation).ToList();
                else
                    parent.Children = parent.Children.OrderBy(c => c.Evaluation).ToList();
            }

            var bestChild = parent.Children.FirstOrDefault();
            var bestPv = new List<Move>();
            if (isRoot) bestNode = bestChild;

            int i = 0;
            while (i < parent.Children.Count)
            {
                var (pv, legal) = await AlphaBeta(parent.Children[i], boardState.MakeMove(parent.Children[i].Move), alpha, beta, depth - 1);

                // Check if this was a legal move
                if (!legal)
                {
                    parent.Children.RemoveAt(i);
                    continue;
                }

                // Alpha beta cutoffs
                if (isWhite && parent.Children[i].Evaluation >= beta)
                {
                    parent.Evaluation = beta;
                    parent.Depth = parent.Children[i].Depth + 1;
                    parent.IsMate = parent.Children[i].IsMate;
                    return (bestPv, true);
                }
                if (!isWhite && parent.Children[i].Evaluation <= alpha)
                {
                    parent.Evaluation = alpha;
                    parent.Depth = parent.Children[i].Depth + 1;
                    parent.IsMate = parent.Children[i].IsMate;
                    return (bestPv, true);
                }

                if (cancel) return (bestPv, true);

                // Finding new best moves
                if (isWhite && parent.Children[i].Evaluation > alpha)
                {
                    alpha = parent.Children[i].Evaluation;
                    bestChild = parent.Children[i];
                    bestPv = pv;
                    bestPv.Insert(0, parent.Children[i].Move);
                    if (isRoot) bestNode = parent.Children[i];
                }
                if (!isWhite && parent.Children[i].Evaluation < beta)
                {
                    beta = parent.Children[i].Evaluation;
                    bestChild = parent.Children[i];
                    bestPv = pv;
                    bestPv.Insert(0, parent.Children[i].Move);
                    if (isRoot) bestNode = parent.Children[i];
                }

                i++;
            }

            // Test for checkmate / stalemate
            if (!parent.Children.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                {
                    parent.Evaluation = isWhite ? int.MinValue : int.MaxValue;
                    parent.IsMate = true;
                }
                else
                {
                    parent.Evaluation = 0;
                }
            }
            else
            {
                parent.Evaluation = isWhite ? alpha : beta;
                parent.Depth = bestChild.Depth + 1;
                parent.IsMate = bestChild.IsMate;
            }

            return (bestPv, true);
        }





        private async Task<bool> Quiesce(TreeNode parent, BoardState boardState, int alpha, int beta, int depth)
        {
            bool isWhite = boardState.ToMove == PieceColour.White;

            // Test if our opponent made a legal move to get here
            if (boardState.IsInCheck(isWhite ? PieceColour.Black : PieceColour.White))
            {
                parent.Evaluation = isWhite ? int.MaxValue : int.MinValue;
                return false;
            }

            var standPat = evaluator.Evaluate(boardState);

            if (depth == 0)
            {
                // Evaluate this node
                parent.Evaluation = standPat;
                return true;
            }

            // Alpha beta cutoff
            if ((isWhite && standPat >= beta) ||
                (!isWhite && standPat <= alpha))
            {
                parent.Evaluation = standPat;
                return true;
            }

            if (isWhite && standPat > alpha) alpha = standPat;
            if (!isWhite && standPat < beta) beta = standPat;

            // Test for 3-move repetition
            if (boardState.IsThreeMoveRepetition())
            {
                parent.Evaluation = 0;
                return true;
            }

            if (!parent.Children.Any())
            {
                // Generate child nodes if not already there
                var defaultEval = isWhite ? int.MinValue : int.MaxValue;
                parent.Children = generator.GeneratePseudoLegalMoves(boardState)
                                           .Select(m => new TreeNode(m, defaultEval)).ToList();
            }
            else
            {
                // Sort the move list in order of best to worst
                if (isWhite)
                    parent.Children = parent.Children.OrderByDescending(c => c.Evaluation).ToList();
                else
                    parent.Children = parent.Children.OrderBy(c => c.Evaluation).ToList();
            }

            var bestChild = parent.Children.FirstOrDefault();

            int i = 0;
            while (i < parent.Children.Count)
            {
                // We are only interested in captures here
                if (!parent.Children[i].Move.IsCapture(boardState)) continue;

                var legal = await Quiesce(parent.Children[i], boardState.MakeMove(parent.Children[i].Move), alpha, beta, depth - 1);

                // Check if this was a legal move
                if (!legal)
                {
                    parent.Children.RemoveAt(i);
                    continue;
                }

                if (isWhite && parent.Children[i].Evaluation > alpha)
                {
                    if (parent.Children[i].Evaluation >= beta)
                    {
                        parent.Evaluation = parent.Children[i].Evaluation;
                        return true;
                    }
                    alpha = parent.Children[i].Evaluation;
                }

                if (!isWhite && parent.Children[i].Evaluation < beta)
                {
                    if (parent.Children[i].Evaluation <= alpha)
                    {
                        parent.Evaluation = parent.Children[i].Evaluation;
                        return true;
                    }
                    beta = parent.Children[i].Evaluation;
                }

                if (cancel) return true;

                i++;
            }

            // Test for checkmate / stalemate
            if (!parent.Children.Any())
            {
                if (boardState.IsInCheck(boardState.ToMove))
                {
                    parent.Evaluation = isWhite ? int.MinValue : int.MaxValue;
                }
                else
                {
                    parent.Evaluation = 0;
                }
            }
            else
            {
                parent.Evaluation = isWhite ? alpha : beta;
            }
            return true;
        }
    }
}
