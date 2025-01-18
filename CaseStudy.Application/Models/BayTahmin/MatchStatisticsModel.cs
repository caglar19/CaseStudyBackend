namespace CaseStudy.Application.Models.BayTahmin
{
    public class MatchStatisticsModel : BaseResponseModel
    {
        public int MatchId { get; set; }
        public int TeamId { get; set; }
        public int ShotsOnGoal { get; set; }
        public int ShotsOffGoal { get; set; }
        public float Possession { get; set; }
        public int Corners { get; set; }
        public int Fouls { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }

        public virtual TeamModel Team { get; set; }
    }
}
