namespace Charlie.Moves;

public static class PromotionTypeExtensions
{
    public static string GetSuffix(this PromotionType promotionType) => promotionType switch
    {
        PromotionType.Knight => "N",
        PromotionType.Bishop => "B",
        PromotionType.Rook => "R",
        PromotionType.Queen => "Q",
        _ => "?",
    };
}
