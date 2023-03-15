using QQEgg_Backend.Abstract;

namespace QQEgg_Backend.DTO
{
    
    public class CustomersPUTDTO : CustomerAbstractDTOValidation
    {
        public int CustomerId { get; set; }
      
    }
}
