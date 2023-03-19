namespace QQEgg_Backend.DTO
{
    public class OrdersDTO
    {
        public int OrderId { get; set; }
        public int? TradeNo { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int RoomId { get; set; }
        public string? CategoryName { get; set; }
        public int? CouponId { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Price { get; set; }
    }
}
