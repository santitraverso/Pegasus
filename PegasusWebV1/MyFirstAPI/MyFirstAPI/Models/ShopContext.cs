using Microsoft.EntityFrameworkCore;

namespace MyFirstAPI.Models
{
    public class ShopContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public ShopContext(DbContextOptions<ShopContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(a => a.Category)
                .HasForeignKey(c => c.CategoryId);

            modelBuilder.Seed();
        }
    }

}
