namespace Charlie.BoardRepresentation;

public interface IPositionState
{
    Board Board { get; }

    byte CastleRules { get; }

    ulong WhiteEnPassant { get; }

    ulong BlackEnPassant { get; }

    PieceColour ToMove { get; }

    long HashCode { get; }

    bool IsInCheck(PieceColour toMove);

    bool IsInPseudoCheck(PieceColour attacker);

    bool IsUnderAttack(ulong cell, PieceColour attacker);
}
