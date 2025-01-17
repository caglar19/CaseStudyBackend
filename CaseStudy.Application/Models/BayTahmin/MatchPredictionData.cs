using Microsoft.ML.Data;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class MatchPredictionData
    {
        [LoadColumn(0)]
        public float HomeTeamRank { get; set; }

        [LoadColumn(1)]
        public float AwayTeamRank { get; set; }

        [LoadColumn(2)]
        public float HomeTeamForm { get; set; } // Son 5 maçtaki galibiyet yüzdesi

        [LoadColumn(3)]
        public float AwayTeamForm { get; set; }

        [LoadColumn(4)]
        public float HomeTeamGoalsScored { get; set; }

        [LoadColumn(5)]
        public float AwayTeamGoalsScored { get; set; }

        [LoadColumn(6)]
        public float HomeTeamGoalsConceded { get; set; }

        [LoadColumn(7)]
        public float AwayTeamGoalsConceded { get; set; }

        [LoadColumn(8)]
        public float H2HHomeWins { get; set; }

        [LoadColumn(9)]
        public float H2HAwayWins { get; set; }

        [LoadColumn(10)]
        public float HomeTeamInjuredPlayers { get; set; }

        [LoadColumn(11)]
        public float AwayTeamInjuredPlayers { get; set; }

        [LoadColumn(12)]
        public string Label { get; set; } // "HOME_WIN", "DRAW", "AWAY_WIN"
    }

    public class MatchPredictionOutput
    {
        [ColumnName("PredictedLabel")]
        public string PredictedResult { get; set; }

        public float[] Score { get; set; }
    }
}
