using System;
using System.Text.Json.Serialization;

namespace CaseStudy.Application.Models.BayTahmin
{
    public class TransferModel
    {
        [JsonPropertyName("player")]
        public PlayerInfo Player { get; set; }

        [JsonPropertyName("update")]
        public DateTime Update { get; set; }

        [JsonPropertyName("transfers")]
        public TransferInfo[] Transfers { get; set; }
    }

    public class TransferInfo
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("teams")]
        public TransferTeams Teams { get; set; }
    }

    public class TransferTeams
    {
        [JsonPropertyName("in")]
        public TeamInfo In { get; set; }

        [JsonPropertyName("out")]
        public TeamInfo Out { get; set; }
    }
}
