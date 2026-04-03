using System.ComponentModel.DataAnnotations;

namespace Project_385.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required,MinLength(4,ErrorMessage = " Yêu cầu nhập Danh mục")]

        public string Name { get; set; }
        [Required, MinLength(4, ErrorMessage = " Yêu cầu nhập mô tả")]

        public string Description { get; set; }
        [Required]

        public string Slug { get; set; } //link
        [Required]

        public int Status { get; set; }
    }
}
