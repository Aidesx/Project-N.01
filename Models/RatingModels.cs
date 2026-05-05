using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_385.Models
{
    public class RatingModels
    {
        [Key]   
        public int Id { get; set; }
        [Required(ErrorMessage ="Yêu cầu nhập tên sản phẩm")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập đánh giá")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên email")]
        public string Email { get; set; }
        public string Rating { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]

        public ProductModels Product { get; set; }
    }
}
