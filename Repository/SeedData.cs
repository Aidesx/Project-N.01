using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;
using System.Linq;

namespace Project_385.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();

            if (!_context.Products.Any())
            {
                CategoryModel Macbook = new CategoryModel { Name = "Macbook", Description = "Apple — MacBook laptops and accessories", Slug = "macbook", Status = 1 };
                CategoryModel GalaxyBook = new CategoryModel { Name = "GalaxyBook", Description = "Samsung — GalaxyBook laptops and accessories", Slug = "galaxybook", Status = 1 };

                BrandModels AppleBrand = new BrandModels { Name = "Apple", Description = "Apple — designs and markets iPhone, iPad, Mac, Apple Watch and accessories", Slug = "apple", Status = 1 };
                BrandModels SamsungBrand = new BrandModels { Name = "Samsung", Description = "Samsung — global electronics leader offering Galaxy smartphones", Slug = "samsung", Status = 1 };

                _context.Products.AddRange(
                    new ProductModels { Name = "Điện thoại iPhone 13 Pro Max", Description = "Điện thoại cao cấp", Price = 29990000, Brand = AppleBrand, Slug = "iphone-13-pro-max", Category = Macbook, Image = "iphone13promax.jpg" },
                    new ProductModels { Name = "Điện thoại Samsung Galaxy S21 Ultra", Description = "Điện thoại cao cấp", Price = 27990000, Brand = SamsungBrand, Slug = "samsung-galaxy-s21-ultra", Category = GalaxyBook, Image = "galaxys21ultra.jpg" }
                );

                _context.SaveChanges();
            }

        }

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUserModels>>();

            if (await userManager.FindByNameAsync("admin") == null)
            {
                AppUserModels admin = new AppUserModels
                {
                    UserName = "admin",
                    Email = "admin@admin.com",
                    EmailConfirmed = true
                };

                IdentityResult result = await userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}