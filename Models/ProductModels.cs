using Project_385.Repositery.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Project_385.Models
{
    public class ProductModels
    {
        
        [Key]
        public int Id { get; set; }

        [Required, MinLength(4, ErrorMessage = " Yêu cầu nhập Sản phẩm")]
        public string Name { get; set; }
        public string Slug { get; set; } //link
        [Required, MinLength(4, ErrorMessage = " Yêu cầu nhập mô tả Sản phẩm")]

        public string Description { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập giá Sản phẩm")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn 0")] 
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required,Range(1,int.MaxValue,ErrorMessage ="Yêu cầu chọn nhãn hàng")]
        public int BrandID { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Yêu cầu chọn loại hàng")]
        public int CategoryID { get; set; }

        public string Image { get; set; } = "default.png"; // add ảnh mặc định
        
        public CategoryModel Category { get; set; }
        
        public BrandModels Brand { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpLoad { get; set; }
    }
}
