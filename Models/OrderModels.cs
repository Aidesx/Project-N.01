using System.ComponentModel.DataAnnotations.Schema;

namespace Project_385.Models
{
    public class OrderModels
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string UserName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        public int Status { get; set; }
    }
}
