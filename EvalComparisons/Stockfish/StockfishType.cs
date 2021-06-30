namespace EvalComparisons.Stockfish
{
    public abstract class StockfishType
    {
        public static StockfishType Official => new OfficialStockfish();
        public static StockfishType Test => new TestStockfish();

        public abstract string ExecutableLocation { get; }

        private class OfficialStockfish : StockfishType
        {
            public override string ExecutableLocation => @"F:\Documents\Simon\chess blah\Engines\Stockfish\HomeFish\Homefish 200412\Stockfish.exe";
        }

        private class TestStockfish : StockfishType
        {
            public override string ExecutableLocation => @"F:\Documents\Simon\chess blah\Engines\Stockfish\HomeFish\TestFish\Stockfish.exe";
        }
    }
}
