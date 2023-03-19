namespace QQEgg_Backend.DTO
{
	public class CouponsDTO
	{
		public int CouponId { get; set; }
		public string Code { get; set; }
		public decimal? Discount { get; set; }
		public DateTime? ExpiryDate { get; set; }
	}
}
