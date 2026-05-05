using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_385.Models
{
    public class WishListModels
    {
        [Key]
        public int Id { get; set; }
        public int ProductID { get; set; }
        public string UserId { get; set; }
        [ForeignKey("ProductID")]
        public ProductModels Product { get; set; }
    }
}
