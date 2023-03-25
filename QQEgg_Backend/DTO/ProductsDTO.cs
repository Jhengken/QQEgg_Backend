using QQEgg_Backend.Models;

namespace QQEgg_Backend.DTO
{
    public class ProductsDTO
    {
        public int ProductId { get; set; }
        public int? SupplierId { get; set; }
        public string Name { get; set; }


        public virtual SupplierDTO Supplier { get; set; }      
        public virtual ICollection<PsiteDTO>  Psite { get; set; }
    }
}
