using Microsoft.EntityFrameworkCore;

namespace userinterface.Models
{
    public class MemoryDbContext : DbContext
    {
        public MemoryDbContext(DbContextOptions<MemoryDbContext> options) : base(options) { }

        public DbSet<FinreflectkgFull> FinreflectkgFull { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinreflectkgFull>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("finreflectkg_full");
            });
        }
    }
}
