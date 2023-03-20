using QQEgg_Backend.Models;

namespace QQEgg_Backend.DTO
{
    public class PsiteDTO
    {
        public int SiteId { get; set; }        
        public string Name { get; set; }
        public string? Image { get; set; }
        public string? OpenTime { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Address { get; set; }
        public string? SiteDescription { get; set; }
    
        public virtual ICollection<PsiteRoomDTO> PsiteRoom { get; set; }

       
    }
}
