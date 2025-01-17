namespace CaseStudy.Application.Models.BayTahmin
{
    public class TransferModel : BaseResponseModel
    {
        public int PlayerId { get; set; }
        public int FromTeamId { get; set; }
        public int ToTeamId { get; set; }
        public DateTime TransferDate { get; set; }
        public decimal? Fee { get; set; }

        public virtual PlayerModel Player { get; set; }
        public virtual TeamModel FromTeam { get; set; }
        public virtual TeamModel ToTeam { get; set; }
    }
}
