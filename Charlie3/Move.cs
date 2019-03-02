using System.Collections.Generic;
using System.Linq;

namespace Charlie3
{
    public readonly struct Move
    {
        public ulong FromCell { get; }

        public ulong ToCell { get; }

        public bool IsEnPassant { get; }

        public bool IsCastle { get; }

        public bool IsDoublePush { get; }

        public PromotionType PromotionType { get; }

        public Move(ulong fromCell, ulong toCell,
            bool isEnPassant, bool isCastle, bool isDoublePush, PromotionType promotionType) =>
            (FromCell, ToCell, IsEnPassant, IsCastle, IsDoublePush, PromotionType) =
            (fromCell, toCell, isEnPassant, isCastle, isDoublePush, promotionType);

        public Move(ulong fromCell, ulong toCell) :
            this(fromCell, toCell, false, false, false, PromotionType.None)
        {
        }

        public override string ToString()
        {
            var from = Utils.CellNames[Utils.CountLeadingZeros(FromCell)];
            var to = Utils.CellNames[Utils.CountLeadingZeros(ToCell)];

            var promotion = string.Empty;

            if (PromotionType != PromotionType.None)
                promotion = "=" + Utils.PromotionSuffixes[(int)PromotionType];

            return $"{from}-{to}{promotion}";
        }

        public static Move FromString(IEnumerable<Move> possibleMoves, string move)
        {
            var from = new string(move.Take(2).ToArray());
            var to = new string(move.TakeLast(2).ToArray());

            ulong fromCell = 0, toCell = 0;
            for (int i = 0; i < Utils.CellNames.Length; i++)
            {
                if (from.ToUpper() == Utils.CellNames[i].ToUpper())
                    fromCell = 1ul << (63 - i);

                if (to.ToUpper() == Utils.CellNames[i].ToUpper())
                    toCell = 1ul << (63 - i);
            }

            var matches = possibleMoves.Where(m => m.FromCell == fromCell && m.ToCell == toCell);

            // If it was a pawn promotion, there will be multiple matching moves
            if (matches.Count() > 1 && move.Length == 5)
            {
                var promotion = move[4].ToString().ToUpper();
                return matches.FirstOrDefault(m => Utils.PromotionSuffixes[(int)m.PromotionType].ToString().ToUpper()== promotion);
            }

            return matches.FirstOrDefault();
        }
    }
}
