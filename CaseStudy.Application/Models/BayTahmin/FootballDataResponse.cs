namespace CaseStudy.Application.Models.BayTahmin
{
    public class FootballDataResponse
    {
        public CountryModel Country { get; set; }
        public LeagueModel League { get; set; }
        public int Season { get; set; }
        public List<TeamModel> Teams { get; set; }

        public FootballDataResponse()
        {
            Teams = new List<TeamModel>();
        }
    }
}
