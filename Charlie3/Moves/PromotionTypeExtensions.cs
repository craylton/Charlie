namespace Charlie.Moves
{
    public static class PromotionTypeExtensions
    {
        public static char GetSuffix(this PromotionType promotionType) => promotionType switch
        {
            PromotionType.Knight => 'N',
            PromotionType.Bishop => 'N',
            PromotionType.Rook => 'N',
            PromotionType.Queen => 'N',
            _ => '?',
        };
    }
}
