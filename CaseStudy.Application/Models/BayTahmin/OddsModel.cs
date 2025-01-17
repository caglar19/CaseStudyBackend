namespace CaseStudy.Application.Models.BayTahmin
{
    public class OddsModel : BaseResponseModel
    {
        public int MatchId { get; set; }
        public string Bookmaker { get; set; }
        public float HomeWin { get; set; }
        public float Draw { get; set; }
        public float AwayWin { get; set; }

        public virtual MatchModel Match { get; set; }
    }
}
