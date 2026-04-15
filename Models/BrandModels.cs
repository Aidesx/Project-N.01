using System.ComponentModel.DataAnnotations;

namespace Project_385.Models
{
    public class BrandModels
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = " Yêu cầu nhập Thương hiệu")]
        public string Name { get; set; }
        [Required(ErrorMessage = " Yêu cầu nhập mô tả Thương hiệu")]

        public string Description { get; set; }

        public string Slug { get; set; } //link
        public int Status { get; set; }
    }
}
