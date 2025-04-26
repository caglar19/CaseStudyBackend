using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace CaseStudy.Application.Models.Roulette
{
    public class RouletteData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        public string Name { get; set; } = "default";
        
        public List<int> Numbers { get; set; } = new List<int>();
    }
}
