using Newtonsoft.Json;
using QQEgg_Backend.Models;

namespace QQEgg_Backend.DTO
{
    public class OrdersDTO
    {
        public int OrderId { get; set; }
        public string? TradeNo { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RoomId { get; set; }
        public int? CouponId { get; set; }
        public decimal? Discount { get; set; }
        public int? Price { get; set; }
       
        public string CategoryName { get; set; }
        [JsonIgnore]
        public virtual TCustomers Customer { get; set; }
        [JsonIgnore]
        public virtual TProducts Product { get; set; }
        [JsonIgnore]
        public virtual ICollection<TCorderDetail> TCorderDetail { get; set; }
    }
}
