﻿using System.Numerics;

namespace Charlie.BoardRepresentation
{
    public class Evaluator
    {
        private const int pawn = 100, knight = 320, bishop = 330, rook = 500, queen = 900;

        private readonly int[] pawnPsqt = new[]
        {
            0,  0,  0,  0,  0,  0,  0,  0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5,  5,  10, 25, 25, 10, 5,  5,
            0,  0,  0,  20, 20,  0, 0,  0,
            5, -5, -10, 0,  0, -10,-5,  5,
            5, 10, 10, -20,-20, 10,10,  5,
            0,  0,  0,  0,  0,  0,  0,  0,
        };

        private readonly int[] knightPsqt = new[]
        {
            -50,-40,-30,-30,-30,-30,-40,-50,
            -40,-20,  0,  0,  0,  0,-20,-40,
            -30,  0, 10, 15, 15, 10,  0,-30,
            -30,  5, 15, 20, 20, 15,  5,-30,
            -30,  0, 15, 20, 20, 15,  0,-30,
            -30,  5, 10, 15, 15, 10,  5,-30,
            -40,-20,  0,  5,  5,  0,-20,-40,
            -50,-40,-30,-30,-30,-30,-40,-50,
        };

        private readonly int[] bishopPsqt = new[]
        {
            -20,-10,-10,-10,-10,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5, 10, 10,  5,  0,-10,
            -10,  5,  5, 10, 10,  5,  5,-10,
            -10,  0, 10, 10, 10, 10,  0,-10,
            -10, 10, 10, 10, 10, 10, 10,-10,
            -10,  5,  0,  0,  0,  0,  5,-10,
            -20,-10,-10,-10,-10,-10,-10,-20,
        };

        private readonly int[] rookPsqt = new[]
        {
             0,  0,  0,  0,  0,  0,  0,  0,
             5, 10, 10, 10, 10, 10, 10,  5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
            -5,  0,  0,  0,  0,  0,  0, -5,
             0,  0,  0,  5,  5,  0,  0,  0,
        };

        private readonly int[] queenPsqt = new[]
        {
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20,
        };

        private readonly int[] openingQueenPsqt = new[]
        {
            -20,-20,-20,-20,-20,-20,-20,-20,
            -20,-20,-20,-20,-20,-20,-20,-20,
            -30,-30,-30,-30,-30,-30,-30,-30,
            -33,-30,-30,-30,-30,-30,-30,-33,
            -29,-24,-19,-16,-16,-19,-24,-29,
             -5, -3,  0,  1,  1,  0, -3, -5,
              0,  2,  5,  7,  7,  5,  2,  0,
              2,  5, 11, 17, 17, 11,  5,  2,
        };

        private readonly int[] kingPsqt = new[]
        {
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -30,-40,-40,-50,-50,-40,-40,-30,
            -20,-30,-30,-40,-40,-30,-30,-20,
            -10,-20,-20,-20,-20,-20,-20,-10,
             20, 20,  0,  0,  0,  0, 20, 20,
             20, 30, 10,  0,  0, 10, 30, 20
        };

        private readonly int[] endgameKingPsqt = new[]
        {
            -10,-5,  0,  5,  5,  0,-5, -10,
            -5,  0,  5,  8,  8,  5, 0, -5,
             0,  5,  8, 10, 10,  8, 5,  0,
             4,  7, 11, 14, 14, 11, 7,  4,
             4,  7, 11, 14, 14, 11, 7,  4,
             0,  5,  8, 10, 10,  8, 5,  0,
            -5,  0,  5,  8,  8,  5, 0, -5,
            -10,-5,  0,  5,  5,  0,-5, -10,
        };

