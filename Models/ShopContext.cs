using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HPlusSport.API.Models
{
    public class ShopContext:DbContext
    {
        public ShopContext(DbContextOptions<ShopContext> options): base(options)
        {}

        //we need to tell EF the relationship between these models.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasMany(c => c.Products).WithOne(a => a.Category).HasForeignKey(a=>a.CategoryId); //a category has several product
            modelBuilder.Entity<Order>().HasMany(o => o.Products);
            modelBuilder.Entity<Order>().HasOne(o => o.User);
            modelBuilder.Entity<User>().HasMany(u => u.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId);
            modelBuilder.Seed(); //Seed method fills the data store with sample data
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }
    
    
    }
}
