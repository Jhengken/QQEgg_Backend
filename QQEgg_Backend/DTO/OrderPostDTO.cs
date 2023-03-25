namespace QQEgg_Backend.DTO
{
    public class OrderPostDTO
    {

        public int OrderId { get; set; }
        public int? TradeNo { get; set; }
        public int? CustomerId { get; set; }       
        public int? ProductId { get; set; }             
        public DateTime OrderDate { get; set; }  
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }    
        public decimal? Price { get; set; }
        public int RoomId { get; set; }

    }
}
