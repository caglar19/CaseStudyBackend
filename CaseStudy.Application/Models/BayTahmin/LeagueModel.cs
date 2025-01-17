namespace CaseStudy.Application.Models.BayTahmin
{
    public class LeagueModel : BaseResponseModel
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int Season { get; set; }
        public int ApiLeagueId { get; set; }
        
        public virtual ICollection<TeamModel> Teams { get; set; }
        public virtual ICollection<MatchModel> Matches { get; set; }
    }
}
