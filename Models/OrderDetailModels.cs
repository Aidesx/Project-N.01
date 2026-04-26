
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_385.Models
{
    public class OrderDetailModels
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
      
        public OrderModels Order { get; set; }
    }
}
