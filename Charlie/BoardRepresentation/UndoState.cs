namespace Charlie.BoardRepresentation;

public struct UndoState
{
    public long PreviousHash { get; init; }

    public byte PreviousCastleRules { get; init; }

    public ulong PreviousWhiteEnPassant { get; init; }

    public ulong PreviousBlackEnPassant { get; init; }

    public bool HadCapture { get; init; }

    public PieceType CapturedPiece { get; init; }

    public ulong CapturedSquare { get; init; }
}
