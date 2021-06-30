using EvalComparisons.Data;
using EvalComparisons.Filters;
using EvalComparisons.Graph;
using EvalComparisons.Stockfish;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Input;

namespace EvalComparisons
{
    public class MainViewModel : BindableBase
    {
        private readonly DataRetriever dataRetriever = new DataRetriever();
        private readonly GraphType graphType = new GraphType();

        private bool _hasAnalysed;
        private bool _isAnalysisRequired = true;
        private List<KeyValuePair<string, decimal>> _customGraphPoints;
        private ICommand _compareCommand;
        private ICommand _filterCommand;

        public string Filename { get; set; }

        public string ComparisonFilename { get; set; } = Constants.DefaultComparisonFilename;

        public bool HasAnalysed { get => _hasAnalysed; set => SetProperty(ref _hasAnalysed, value); }

        public bool IsAnalysisRequired
        {
            get => _isAnalysisRequired;
            set => SetProperty(ref _isAnalysisRequired, value);
        }

        public List<KeyValuePair<string, decimal>> CustomGraphPoints
        {
            get => _customGraphPoints;
            set => SetProperty(ref _customGraphPoints, value);
        }

        public ImprovementStatistics ImprovementStatistics { get; set; } = new ImprovementStatistics();

        public IEnumerable<Filter> Filters { get; set; } = new List<Filter>();

        public FilterStatistics FilterStatistics { get; set; } = new FilterStatistics();

        public IEnumerable<string> CustomGraphTypes => graphType.AllowedGraphTypes;

        public string CustomGraphType
        {
            get => graphType.TypeName;
            set
            {
                graphType.TypeName = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand CompareCommand =>
            _compareCommand ?? (_compareCommand = new RelayCommand(_ => Compare()));

        public ICommand FilterCommand =>
            _filterCommand ?? (_filterCommand = new RelayCommand(_ => Filter()));

        public string TruthFilePath => Constants.FileDirectory + Constants.TruthDataFilename;
        public string AnalysisFilePath => Constants.FileDirectory + Filename;
        public string ComparisonFilePath => Constants.FileDirectory + ComparisonFilename;

        private void Compare()
        {
            dataRetriever.GetTruthData(TruthFilePath);

            if (IsAnalysisRequired)
            {
                var benchAnalyser = new BenchAnalysis(StockfishType.Test);
                //var benchAnalyser = new BenchAnalysis(StockfishType.Official);
                benchAnalyser.PerformStaticBench(AnalysisFilePath, dataRetriever.Truths);
            }

            dataRetriever.GenerateImprovementData(AnalysisFilePath, ComparisonFilePath);
            GetImprovementStatistics(dataRetriever.Truths, dataRetriever.Improvements);
            Filter();

            HasAnalysed = true;
        }

        private void GetImprovementStatistics(AnalysedFenData[] truths, int[] improvements)
        {
            ImprovementStatistics.GenerateStatistics(truths, improvements);
            NotifyPropertyChanged(nameof(ImprovementStatistics));
        }

        private void Filter()
        {
            if (!dataRetriever.HasData) return;

            List<Point> dataPoints = GetFilteredDataPoints(Filters, dataRetriever);

            UpdateFilterStatistics(dataPoints);

            //var graph = new GraphEquation(dataPoints);
            //CustomGraphPoints = graph.GetGraphPoints(new[] { firstPoint, lastPoint });

            var graph2 = new GraphPoints(dataPoints);
            int[] points = graph2.GetGraphSegments(dataPoints, Constants.NumberOfGraphSegments);
            CustomGraphPoints = graph2.GetGraphPoints(points);
        }

        private void UpdateFilterStatistics(List<Point> filteredDataPoints)
        {
            FilterStatistics.PositionCount = filteredDataPoints.Count;

            if (filteredDataPoints.Count != 0)
                FilterStatistics.AverageImprovement = (decimal)filteredDataPoints.Average(dp => dp.Y);
            else
                FilterStatistics.AverageImprovement = 0;

            NotifyPropertyChanged(nameof(FilterStatistics));
        }

        private List<Point> GetFilteredDataPoints(IEnumerable<Filter> filters, DataRetriever dataRetriever)
        {
            var dataPoints = new List<Point>();
            for (int i = 0; i < dataRetriever.Truths.Length; i++)
            {
                var truthScore = dataRetriever.Truths[i].AnalysisScore;
                var improvement = dataRetriever.Improvements[i];
                var material = dataRetriever.Truths[i].TotalMaterialOnBoard;
                var xValue = graphType.GetGraphTypeValue(truthScore, material);

                if (filters.All(filter => filter.DoesPass(truthScore, improvement, material)))
                    dataPoints.Add(new Point(xValue, improvement));
            }

            return dataPoints;
        }
    }
}
