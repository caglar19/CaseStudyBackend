namespace CaseStudy.Application.Models.BayTahmin
{
    public class MatchModel : BaseResponseModel
    {
        public int LeagueId { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public DateTime MatchDate { get; set; }
        public string Status { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public int ApiFixtureId { get; set; }

        public virtual LeagueModel League { get; set; }
        public virtual TeamModel HomeTeam { get; set; }
        public virtual TeamModel AwayTeam { get; set; }
        public virtual MatchStatisticsModel Statistics { get; set; }
        public virtual ICollection<PredictionModel> Predictions { get; set; }
        public virtual ICollection<OddsModel> Odds { get; set; }
    }
}
