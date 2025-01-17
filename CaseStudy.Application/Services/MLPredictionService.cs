using Microsoft.ML;
using Microsoft.ML.Data;
using CaseStudy.Application.Models.BayTahmin;
using Microsoft.Extensions.Logging;
using CaseStudy.Application.Interfaces;

namespace CaseStudy.Application.Services
{
    public class MLPredictionService
    {
        private readonly MLContext _mlContext;
        private ITransformer _trainedModel;
        private readonly IBayTahminService _bayTahminService;
        private readonly ILogger<MLPredictionService> _logger;

        public MLPredictionService(IBayTahminService bayTahminService, ILogger<MLPredictionService> logger)
        {
            _mlContext = new MLContext(seed: 0);
            _bayTahminService = bayTahminService;
            _logger = logger;
        }

        public async Task TrainModel()
        {
            try
            {
                // Eğitim verilerini hazırla
                var trainingData = await PrepareTrainingData();
                
                // Veriyi ML.NET'in anlayacağı formata dönüştür
                var trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                // ML pipeline oluştur
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label")
                    .Append(_mlContext.Transforms.Concatenate("Features",
                        nameof(MatchPredictionData.HomeTeamRank),
                        nameof(MatchPredictionData.AwayTeamRank),
                        nameof(MatchPredictionData.HomeTeamForm),
                        nameof(MatchPredictionData.AwayTeamForm),
                        nameof(MatchPredictionData.HomeTeamGoalsScored),
                        nameof(MatchPredictionData.AwayTeamGoalsScored),
                        nameof(MatchPredictionData.HomeTeamGoalsConceded),
                        nameof(MatchPredictionData.AwayTeamGoalsConceded),
                        nameof(MatchPredictionData.H2HHomeWins),
                        nameof(MatchPredictionData.H2HAwayWins),
                        nameof(MatchPredictionData.HomeTeamInjuredPlayers),
                        nameof(MatchPredictionData.AwayTeamInjuredPlayers)))
                    .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Modeli eğit
                _trainedModel = pipeline.Fit(trainingDataView);

                // Modeli değerlendir
                var predictions = _trainedModel.Transform(trainingDataView);
                var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

                _logger.LogInformation($"Macro Accuracy: {metrics.MacroAccuracy}");
                _logger.LogInformation($"Micro Accuracy: {metrics.MicroAccuracy}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Model eğitimi sırasında hata oluştu");
                throw;
            }
        }

