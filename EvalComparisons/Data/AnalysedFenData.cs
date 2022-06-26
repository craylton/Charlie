using PgnToFenCore;

namespace EvalComparisons.Data
{
    public class AnalysedFenData
    {
        public int Id { get; set; }
        public string Fen { get; set; }
        public int MoveNumber { get; set; }
        public FinalGameResult FinalResult { get; set; }
        public bool IsWhiteToMove { get; set; }
        public int TotalMaterialOnBoard { get; set; }
        public int AnalysisScore { get; set; }

        public AnalysedFenData() { }

        public static AnalysedFenData FromFenData(FenData fenData) =>
            new()
            {
                Id = 0,
                Fen = fenData.Fen,
                MoveNumber = fenData.MoveNumber,
                FinalResult = fenData.FinalResult,
                IsWhiteToMove = fenData.IsWhiteToMove,
                TotalMaterialOnBoard = fenData.TotalMaterialOnBoard,
                AnalysisScore = 0,
            };

        public AnalysedFenData StoreAnalysis(int score) =>
            new()
            {
                Id = 0,
                Fen = Fen,
                MoveNumber = MoveNumber,
                FinalResult = FinalResult,
                IsWhiteToMove = IsWhiteToMove,
                TotalMaterialOnBoard = TotalMaterialOnBoard,
                AnalysisScore = score,
            };
    }
}
