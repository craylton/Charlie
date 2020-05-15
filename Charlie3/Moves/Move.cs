using Charlie.Board;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Charlie.Moves
{
    public readonly struct Move
    {
        public ulong FromCell { get; }

        public ulong ToCell { get; }

        public bool IsEnPassant { get; }

        public bool IsCastle { get; }

        public bool IsDoublePush { get; }

        public PromotionType PromotionType { get; }

        public Move(
            ulong fromCell,
            ulong toCell,
            bool isEnPassant,
            bool isCastle,
            bool isDoublePush,
            PromotionType promotionType) =>
            (FromCell, ToCell, IsEnPassant, IsCastle, IsDoublePush, PromotionType) =
            (fromCell, toCell, isEnPassant, isCastle, isDoublePush, promotionType);

        public Move(ulong fromCell, ulong toCell) :
            this(fromCell, toCell, false, false, false, PromotionType.None)
        {
        }

        public override string ToString()
        {
            string from = Chessboard.CellNames[FromCell.CountLeadingZeros()];
            string to = Chessboard.CellNames[ToCell.CountLeadingZeros()];

            var promotion = string.Empty;

            if (PromotionType != PromotionType.None)
                promotion = "=" + PromotionType.GetSuffix();

            return $"{from}-{to}{promotion}";
        }

        public static Move FromString(IEnumerable<Move> possibleMoves, string move)
        {
            var from = new string(move.Take(2).ToArray());
            var to = new string(new string(move.Take(4).ToArray()).TakeLast(2).ToArray());

            ulong fromCell = 0, toCell = 0;

            for (int i = 0; i < Chessboard.CellNames.Length; i++)
            {
                if (from.ToUpper() == Chessboard.CellNames[i].ToUpper())
                    fromCell = 1ul << (63 - i);

                if (to.ToUpper() == Chessboard.CellNames[i].ToUpper())
                    toCell = 1ul << (63 - i);
            }

            IEnumerable<Move> matches = possibleMoves.Where(m => m.FromCell == fromCell && m.ToCell == toCell);

            // If it was a pawn promotion, there will be multiple matching moves
            if (matches.Count() > 1 && move.Length == 5)
            {
                string promotion = move[4].ToString().ToUpper();
                return matches.FirstOrDefault(m => m.PromotionType.GetSuffix().ToUpper() == promotion);
            }

            return matches.FirstOrDefault();
        }

        public bool IsCapture(BoardState board) => (board.BitBoard.Occupied & ToCell) != 0;

        public bool IsCaptureOrPromotion(BoardState board) =>
            IsCapture(board) || PromotionType != PromotionType.None;

        public bool IsValid() => !Equals(default(Move));

        public override bool Equals(object obj) =>
            obj is Move move &&
            FromCell == move.FromCell &&
            ToCell == move.ToCell &&
            IsEnPassant == move.IsEnPassant &&
            IsCastle == move.IsCastle &&
            IsDoublePush == move.IsDoublePush &&
            PromotionType == move.PromotionType;

        public override int GetHashCode() =>
            HashCode.Combine(FromCell, ToCell, IsEnPassant, IsCastle, IsDoublePush, PromotionType);
    }
}