        public Score Evaluate(BoardState board)
        {
            Score whiteScore = Score.Draw, blackScore = Score.Draw;

            ulong whiteAttacks = 0ul, blackAttacks = 0ul;

            int whiteMaterial = GetWhiteMaterialCount(board);
            int blackMaterial = GetBlackMaterialCount(board);

            whiteScore += whiteMaterial;
            blackScore += blackMaterial;

            bool isOpening = whiteMaterial + blackMaterial >= 6200;
            bool isEndgame = whiteMaterial + blackMaterial <= 3600;

            ulong unoccupiedBb = ~board.Board.Occupied;

            if (board.IsInPseudoCheck(PieceColour.White)) whiteScore += 25;
            if (board.IsInPseudoCheck(PieceColour.Black)) blackScore += 25;

            for (int i = 0; i < 64; i++)
            {
                ulong thisSquare = 1ul << i;

                if (board.IsUnderAttack(thisSquare, PieceColour.White)) whiteAttacks |= thisSquare;
                if (board.IsUnderAttack(thisSquare, PieceColour.Black)) blackAttacks |= thisSquare;

                if ((unoccupiedBb & thisSquare) != 0) continue;

                // Assign values to each piece according to its position
                whiteScore += CalculateWhitePsqt(board, isOpening, isEndgame, i, thisSquare);
                blackScore += CalculateBlackPsqt(board, isOpening, isEndgame, i, thisSquare);
            }

            ulong whiteTerritory = whiteAttacks & ~blackAttacks;
            ulong blackTerritory = blackAttacks & ~whiteAttacks;

            //whiteScore -= CalculateKingDanger(board.BitBoard.WhiteKing, blackAttacks) * 15;
            //blackScore -= CalculateKingDanger(board.BitBoard.BlackKing, whiteAttacks) * 15;

            whiteScore += BitOperations.PopCount(whiteAttacks) * 5 + BitOperations.PopCount(whiteTerritory) * 5;
            blackScore += BitOperations.PopCount(blackAttacks) * 5 + BitOperations.PopCount(blackTerritory) * 5;

            // Hanging pieces are worth half value
            if (board.ToMove == PieceColour.Black)
            {
                whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhitePawn) * pawn / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteKnight) * knight / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteBishop) * bishop / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteRook) * rook / 2;
                whiteScore -= BitOperations.PopCount(blackTerritory & board.Board.WhiteQueen) * queen / 2;
            }
            else if (board.ToMove == PieceColour.White)
            {
                blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackPawn) * pawn / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackKnight) * knight / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackBishop) * bishop / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackRook) * rook / 2;
                blackScore -= BitOperations.PopCount(whiteTerritory & board.Board.BlackQueen) * queen / 2;
            }

            for (int i = 0; i < Chessboard.Files.Length; i++)
            {
                // Check for isolated pawns
                if ((board.Board.WhitePawn & Chessboard.Files[i]) != 0)
                {
                    bool isPawnToLeft = i != 0 && (board.Board.WhitePawn & Chessboard.Files[i - 1]) != 0;
                    bool IsPawnToRight = i != Chessboard.Files.Length - 1 &&
                                        (board.Board.WhitePawn & Chessboard.Files[i + 1]) != 0;

                    if (!isPawnToLeft && !IsPawnToRight) whiteScore -= 20;
                }

                if ((board.Board.BlackPawn & Chessboard.Files[i]) != 0)
                {
                    bool isPawnToLeft = i != 0 && (board.Board.BlackPawn & Chessboard.Files[i - 1]) != 0;
                    bool IsPawnToRight = i != Chessboard.Files.Length - 1 &&
                                        (board.Board.BlackPawn & Chessboard.Files[i + 1]) != 0;

                    if (!isPawnToLeft && !IsPawnToRight) blackScore -= 20;
                }

                // Check for doubled pawns
                if (BitOperations.PopCount(board.Board.WhitePawn & Chessboard.Files[i]) > 1) whiteScore -= 20;
                if (BitOperations.PopCount(board.Board.BlackPawn & Chessboard.Files[i]) > 1) blackScore -= 20;
            }

            return (whiteScore - blackScore) * (board.ToMove == PieceColour.White ? 1 : -1);
        }

        private int CalculateKingDanger(ulong king, ulong attacks)
        {
            ulong kingRing = GetNeighbours(king);
            return BitOperations.PopCount(kingRing & attacks);
        }

        private static ulong GetNeighbours(ulong centre)
        {
            ulong kingRing = centre;
            bool isOnAFile = (centre & Chessboard.AFile) != 0;
            bool isOnHFile = (centre & Chessboard.HFile) != 0;
            bool isOnFirstRank = (centre & Chessboard.Rank1) != 0;
            bool isOnEighthRank = (centre & Chessboard.Rank8) != 0;

            if (!isOnAFile)
            {
                kingRing |= centre << 1;

                if (!isOnFirstRank) kingRing |= centre << 9;
                if (!isOnEighthRank) kingRing |= centre >> 7;
            }

            if (!isOnHFile)
            {
                kingRing |= centre >> 1;

                if (!isOnFirstRank) kingRing |= centre << 7;
                if (!isOnEighthRank) kingRing |= centre >> 9;
            }

            kingRing |= centre << 8;
            kingRing |= centre >> 8;

            return kingRing;
        }

        private static int GetWhiteMaterialCount(BoardState board)
        {
            int whiteMaterial = 0;
            whiteMaterial += BitOperations.PopCount(board.Board.WhitePawn) * pawn;
            whiteMaterial += BitOperations.PopCount(board.Board.WhiteKnight) * knight;
            whiteMaterial += BitOperations.PopCount(board.Board.WhiteBishop) * bishop;
            whiteMaterial += BitOperations.PopCount(board.Board.WhiteRook) * rook;
            whiteMaterial += BitOperations.PopCount(board.Board.WhiteQueen) * queen;
            return whiteMaterial;
        }

        private static int GetBlackMaterialCount(BoardState board)
        {
            int blackMaterial = 0;
            blackMaterial += BitOperations.PopCount(board.Board.BlackPawn) * pawn;
            blackMaterial += BitOperations.PopCount(board.Board.BlackKnight) * knight;
            blackMaterial += BitOperations.PopCount(board.Board.BlackBishop) * bishop;
            blackMaterial += BitOperations.PopCount(board.Board.BlackRook) * rook;
            blackMaterial += BitOperations.PopCount(board.Board.BlackQueen) * queen;
            return blackMaterial;
        }

        private Score CalculateWhitePsqt(BoardState board, bool isOpening, bool isEndgame, int cellIndex, ulong thisSquare)
        {
            Score psqt = Score.Draw;

            if ((board.Board.WhitePawn & thisSquare) != 0) psqt += pawnPsqt[cellIndex];
            if ((board.Board.WhiteKnight & thisSquare) != 0) psqt += knightPsqt[cellIndex];
            if ((board.Board.WhiteBishop & thisSquare) != 0) psqt += bishopPsqt[cellIndex];
            if ((board.Board.WhiteRook & thisSquare) != 0) psqt += rookPsqt[cellIndex];
            if ((board.Board.WhiteQueen & thisSquare) != 0)
            {
                psqt += isOpening ? openingQueenPsqt[cellIndex] : queenPsqt[cellIndex];
            }
            if ((board.Board.WhiteKing & thisSquare) != 0)
            {
                psqt += isEndgame ? endgameKingPsqt[cellIndex] : kingPsqt[cellIndex];
            }

            return psqt;
        }

        private Score CalculateBlackPsqt(BoardState board, bool isOpening, bool isEndgame, int cellIndex, ulong thisSquare)
        {
            Score psqt = Score.Draw;

            if ((board.Board.BlackPawn & thisSquare) != 0) psqt += pawnPsqt[63 - cellIndex];
            if ((board.Board.BlackKnight & thisSquare) != 0) psqt += knightPsqt[63 - cellIndex];
            if ((board.Board.BlackBishop & thisSquare) != 0) psqt += bishopPsqt[63 - cellIndex];
            if ((board.Board.BlackRook & thisSquare) != 0) psqt += rookPsqt[63 - cellIndex];
            if ((board.Board.BlackQueen & thisSquare) != 0)
            {
                psqt += isOpening ? openingQueenPsqt[63 - cellIndex] : queenPsqt[63 - cellIndex];
            }
            if ((board.Board.BlackKing & thisSquare) != 0)
            {
                psqt += isEndgame ? endgameKingPsqt[63 - cellIndex] : kingPsqt[63 - cellIndex];
            }

            return psqt;
        }
    }
}