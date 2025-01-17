namespace CaseStudy.Application.Models.BayTahmin
{
    public class TeamModel : BaseResponseModel
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Founded { get; set; }
        public int ApiTeamId { get; set; }
        public string LogoUrl { get; set; }
        public int LeagueId { get; set; }
        
        public virtual LeagueModel League { get; set; }
        public virtual ICollection<PlayerModel> Players { get; set; }
        public virtual ICollection<MatchModel> HomeMatches { get; set; }
        public virtual ICollection<MatchModel> AwayMatches { get; set; }
        public virtual ICollection<MatchStatisticsModel> Statistics { get; set; }
    }
}
