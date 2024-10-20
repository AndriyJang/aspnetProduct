using Microsoft.EntityFrameworkCore;
using WebApplication2.Data.Entities;

namespace WebApplication2.Data
{
    public class AndriyContexDb:DbContext
    {
        public AndriyContexDb(DbContextOptions<AndriyContexDb> options)
            : base(options) { }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<ProductImageEntity> ProductImages { get; set; }
    }
   
}
