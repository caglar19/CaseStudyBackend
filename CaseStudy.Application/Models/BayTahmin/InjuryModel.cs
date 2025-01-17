namespace CaseStudy.Application.Models.BayTahmin
{
    public class InjuryModel : BaseResponseModel
    {
        public int PlayerId { get; set; }
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }

        public virtual PlayerModel Player { get; set; }
    }
}
