namespace CaseStudy.Application.Models.BayTahmin
{
    public class PlayerModel : BaseResponseModel
    {
        public string Name { get; set; }
        public int TeamId { get; set; }
        public string Position { get; set; }
        public string Nationality { get; set; }
        public DateTime BirthDate { get; set; }
        public int ApiPlayerId { get; set; }
        
        public virtual TeamModel Team { get; set; }
        public virtual ICollection<TransferModel> Transfers { get; set; }
        public virtual ICollection<InjuryModel> Injuries { get; set; }
    }
}
