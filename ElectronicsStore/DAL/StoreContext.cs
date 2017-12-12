using ElectronicsStore.Models;
using System.Data.Entity;

namespace ElectronicsStore.DAL
{
    public class StoreContext : DbContext
        {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductImageMapping> ProductImageMappings { get; set; }
    }
}