using Microsoft.EntityFrameworkCore;
using Project_385.Models;

namespace Project_385.Repositery
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

    }
        public DbSet<BrandModels> Brands { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<ProductModels> Products { get; set; }

    }
}
