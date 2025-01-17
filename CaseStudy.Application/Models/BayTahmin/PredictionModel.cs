namespace CaseStudy.Application.Models.BayTahmin
{
    public class PredictionModel : BaseResponseModel
    {
        public int MatchId { get; set; }
        public int? PredictedWinnerId { get; set; }
        public float WinProbability { get; set; }
        public float DrawProbability { get; set; }

        public virtual MatchModel Match { get; set; }
        public virtual TeamModel PredictedWinner { get; set; }
    }
}