        public async Task<PredictionAnalysis> PredictMatch(int matchId)
        {
            try
            {
                var match = await _bayTahminService.GetMatchByIdAsync(matchId);
                var homeTeam = await _bayTahminService.GetTeamByIdAsync(match.HomeTeamId);
                var awayTeam = await _bayTahminService.GetTeamByIdAsync(match.AwayTeamId);

                // Tahmin için veriyi hazırla
                var predictionData = new MatchPredictionData
                {
                    HomeTeamRank = await GetTeamRank(homeTeam.Id),
                    AwayTeamRank = await GetTeamRank(awayTeam.Id),
                    HomeTeamForm = await CalculateTeamForm(homeTeam.Id),
                    AwayTeamForm = await CalculateTeamForm(awayTeam.Id),
                    HomeTeamGoalsScored = await GetTeamGoalsScored(homeTeam.Id),
                    AwayTeamGoalsScored = await GetTeamGoalsScored(awayTeam.Id),
                    HomeTeamGoalsConceded = await GetTeamGoalsConceded(homeTeam.Id),
                    AwayTeamGoalsConceded = await GetTeamGoalsConceded(awayTeam.Id),
                    H2HHomeWins = await GetH2HWins(homeTeam.Id, awayTeam.Id),
                    H2HAwayWins = await GetH2HWins(awayTeam.Id, homeTeam.Id),
                    HomeTeamInjuredPlayers = await GetInjuredPlayersCount(homeTeam.Id),
                    AwayTeamInjuredPlayers = await GetInjuredPlayersCount(awayTeam.Id)
                };

                // Tahmin yap
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<MatchPredictionData, MatchPredictionOutput>(_trainedModel);
                var prediction = predictionEngine.Predict(predictionData);

                // Analiz oluştur
                return new PredictionAnalysis
                {
                    MatchId = matchId,
                    HomeTeam = homeTeam.Name,
                    AwayTeam = awayTeam.Name,
                    PredictedResult = prediction.PredictedResult,
                    HomeWinProbability = prediction.Score[0],
                    DrawProbability = prediction.Score[1],
                    AwayWinProbability = prediction.Score[2],
                    AnalysisFactors = new List<AnalysisFactor>
                    {
                        new AnalysisFactor { Factor = "Form", Description = AnalyzeForm(predictionData.HomeTeamForm, predictionData.AwayTeamForm) },
                        new AnalysisFactor { Factor = "Goals", Description = AnalyzeGoals(predictionData) },
                        new AnalysisFactor { Factor = "H2H", Description = AnalyzeH2H(predictionData.H2HHomeWins, predictionData.H2HAwayWins) },
                        new AnalysisFactor { Factor = "Injuries", Description = AnalyzeInjuries(predictionData.HomeTeamInjuredPlayers, predictionData.AwayTeamInjuredPlayers) }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Maç tahmini sırasında hata oluştu. MatchId: {matchId}");
                throw;
            }
        }

        private async Task<List<MatchPredictionData>> PrepareTrainingData()
        {
            // Son 2 sezonun maç verilerini al
            var trainingData = new List<MatchPredictionData>();
            // Veri hazırlama mantığı...
            return trainingData;
        }

        private string AnalyzeForm(float homeForm, float awayForm)
        {
            var formDiff = homeForm - awayForm;
            if (formDiff > 0.2f)
                return "Ev sahibi takım son maçlarda daha iyi form gösteriyor.";
            else if (formDiff < -0.2f)
                return "Deplasman takımı son maçlarda daha iyi form gösteriyor.";
            else
                return "İki takım da benzer form düzeyinde.";
        }

        private string AnalyzeGoals(MatchPredictionData data)
        {
            var homeAttack = data.HomeTeamGoalsScored - data.AwayTeamGoalsConceded;
            var awayAttack = data.AwayTeamGoalsScored - data.HomeTeamGoalsConceded;

            if (homeAttack > 1 && awayAttack > 1)
                return "İki takım da ofansif güçlü, yüksek skorlu bir maç beklenebilir.";
            else if (homeAttack < 0 && awayAttack < 0)
                return "İki takım da defansif ağırlıklı, düşük skorlu bir maç beklenebilir.";
            else if (homeAttack > awayAttack)
                return "Ev sahibi takımın gol bulma potansiyeli daha yüksek.";
            else
                return "Deplasman takımının gol bulma potansiyeli daha yüksek.";
        }

        private string AnalyzeH2H(float homeWins, float awayWins)
        {
            if (homeWins > awayWins + 2)
                return "Ev sahibi takım geçmiş karşılaşmalarda üstünlük kurmuş durumda.";
            else if (awayWins > homeWins + 2)
                return "Deplasman takımı geçmiş karşılaşmalarda üstünlük kurmuş durumda.";
            else
                return "Geçmiş karşılaşmalarda dengeli bir görüntü var.";
        }

        private string AnalyzeInjuries(float homeInjuries, float awayInjuries)
        {
            var diff = homeInjuries - awayInjuries;
            if (diff > 2)
                return "Ev sahibi takımda önemli eksikler var.";
            else if (diff < -2)
                return "Deplasman takımında önemli eksikler var.";
            else
                return "İki takımda da benzer düzeyde eksikler var.";
        }

        // Yardımcı metodlar...
        private async Task<float> GetTeamRank(int teamId) => 0; // Implement
        private async Task<float> CalculateTeamForm(int teamId) => 0; // Implement
        private async Task<float> GetTeamGoalsScored(int teamId) => 0; // Implement
        private async Task<float> GetTeamGoalsConceded(int teamId) => 0; // Implement
        private async Task<float> GetH2HWins(int team1Id, int team2Id) => 0; // Implement
        private async Task<float> GetInjuredPlayersCount(int teamId) => 0; // Implement
    }

    public class PredictionAnalysis
    {
        public int MatchId { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string PredictedResult { get; set; }
        public float HomeWinProbability { get; set; }
        public float DrawProbability { get; set; }
        public float AwayWinProbability { get; set; }
        public List<AnalysisFactor> AnalysisFactors { get; set; }
    }

    public class AnalysisFactor
    {
        public string Factor { get; set; }
        public string Description { get; set; }
    }
}
