using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project_385.Models;

namespace Project_385.Repository
{
    public class DataContext : IdentityDbContext<AppUserModels>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

    }
        public DbSet<BrandModels> Brands { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<ProductModels> Products { get; set; }
        public DbSet<OrderModels> Orders { get; set; }
        public DbSet<OrderDetailModels> OrderDetails { get; set; }
        public DbSet<RoleModels> RolePermissions { get; set; }

        public DbSet<RatingModels> Ratings { get; set; }
        public DbSet<WishListModels> WishList { get; set; }
        public DbSet<CompareModels> CompareModels { get; set; }

    }
}
