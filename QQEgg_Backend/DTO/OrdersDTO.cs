namespace QQEgg_Backend.DTO
{
    public class OrdersDTO
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? ProductId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? TakeDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
