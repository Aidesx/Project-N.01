using System.ComponentModel.DataAnnotations;

namespace Project_385.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required (ErrorMessage = " Yêu cầu nhập Danh mục")]

        public string Name { get; set; }
        [Required (ErrorMessage = " Yêu cầu nhập mô tả")]

        public string Description { get; set; }

        public string Slug { get; set; } //link

        public int Status { get; set; }
    }
}
