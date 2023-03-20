using QQEgg_Backend.Models;

namespace QQEgg_Backend.DTO
{
    public class CouponDTO
    {
        public int CouponId { get; set; }
        public string Code { get; set; }
        public decimal? Discount { get; set; }
        public int? Quantity { get; set; }
        public bool? Available { get; set; }
        public int? HowPoint { get; set; }

    }
}
