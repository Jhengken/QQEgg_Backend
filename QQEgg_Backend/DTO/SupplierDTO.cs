namespace QQEgg_Backend.DTO
{
    public class SupplierDTO
    {
        public int SupplierId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public int? CreditPoints { get; set; }
        public bool? BlackListed { get; set; }
    }
}
