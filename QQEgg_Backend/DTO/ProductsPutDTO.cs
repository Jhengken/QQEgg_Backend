namespace QQEgg_Backend.DTO
{
    public class ProductsPutDTO
    {
        public int ProductId { get; set; }
        public int? SupplierId { get; set; }
        public string? Name { get; set; }

        
        public virtual ICollection<PsiteDTO> Psite { get; set; }
    }
}
