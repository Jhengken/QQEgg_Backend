using Newtonsoft.Json;
using System.Runtime.Serialization;
using QQEgg_Backend;

namespace QQEgg_Backend.DTO
{
    public class EvaluationDTO
    {
        [JsonIgnore]
        public int? EvaluationId { get; set; } 
        [JsonIgnore]
        public int? RoomID { get; set; } 

        public DateTime? Date { get; set; }
        public  string Title { get; set; }
        public string Description { get; set; }
        public string CustomerName { get; set; }
        public int? Star { get; set; }



    }
}
