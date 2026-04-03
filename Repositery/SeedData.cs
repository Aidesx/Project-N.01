using System.Linq;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;

namespace Project_385.Repositery
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {

            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                // seed categories (one-line initializers)
                CategoryModel Macbook = new CategoryModel { Name = "Macbook", Description = "Apple — MacBook laptops and accessories", Slug = "macbook", Status = 1 };
                CategoryModel GalaxyBook = new CategoryModel { Name = "GalaxyBook", Description = "Samsung — GalaxyBook laptops and accessories", Slug = "galaxybook", Status = 1 };

                // seed brands (one-line initializers)
                BrandModels AppleBrand = new BrandModels { Name = "Apple", Description = "Apple — designs and markets iPhone, iPad, Mac, Apple Watch and accessories; known for tight hardware-software integration and iOS/macOS ecosystems.", Slug = "apple", Status = 1 };
                BrandModels SamsungBrand = new BrandModels { Name = "Samsung", Description = "Samsung — global electronics leader offering Galaxy smartphones, tablets, GalaxyBook laptops, TVs and components; strong Android portfolio and hardware innovation.", Slug = "samsung", Status = 1 };

                // add categories and brands and persist to obtain IDs
                _context.Products.AddRange(
                    new ProductModels { Name = "Điện thoại iPhone 13 Pro Max", Description = "Điện thoại cao cấp", Price = 29990000, Brand = AppleBrand, Slug = "iphone-13-pro-max", Category = Macbook, Image = "iphone13promax.jpg" },
                    new ProductModels { Name = "Điện thoại Samsung Galaxy S21 Ultra", Description = "Điện thoại cao cấp", Price = 27990000, Brand = SamsungBrand, Slug = "samsung-galaxy-s21-ultra", Category = GalaxyBook, Image = "galaxys21ultra.jpg" }
                );

                _context.SaveChanges();
            }
        }
    }
}
