using QQEgg_Backend.Models;

namespace QQEgg_Backend.DTO
{
    public class PsiteRoomDTO
    {
        public int? RoomId { get; set; }
        public int? productId { get; set; }
        public int? SiteId { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? HourPrice { get; set; }
        public int? DatePrice { get; set; }
        public int? Ping { get; set; }
        public string? Image { get; set; }
        public bool? Status { get; set; }
        public string? RoomDescription { get; set; }
        public string? OpenTime { get; set; }
        public string? RoomPassWord { get; set; }
        public string? Iframe { get; set; }
        public IFormFile? RoomPhoto { get; set; }
        public string? SiteName { get; internal set; }
    }
}
