using System.ComponentModel.DataAnnotations;

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
        [Required, MinLength(4, ErrorMessage = " Yêu cầu nhập giá Sản phẩm")]

        public decimal Price { get; set; }


        public int BrandID { get; set; }

        public int CategoryID { get; set; }

        public string Image { get; set; }

        public CategoryModel Category { get; set; }

        public BrandModels Brand { get; set; }

    }
}
