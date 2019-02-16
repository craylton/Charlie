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
                promotion = "=" + PromotionType.ToString()[0].ToString();

            return $"{from}-{to}{promotion}";
        }
    }
}
