using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EvalComparisons.Stockfish
{
    public class StockfishWrapper : IDisposable
    {
        private readonly Process stockfishProcess = new Process();
        private readonly StockfishType stockfishType;
        private Analysis currentAnalysis;

        public StockfishWrapper(StockfishType stockfishType) => this.stockfishType = stockfishType;

        public void Start()
        {
            stockfishProcess.StartInfo.UseShellExecute = false;
            stockfishProcess.StartInfo.RedirectStandardOutput = true;
            stockfishProcess.StartInfo.RedirectStandardError = true;
            stockfishProcess.StartInfo.FileName = stockfishType.ExecutableLocation;

            stockfishProcess.StartInfo.RedirectStandardInput = true;

            stockfishProcess.OutputDataReceived += ProcessOutputDataHandler;
            stockfishProcess.ErrorDataReceived += ProcessErrorDataHandler;

            stockfishProcess.Start();
            stockfishProcess.BeginOutputReadLine();
            stockfishProcess.BeginErrorReadLine();
        }

        public void Start(int contempt)
        {
            Start();
            Send("setoption name Contempt value " + contempt);
        }

        public void EvaluateFen(Analysis analysis)
        {
            currentAnalysis = analysis;
            currentAnalysis.SearchType = SearchType.SinglePosition;

            Send("position fen " + currentAnalysis.Fen);
            if (currentAnalysis.Depth == 0)
            {
                Send("eval");
            }
            else
            {
                Send("go depth " + currentAnalysis.Depth);
            }
        }

        public void EvaluateFensFromFile(Analysis analysis, string fullFenFilename)
        {
            currentAnalysis = analysis;
            currentAnalysis.SearchType = SearchType.Bench;
            currentAnalysis.EvalScores = new List<int>();

            var benchType = analysis.Depth == 0 ? "eval" : "depth";
            var benchDepth = analysis.Depth == 0 ? 1 : analysis.Depth;

            Send($"bench 16 1 {benchDepth} {fullFenFilename} {benchType}");
        }

        public void Send(string command) => stockfishProcess.StandardInput.WriteLine(command);

        public void ClearHash() => Send("ucinewgame");

        public void Stop()
        {
            Send("quit");
            stockfishProcess.WaitForExit();
        }

        public StockfishWrapper GetDuplicate()
        {
            var filename = stockfishType.ExecutableLocation;

            int i = 0;
            string newFilename;
            do
            {
                newFilename = filename.Replace(".exe", i++ + ".exe");
            }
            while (File.Exists(newFilename));

            File.Copy(filename, newFilename);

            return new StockfishWrapper(stockfishType);
        }

        private static int GetScoreFromInfo(string info)
        {
            var words = info.Split(" ");
            int index = GetIndexOfCentipawnToken(words);
            return int.Parse(words[index]);
        }

        private static int GetIndexOfCentipawnToken(string[] words)
        {
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "cp")
                    return i + 1;
            }

            return -1;
        }

        private int GetScoreFromEval(string data)
        {
            var scoreString = data.Split(':')[1].Split('(')[0].Trim();
            var pawnsScore = double.Parse(scoreString);
            return (int)(pawnsScore * 100);
        }

        private void ProcessOutputDataHandler(object sendingProcess, DataReceivedEventArgs e)
        {
            var data = e.Data;

            if (data is null) return;

            if (data.Contains("Linscott")) return;

            if (currentAnalysis.SearchType == SearchType.SinglePosition)
            {
                if (data.Contains("mate"))
                    currentAnalysis.IsMate = true;

                if (data.StartsWith("info") && data.Contains(" cp "))
                {
                    currentAnalysis.CurrentScore = GetScoreFromInfo(data);
                    return;
                }

                if (data.StartsWith("bestmove"))
                {
                    var words = data.Split(" ");
                    currentAnalysis.BestMove = words[1];

                    currentAnalysis.IsComplete = true;
                    return;
                }

                if (data.Contains("Total evaluation: "))
                {
                    currentAnalysis.CurrentScore = GetScoreFromEval(data);
                    currentAnalysis.IsComplete = true;
                    return;
                }
            }
            else
            {
                if (data.StartsWith("info") && data.Contains(" cp "))
                {
                    currentAnalysis.CurrentScore = GetScoreFromInfo(data);
                    return;
                }

                if (data.StartsWith("bestmove"))
                {
                    var words = data.Split(" ");
                    currentAnalysis.BestMove = words[1];
                    currentAnalysis.EvalScores.Add(currentAnalysis.CurrentScore);
                    return;
                }

                if (data.Contains("Total evaluation: "))
                {
                    currentAnalysis.EvalScores.Add(GetScoreFromEval(data));
                    return;
                }
            }
        }


        private void ProcessErrorDataHandler(object sendingProcess, DataReceivedEventArgs e)
        {
            if (e.Data.Contains("============"))
                currentAnalysis.IsComplete = true;
            else if (!string.IsNullOrWhiteSpace(e.Data) && !e.Data.StartsWith("Position: "))
                Console.WriteLine(e.Data);
        }

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing) stockfishProcess.Dispose();
            disposed = true;
        }

        ~StockfishWrapper() => Dispose(false);

        #endregion
    }
}
